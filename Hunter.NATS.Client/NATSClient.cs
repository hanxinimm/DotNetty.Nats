using DotNetty.Codecs.NATS;
using DotNetty.Codecs.NATS.Packets;
using DotNetty.Codecs.NATSJetStream;
using DotNetty.Handlers.NATS;
using DotNetty.Transport.Bootstrapping;
using DotNetty.Transport.Channels;
using DotNetty.Transport.Channels.Sockets;
using Hunter.NATS.Client.Handlers;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Polly;
using Polly.Timeout;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Hunter.NATS.Client
{
    public partial class NATSClient : IAsyncDisposable
    {
        private readonly ILogger _logger;

        /// <summary>
        /// NATS配置
        /// </summary>
        private readonly NATSOptions _options;

        /// <summary>
        /// 客户端编号
        /// </summary>
        private readonly string _clientId;

        /// <summary>
        /// 客户端标识
        /// </summary>
        public readonly string _identity;

        /// <summary>
        /// 消息应答收件箱
        /// </summary>
        private readonly string _replyInboxId;

        /// <summary>
        /// JetStream 流设置
        /// </summary>
        private readonly JsonSerializerSettings _jetStreamSetting;

        /// <summary>
        /// 通道引导
        /// </summary>
        private readonly Bootstrap _bootstrap;

        ///// <summary>
        ///// 连接通道实例
        ///// </summary>
        private IChannel _channel;

        /// <summary>
        /// 订阅编号
        /// </summary>
        private int _subscribeId;

        /// <summary>
        /// 限制并发线程
        /// </summary>
        private readonly ManualResetEventSlim _autoResetEvent;

        /// <summary>
        /// 连接状态
        /// </summary>
        private NATSConnectionState _connectionState;

        /// <summary>
        /// 连接配置
        /// </summary>
        private InfoPacket _info;

        /// <summary>
        /// 等待发送消息确认安排表
        /// </summary>
        private TaskCompletionSource<InfoPacket> _infoTaskCompletionSource;

        /// <summary>
        /// 订阅消息处理器集合
        /// </summary>
        private readonly List<SubscriptionMessageHandler> _subscriptionMessageHandler;

        /// <summary>
        /// 连接故障策略
        /// </summary>
        private readonly AsyncPolicy _connectPolicy;

        /// <summary>
        /// 执行故障策略
        /// </summary>
        private readonly AsyncPolicy _policy;

        private static readonly Regex _publishSubjectRegex = new Regex(@"^[a-zA-Z\d]+(\.(\*|\>|[a-zA-Z\d]+))*$", RegexOptions.Compiled);
        private static readonly Regex _subscribeSubjectRegex = new Regex(@"^[a-zA-Z\d]+(\.(\*|\>|[a-zA-Z\d]+))*$", RegexOptions.Compiled);

        public NATSClient(
            ILogger<NATSClient> logger,
            NATSOptions options)
        {
            _options = options;
            _identity = Guid.NewGuid().ToString("n");
            _clientId = $"{_options.ClientId}-{_identity}";
            _replyInboxId = _identity;
            _infoTaskCompletionSource = new TaskCompletionSource<InfoPacket>();
            _subscriptionMessageHandler = new List<SubscriptionMessageHandler>();
            _consumerMessageHandler = new List<ConsumerMessageHandler>();

            _bootstrap = InitBootstrap();
            _logger = logger;

            _jetStreamSetting = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                ContractResolver = new IgnoreEmptyEnumerablesResolver()
            };

            #region Connect;

            //重试
            var connectPolicyRetry = Policy
                .Handle<ConnectException>()
                .Or<SocketException>()
                .Or<TimeoutException>()
                .WaitAndRetryForeverAsync(
                    (retryAttempt) =>
                {
                    logger.LogWarning($"重试连接Stan客户端 客户端标识{_clientId} 第 {retryAttempt} 次尝试");
                    return TimeSpan.FromSeconds(retryAttempt);
                },
                (ex, retrySecond, context) =>
                {
                    logger.LogError(ex, $"连接Stan客户端异常 客户端标识{_clientId}  将在 {retrySecond} 秒后重试");
                });

            //短路保护
            var connectPolicyBreaker = Policy
                .Handle<ConnectException>()
                .Or<TimeoutException>()
                .Or<SocketException>()
                .CircuitBreakerAsync(5, TimeSpan.FromSeconds(30));

            _connectPolicy = Policy.WrapAsync(connectPolicyRetry, connectPolicyBreaker);

            #endregion;

            #region Execute;

            //重试
            var policyRetry = Policy
                .Handle<TimeoutRejectedException>()
                .Or<TimeoutException>()
                .WaitAndRetryAsync(3,
                    (retryAttempt, context) =>
                {
                    logger.LogWarning($"重试执行Stan客户端命令 客户端标识{_clientId} 第 {retryAttempt} 次尝试");
                    return TimeSpan.FromSeconds(retryAttempt);
                },
                (ex, retryAttempt, retrySecond, context) =>
                {
                    logger.LogError(ex, $"第 {retryAttempt}次 重新执行当前命令 客户端标识{_clientId} 操作 {context["hld"]} 主题 {context["sub"]} 将在 {retrySecond} 秒后重试");
                });

            //超时
            var policyTimeout = Policy.TimeoutAsync(10, TimeoutStrategy.Pessimistic);

            //短路保护
            var policyBreaker = Policy.Handle<TimeoutRejectedException>()
                    .CircuitBreakerAsync(50, TimeSpan.FromSeconds(15))
                    .WrapAsync(policyTimeout);

            _policy = Policy.WrapAsync(policyRetry, policyBreaker);

            #endregion;
        }

        public NATSClient(
            ILogger<NATSClient> logger,
            IOptions<NATSOptions> options) : this(logger, options.Value)
        {
            
        }

        public NATSConnectionState ConnectionState => _connectionState;

        private Bootstrap InitBootstrap()
        {
            return new Bootstrap()
                .Group(new MultithreadEventLoopGroup())
                .Channel<TcpSocketChannel>()
                .Option(ChannelOption.TcpNodelay, false)
                .Handler(new ActionChannelInitializer<ISocketChannel>(channel =>
                {
                    channel.Pipeline.AddLast("NATSJetStreamEncoder", NATSJetStreamEncoder.Instance);
                    channel.Pipeline.AddLast("NATSJetStreamDecoder", new NATSJetStreamDecoder(_logger));
                    channel.Pipeline.AddLast("Reconnect", new ReconnectChannelHandler(_logger, ReconnectIfNeedAsync));
                    channel.Pipeline.AddLast("Ping", new PingPacketHandler(_logger));
                    channel.Pipeline.AddLast("Pong", new PongPacketHandler(_logger));
                    channel.Pipeline.AddLast("OK", new OKPacketHandler(_logger));
                    channel.Pipeline.AddLast("INFO", new InfoPacketHandler(_logger, _infoTaskCompletionSource));
                    channel.Pipeline.AddLast("Error", new ErrorPacketHandler(_logger));
                }));
        }

        private void ReconnectIfNeedAsync(EndPoint socketAddress)
        {
            _logger.LogInformation("NATS连接端口 开始实例化新的连接管道");

            _channel = null;

            _autoResetEvent.Set();

            _logger.LogInformation("NATS连接端口 完成实例化新的连接管道");
        }

        async ValueTask<IChannel> ChannelConnectAsync(TimeSpan? timeout = null)
        {
            if (_channel != null && _channel.Active)
                return _channel;

            if (timeout.HasValue)
            {
                var receivesSignal = _autoResetEvent.Wait(timeout.Value);
                if (!receivesSignal) return null;
            }
            else
            {
                _autoResetEvent.Wait();
            }

            _autoResetEvent.Reset();

            if (_channel != null && _channel.Active)
            {
                _autoResetEvent.Set();
                return _channel;
            }

            _channel = await ConnectAsync();

            _autoResetEvent.Set();

            return _channel;
        }
    }
}
