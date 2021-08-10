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
        private IChannel _embed_channel;

        /// <summary>
        /// 订阅编号
        /// </summary>
        private int _subscribeId;

        /// <summary>
        /// 限制并发线程
        /// </summary>
        private readonly AutoResetEvent _autoResetEvent;

        /// <summary>
        /// 连接状态
        /// </summary>
        private NATSConnectionState _connectionState;

        /// <summary>
        /// 连接配置
        /// </summary>
        private InfoPacket _info;

        /// <summary>
        /// 连接配置任务
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

        /// <summary>
        /// 心跳服务定时器
        /// </summary>
        private readonly System.Timers.Timer _pingTimer;

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
            _autoResetEvent = new AutoResetEvent(true);
            _pingTimer = new System.Timers.Timer(TimeSpan.FromMinutes(1).TotalMilliseconds);
            _pingTimer.Elapsed += _pingTimer_Elapsed;
            _pingTimer.Enabled = false;

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

            //超时
            var connectPolicyTimeout = Policy.TimeoutAsync(10, TimeoutStrategy.Pessimistic);

            _connectPolicy = Policy.WrapAsync(connectPolicyRetry, connectPolicyTimeout);

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
                    if (context.TryGetValue("hld", out var handleName))
                    {
                        if (context.TryGetValue("sub", out var handleSubject))
                        {
                            logger.LogError(ex, $"第 {retryAttempt}次 重新执行当前命令 客户端标识{_clientId} 操作 {handleName} 主题 {handleSubject} 将在 {retrySecond} 秒后重试");
                        }
                        else
                        {
                            logger.LogError(ex, $"第 {retryAttempt}次 重新执行当前命令 客户端标识{_clientId} 操作 {handleName} 将在 {retrySecond} 秒后重试");
                        }
                    }
                    else
                    {
                        logger.LogError(ex, $"第 {retryAttempt}次 重新执行当前命令 客户端标识{_clientId} 操作未知 将在 {retrySecond} 秒后重试");
                    }
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

            _pingTimer.Stop();

            _embed_channel = null;

            if (_subscriptionMessageHandler.Count > 0)
            {
                Task.Factory.StartNew(async () => await ConnectAsync());
            }

            _logger.LogInformation("NATS连接端口 完成实例化新的连接管道");
        }

        public async ValueTask<IChannel> ConnectAsync(TimeSpan? timeout = null)
        {
            if (_embed_channel != null && _embed_channel.Active)
                return _embed_channel;

            _logger.LogInformation($"当前通道 1 ClientId = {_clientId} _channel = {_embed_channel != null} _active = {_embed_channel?.Active} _isSet = {_autoResetEvent.SafeWaitHandle.IsClosed}");

            if (timeout.HasValue)
            {
                var receivesSignal = _autoResetEvent.WaitOne(timeout.Value);
                if (!receivesSignal) return null;
            }
            else
            {
                _autoResetEvent.WaitOne();
            }

            _logger.LogInformation($"当前通道 2 ClientId = {_clientId} _channel = {_embed_channel != null} _active = {_embed_channel?.Active} _isSet = {_autoResetEvent.SafeWaitHandle.IsClosed}");

            if (_embed_channel != null && _embed_channel.Active)
            {
                _autoResetEvent.Set();
                return _embed_channel;
            }

            await ChannelConnectAsync();

            _logger.LogInformation($"当前通道 3 ClientId = {_clientId} _channel = {_embed_channel != null} _active = {_embed_channel?.Active} _isSet = {_autoResetEvent.SafeWaitHandle.IsClosed}");

            _autoResetEvent.Set();

            _pingTimer.Start();

            return _embed_channel;
        }

        private void _pingTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            Task.Factory.StartNew(async () =>
            {
                await PingAsync();
            });
        }

    }
}
