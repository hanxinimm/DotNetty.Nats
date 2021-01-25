using DotNetty.Codecs.NATS;
using DotNetty.Codecs.NATS.Packets;
using DotNetty.Handlers.NATS;
using DotNetty.Transport.Bootstrapping;
using DotNetty.Transport.Channels;
using DotNetty.Transport.Channels.Sockets;
using Hunter.NATS.Client.Handlers;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Net;
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
        /// 通道引导
        /// </summary>
        private readonly Bootstrap _bootstrap;

        /// <summary>
        /// 订阅编号
        /// </summary>
        private int _subscribeId;

        /// <summary>
        /// 连接通道
        /// </summary>
        private IChannel _channel;

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
        /// 限制并发线程
        /// </summary>
        private readonly ManualResetEvent _manualResetEvent;

        private static readonly Regex _publishSubjectRegex = new Regex(@"^[a-zA-Z\d]+(\.(\*|\>|[a-zA-Z\d]+))*$", RegexOptions.Compiled);
        private static readonly Regex _subscribeSubjectRegex = new Regex(@"^[a-zA-Z\d]+(\.(\*|\>|[a-zA-Z\d]+))*$", RegexOptions.Compiled);

        public NATSClient(
            ILogger<NATSClient> logger,
            NATSOptions options)
        {
            _options = options;
            _clientId = _options.ClientId;
            _subscriptionMessageHandler = new List<SubscriptionMessageHandler>();
            _manualResetEvent = new ManualResetEvent(true);
            _bootstrap = InitBootstrap();
            _logger = logger;
        }

        public NATSClient(
            ILogger<NATSClient> logger,
            IOptions<NATSOptions> options) : this(logger, options.Value)
        {
            
        }

        public bool IsOpen => _channel?.Open ?? false;

        public NATSConnectionState ConnectionState => _connectionState;

        private Bootstrap InitBootstrap()
        {
            return new Bootstrap()
                .Group(new MultithreadEventLoopGroup())
                .Channel<TcpSocketChannel>()
                .Option(ChannelOption.TcpNodelay, false)
                .Handler(new ActionChannelInitializer<ISocketChannel>(channel =>
                {
                    channel.Pipeline.AddLast("NATSEncoder", NATSEncoder.Instance);
                    channel.Pipeline.AddLast("NATSDecoder", new NATSDecoder(_logger));
                    channel.Pipeline.AddLast("Reconnect", new ReconnectChannelHandler(_logger, ReconnectIfNeedAsync));
                    channel.Pipeline.AddLast("Ping", new PingPacketHandler(_logger));
                    channel.Pipeline.AddLast("Pong", new PongPacketHandler(_logger));
                    channel.Pipeline.AddLast("OK", new OKPacketHandler(_logger));
                    channel.Pipeline.AddLast("INFO", new InfoPacketHandler(_logger, InfoAsync));
                    channel.Pipeline.AddLast("Error", new ErrorPacketHandler(_logger));
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
            _manualResetEvent.WaitOne();
            _manualResetEvent.Reset();

            _connectionState = NATSConnectionState.Reconnecting;

            if (IsChannelInactive)
            {
                _logger.LogWarning("NATS 开始重新连接");


                while (true)
                {
                    try
                    {
                        _logger.LogDebug("NATS 开始尝试重新连接");

                        if (_connectionState == NATSConnectionState.Reconnecting)
                        {
                            await ReconnectAsync();
                        }
                        else
                        {
                            _logger.LogWarning("NATS 连接状态发生改变,停止重试");
                        }

                        _logger.LogDebug("NATS 结束尝试重新连接");

                        break;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "NATS 尝试重新连接异常");
                        await Task.Delay(TimeSpan.FromSeconds(3));
                    }
                }

                _logger.LogWarning("NATS 完成重新连接");
            }

            _manualResetEvent.Set();
        }

    }
}
