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

        private static readonly Regex _publishSubjectRegex = new Regex(@"^[a-zA-Z\d]+(\.(\*|\>|[a-zA-Z\d]+))*$", RegexOptions.Compiled);
        private static readonly Regex _subscribeSubjectRegex = new Regex(@"^[a-zA-Z\d]+(\.(\*|\>|[a-zA-Z\d]+))*$", RegexOptions.Compiled);

        public NATSClient(
            ILogger<NATSClient> logger,
            NATSOptions options)
        {
            _options = options;
            _clientId = _options.ClientId;
            _subscriptionMessageHandler = new List<SubscriptionMessageHandler>();
            _bootstrap = InitBootstrap();
            _logger = logger;
        }

        public NATSClient(
            ILogger<NATSClient> logger,
            IOptions<NATSOptions> options) : this(logger, options.Value)
        {

        }

        public bool IsOpen => _channel?.Open ?? false;

        private Bootstrap InitBootstrap()
        {

            return new Bootstrap()
                .Group(new MultithreadEventLoopGroup())
                .Channel<TcpSocketChannel>()
                .Option(ChannelOption.TcpNodelay, false)
                .Handler(new ActionChannelInitializer<ISocketChannel>(channel =>
                {
                    channel.Pipeline.AddLast(NATSEncoder.Instance, NATSDecoder.Instance);
                    channel.Pipeline.AddLast(new ReconnectChannelHandler(_logger, ReconnectIfNeedAsync));
                    channel.Pipeline.AddLast(new ErrorPacketHandler(_logger));
                    channel.Pipeline.AddLast(new PingPacketHandler(_logger));
                    channel.Pipeline.AddLast(new PongPacketHandler(_logger));
                    channel.Pipeline.AddLast(new OKPacketHandler(_logger));
                    channel.Pipeline.AddLast(new InfoPacketHandler(InfoAsync));
                }));
        }

        private bool IsChannelInactive
        {
            get
            {
                return !_channel.Active;
            }
        }

        private async Task ReconnectIfNeedAsync(EndPoint socketAddress)
        {
            if (this.IsChannelInactive)
            {
                _logger.LogDebug("NATS 开始重新连接");

                //await this.semaphoreSlim.WaitAsync();
                try
                {
                    if (this.IsChannelInactive)
                    {
                        while (true)
                        {
                            try
                            {
                                _logger.LogDebug("NATS 开始尝试重新连接");

                                await ConnectAsync();

                                _logger.LogDebug("NATS 结束尝试重新连接");

                                break;
                            }
                            catch (Exception ex)
                            {
                                _logger.LogError(ex, "NATS 尝试重新连接异常");
                                await Task.Delay(TimeSpan.FromSeconds(3));
                            }
                        }
                        // this.clientRpcHandler = channel.Pipeline.Get<RpcClientHandler>();
                    }
                }
                finally
                {
                    _logger.LogDebug("NATS 完成重新连接");
                    //this.semaphoreSlim.Release();
                }
            }
        }

        public async ValueTask DisposeAsync()
        {
            await _channel?.CloseAsync();
        }
    }
}
