using DotNetty.Codecs.STAN;
using DotNetty.Codecs.STAN.Packets;
using DotNetty.Codecs.STAN.Protocol;
using DotNetty.Handlers.STAN;
using DotNetty.Transport.Bootstrapping;
using DotNetty.Transport.Channels;
using DotNetty.Transport.Channels.Sockets;
using Hunter.STAN.Client.Handlers;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Polly;
using Polly.CircuitBreaker;
using Polly.Timeout;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace Hunter.STAN.Client
{
    public partial class STANClient : IAsyncDisposable
    {
        private readonly ILogger _logger;

        /// <summary>
        /// STAN配置
        /// </summary>
        private readonly STANOptions _options;

        /// <summary>
        /// 客户端标识
        /// </summary>
        public readonly string _identity;

        /// <summary>
        /// 客户端编号
        /// </summary>
        private readonly string _clientId;

        /// <summary>
        /// 心跳收件箱
        /// </summary>
        private readonly string _heartbeatInboxId;

        /// <summary>
        /// 消息应答收件箱
        /// </summary>
        private readonly string _replyInboxId;

        /// <summary>
        /// 
        /// </summary>
        private readonly ValueTask<bool> _ackSuccessResult = new ValueTask<bool>(true);

        ///// <summary>
        ///// 通道引导
        ///// </summary>
        private Bootstrap _bootstrap;

        ///// <summary>
        ///// 连接通道实例
        ///// </summary>
        private IChannel _embed_channel;

        /// <summary>
        /// 限制并发线程
        /// </summary>
        private readonly ManualResetEventSlim _autoResetEvent;

        /// <summary>
        /// 连接状态
        /// </summary>
        private STANConnectionState _connectionState;

        /// <summary>
        /// 连接配置
        /// </summary>
        private STANConnectionConfig _config;

        /// <summary>
        /// 请求响应任务
        /// </summary>
        private TaskCompletionSource<ConnectResponsePacket> _connectResponseTaskCompletionSource;

        /// <summary>
        /// 等待发送消息确认安排表
        /// </summary>
        private readonly ConcurrentDictionary<string, TaskCompletionSource<PubAckPacket>> _waitPubAckTaskSchedule 
            = new ConcurrentDictionary<string, TaskCompletionSource<PubAckPacket>>();

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

        public STANClient(
            ILogger<STANClient> logger,
            STANOptions options)
        {
            _options = options ?? throw new ArgumentNullException(nameof(STANOptions));
            _identity = Guid.NewGuid().ToString("n");
            _clientId = $"{_options.ClientId}-{_identity}";
            _heartbeatInboxId = _identity;
            _replyInboxId = _identity;
            _subscriptionMessageHandler = new List<SubscriptionMessageHandler>();
            _bootstrap = InitBootstrap();
            _logger = logger;
            _autoResetEvent = new ManualResetEventSlim(true, 1);

            #region Connect;

            //重试
            var connectPolicyRetry = Policy
                .Handle<ConnectException>()
                .Or<SocketException>()
                .Or<TimeoutException>()
                .WaitAndRetryForeverAsync(
                    (retryAttempt, context) =>
                {
                    logger.LogWarning($"重试连接Stan客户端 客户端标识{_clientId} 第 {retryAttempt} 次尝试");
                    return TimeSpan.FromSeconds(retryAttempt);
                },
                (ex, retrySecond, retryAttempt, context) =>
                {
                    logger.LogError(ex, $"第 {retryAttempt}次 重新连接Stan客户端 客户端标识{_clientId} 操作 {context["hld"]} 主题 {context["sub"]} 将在 {retrySecond} 秒后重试");
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
                .Or<SocketException>()
                .WaitAndRetryAsync(3,
                (retryAttempt, context) =>
                {
                    logger.LogWarning($"重试执行Stan客户端命令 客户端标识{_clientId} 第 {retryAttempt} 次尝试");
                    return TimeSpan.FromSeconds(retryAttempt);
                },
                (ex,  retrySecond, retryAttempt, context) =>
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

        public STANClient(
            ILogger<STANClient> logger,
            IOptions<STANOptions> options) : this(logger, options.Value)
        {

        }

        //public bool IsOpen => _channel?.Open ?? false;

        public STANConnectionState ConnectionState => _connectionState;

        private Bootstrap InitBootstrap()
        {
            return new Bootstrap()
            .Group(new MultithreadEventLoopGroup())
            .Channel<TcpSocketChannel>()
            .Option(ChannelOption.TcpNodelay, false)
            .Handler(new ActionChannelInitializer<ISocketChannel>(channel =>
            {
                channel.Pipeline.AddLast(STANEncoder.Instance, new STANDecoder(_logger));
                channel.Pipeline.AddLast(new ReconnectChannelHandler(_logger, ReconnectIfNeed));
                channel.Pipeline.AddLast(new ErrorPacketHandler(_logger));
                channel.Pipeline.AddLast(new HeartbeatPacketHandler());

                channel.Pipeline.AddLast(new ReplyPacketHandler<ConnectResponsePacket>(_replyInboxId, _connectResponseTaskCompletionSource));

                channel.Pipeline.AddLast(new PubAckPacketSyncHandler(_logger, _waitPubAckTaskSchedule));
                channel.Pipeline.AddLast(new PubAckPacketAsynHandler(_logger));
                channel.Pipeline.AddLast(new PingPacketHandler(_logger, _clientId));
                channel.Pipeline.AddLast(new PongPacketHandler(_logger, _clientId));
            }));

            
        }

        private void ReconnectIfNeed(EndPoint socketAddress)
        {
            _logger.LogInformation($"STAN连接端口 ClientId = {_clientId} 开始实例化新的连接管道");

            _embed_channel = null;

            _autoResetEvent.Set();

            _logger.LogInformation($"STAN连接端口 ClientId = {_clientId} 完成实例化新的连接管道");
        }

        async ValueTask<IChannel> ChannelConnectAsync(TimeSpan? timeout = null)
        {
            if (_embed_channel != null && _embed_channel.Active)
                return _embed_channel;

            _logger.LogInformation($"当前通道 1 ClientId = {_clientId} _channel = {_embed_channel != null} _active = {_embed_channel?.Active} _isSet = {_autoResetEvent.IsSet}");

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

            _logger.LogInformation($"当前通道 2 ClientId = {_clientId} _channel = {_embed_channel != null} _active = {_embed_channel?.Active} _isSet = {_autoResetEvent.IsSet}");


            if (_embed_channel != null && _embed_channel.Active)
            {
                _autoResetEvent.Set();
                return _embed_channel;
            }

            await ConnectAsync();

            _logger.LogInformation($"当前通道 3 ClientId = {_clientId} _channel = {_embed_channel != null} _active = {_embed_channel?.Active} _isSet = {_autoResetEvent.IsSet}");

            _autoResetEvent.Set();

            return _embed_channel;
        }

        #region 消息发送确认

        #endregion;

        private void CheckSubject(string subject)
        {
            if (string.IsNullOrEmpty(subject)) throw new ArgumentNullException(nameof(subject));
        }

        private void CheckQueueGroup(string queueGroup)
        {
            if (string.IsNullOrEmpty(queueGroup)) throw new ArgumentNullException(nameof(queueGroup));
        }
    }
}
