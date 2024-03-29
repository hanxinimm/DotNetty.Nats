﻿using DotNetty.Codecs.STAN;
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
        private readonly AutoResetEvent _autoResetEvent;

        /// <summary>
        /// 连接状态
        /// </summary>
        private STANConnectionState _connectionState;

        /// <summary>
        /// 连接配置
        /// </summary>
        private STANConnectionConfig _config;

        /// <summary>
        /// 请求连接处理器
        /// </summary>
        private ConnectRequestPacketHandler _connectResponseReplyHandler;

        /// <summary>
        /// 请求关闭处理器
        /// </summary>
        private CloseRequestPacketHandler _closeResponseReplyHandler;

        /// <summary>
        /// 请求关闭处理器
        /// </summary>
        private PingRequestPacketHandler _pingResponseReplyHandler;


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

        /// <summary>
        /// 心跳服务定时器
        /// </summary>
        private readonly System.Timers.Timer _pingTimer;

        public STANClient(
            ILogger<STANClient> logger,
            STANOptions options)
        {
            _options = options ?? throw new ArgumentNullException(nameof(STANOptions));
            _identity = Guid.NewGuid().ToString("n");
            _clientId = $"{_options.ClientId}-{_identity}";
            _heartbeatInboxId = _identity;
            _replyInboxId = _identity;
            _connectResponseReplyHandler = new ConnectRequestPacketHandler(_replyInboxId);
            _closeResponseReplyHandler = new CloseRequestPacketHandler(_replyInboxId);
            _pingResponseReplyHandler = new PingRequestPacketHandler(_replyInboxId);
            _subscriptionMessageHandler = new List<SubscriptionMessageHandler>();
            _bootstrap = InitBootstrap();
            _logger = logger;
            _autoResetEvent = new AutoResetEvent(true);

            _pingTimer = new System.Timers.Timer(TimeSpan.FromMinutes(1).TotalMilliseconds);
            _pingTimer.Elapsed += _pingTimer_Elapsed;
            _pingTimer.Enabled = false;

            #region Connect;

            //重试
            var connectPolicyRetry = Policy
                .Handle<ConnectException>()
                .Or<SocketException>()
                .Or<TimeoutException>()
                .Or<TimeoutRejectedException>()
                .WaitAndRetryForeverAsync(
                    (retryAttempt, context) =>
                {
                    logger.LogWarning($"重试连接Stan客户端 客户端标识{_clientId} 第 {retryAttempt} 次尝试");
                    return TimeSpan.FromSeconds(retryAttempt);
                },
                (ex, retrySecond, retryAttempt, context) =>
                {
                    logger.LogError(ex, $"第 {retryAttempt}次 重新连接Stan客户端 客户端标识{_clientId} 将在 {retrySecond} 秒后重试");
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
            var policyTimeout = Policy.TimeoutAsync(20, TimeoutStrategy.Pessimistic);

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
                channel.Pipeline.AddLast(_connectResponseReplyHandler);
                channel.Pipeline.AddLast(_closeResponseReplyHandler);
                channel.Pipeline.AddLast(_pingResponseReplyHandler);

                channel.Pipeline.AddLast(new ErrorPacketHandler(_logger));
                channel.Pipeline.AddLast(new HeartbeatPacketHandler());

                channel.Pipeline.AddLast(new PubAckPacketSyncHandler(_logger, _waitPubAckTaskSchedule));
                channel.Pipeline.AddLast(new PubAckPacketAsynHandler(_logger));
                channel.Pipeline.AddLast(new PingPacketHandler(_logger, _clientId));
                channel.Pipeline.AddLast(new PongPacketHandler(_logger, _clientId));
            }));
        }

        private void ReconnectIfNeed(EndPoint socketAddress)
        {
            _logger.LogInformation($"STAN重新连接端口 ClientId = {_clientId} 开始实例化新的连接管道");

            _pingTimer.Stop();

            _embed_channel = null;

            if (_subscriptionMessageHandler.Count > 0)
            {
                ConnectAsync().ConfigureAwait(false).GetAwaiter().GetResult();
            }

            _logger.LogInformation($"STAN重新连接端口 ClientId = {_clientId} 完成实例化新的连接管道");
        }

        public async ValueTask<IChannel> ConnectAsync(TimeSpan? timeout = null)
        {
            if (_embed_channel != null && _embed_channel.Active && _config != null)
                return _embed_channel;

            _logger.LogInformation($"当前通道 1 ClientId = {_clientId} _channel = {_embed_channel != null} _active = {_embed_channel?.Active} _isSet = {_autoResetEvent.SafeWaitHandle.IsClosed} _config = {_config}");

            if (timeout.HasValue)
            {
                var receivesSignal = _autoResetEvent.WaitOne(timeout.Value);
                if (!receivesSignal) return null;
            }
            else
            {
                _autoResetEvent.WaitOne();
            }

            _logger.LogInformation($"当前通道 2 ClientId = {_clientId} _channel = {_embed_channel != null} _active = {_embed_channel?.Active} _isSet = {_autoResetEvent.SafeWaitHandle.IsClosed} _config = {_config}");


            if (_embed_channel != null && _embed_channel.Active && _config != null)
            {
                _autoResetEvent.Set();

                return _embed_channel;
            }

            await ChannelConnectAsync();

            _logger.LogInformation($"当前通道 3 ClientId = {_clientId} _channel = {_embed_channel != null} _active = {_embed_channel?.Active} _isSet = {_autoResetEvent.SafeWaitHandle.IsClosed} _config = {_config}");

            _autoResetEvent.Set();

            _pingTimer.Start();

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

        private void _pingTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            Task.Factory.StartNew(async () =>
            {
                var pingResponse = await ConnectPingAsync();

                if (pingResponse == null || !string.IsNullOrEmpty(pingResponse.Error))
                {
                    _logger.LogError($"STAN 连接不再有效 Client Ping 错误:{pingResponse?.Error}");

                    ReconnectIfNeed(null);
                }
            }).Unwrap();
        }
    }
}
