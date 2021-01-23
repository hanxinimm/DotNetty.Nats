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
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
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

        /// <summary>
        /// 连接通道
        /// </summary>
        private IChannel _channel;

        /// <summary>
        /// 连接状态
        /// </summary>
        private STANConnectionState _connectionState;

        /// <summary>
        /// 是否正在释放资源
        /// </summary>
        private bool _isDisposing;

        /// <summary>
        /// 连接配置
        /// </summary>
        private STANConnectionConfig _config;

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
        /// 限制并发线程
        /// </summary>
        private readonly SemaphoreSlim _semaphoreSlim;

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
            _semaphoreSlim = new SemaphoreSlim(1);
            _bootstrap = InitBootstrap();
            _logger = logger;
        }

        public STANClient(
            ILogger<STANClient> logger,
            IOptions<STANOptions> options) : this(logger, options.Value)
        {

        }

        public bool IsOpen => _channel?.Open ?? false;

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
                channel.Pipeline.AddLast(new ReconnectChannelHandler(_logger, ReconnectIfNeedAsync));
                channel.Pipeline.AddLast(new ErrorPacketHandler(_logger));
                channel.Pipeline.AddLast(new HeartbeatPacketHandler());
                
                channel.Pipeline.AddLast(new PubAckPacketSyncHandler(_logger, _waitPubAckTaskSchedule));
                channel.Pipeline.AddLast(new PubAckPacketAsynHandler(_logger));
                channel.Pipeline.AddLast(new PingPacketHandler(_logger));
                channel.Pipeline.AddLast(new PongPacketHandler(_logger));
            }));
        }

        private bool IsChannelInactive
        {
            get
            {
                if (_channel == null) return true;
                return !_channel.Active;
            }
        }

        private async Task ReconnectIfNeedAsync(EndPoint socketAddress)
        {
            await _semaphoreSlim.WaitAsync();

            if (_isDisposing)
            {
                _semaphoreSlim.Release();
                return;
            }

            _connectionState = STANConnectionState.Reconnecting;

            if (IsChannelInactive)
            {
                _logger.LogWarning("STAN 开始重新连接");

                while (true)
                {
                    try
                    {
                        _logger.LogDebug("STAN 开始尝试重新连接");

                        await ReconnectAsync();

                        _logger.LogDebug("STAN 结束尝试重新连接");

                        break;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "STAN 尝试重新连接异常");
                        await Task.Delay(TimeSpan.FromSeconds(3));
                    }
                }

                _logger.LogWarning("STAN 完成重新连接");
            }

            _semaphoreSlim.Release();
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
