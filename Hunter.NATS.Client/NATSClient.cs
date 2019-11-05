using DotNetty.Codecs.NATS;
using DotNetty.Codecs.NATS.Packets;
using DotNetty.Handlers.NATS;
using DotNetty.Transport.Bootstrapping;
using DotNetty.Transport.Channels;
using DotNetty.Transport.Channels.Sockets;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Hunter.NATS.Client
{
    public partial class NATSClient
    {
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
        /// 本地订阅配置
        /// </summary>
        private Dictionary<string, NATSSubscriptionConfig> _localSubscriptionConfig = new Dictionary<string, NATSSubscriptionConfig>();


        private static readonly Regex _publishSubjectRegex = new Regex(@"^[a-zA-Z\d]+(\.(\*|\>|[a-zA-Z\d]+))*$", RegexOptions.Compiled);
        private static readonly Regex _subscribeSubjectRegex = new Regex(@"^[a-zA-Z\d]+(\.(\*|\>|[a-zA-Z\d]+))*$", RegexOptions.Compiled);

        public NATSClient(NATSOptions options)
        {
            _options = options;
            _clientId = _options.ClientId;
            _localSubscriptionConfig = new Dictionary<string, NATSSubscriptionConfig>();
            _bootstrap = new Bootstrap()
                .Group(new MultithreadEventLoopGroup())
                .Channel<TcpSocketChannel>()
                .Option(ChannelOption.TcpNodelay, false)
                .Handler(new ActionChannelInitializer<ISocketChannel>(channel =>
                {
                    channel.Pipeline.AddLast(NATSEncoder.Instance, NATSDecoder.Instance);
                    channel.Pipeline.AddLast(new ErrorPacketHandler());
                    channel.Pipeline.AddLast(new MessagePacketHandler(MessageAsync));
                    channel.Pipeline.AddLast(new PingPacketHandler());
                    channel.Pipeline.AddLast(new PongPacketHandler());
                    channel.Pipeline.AddLast(new OKPacketHandler());
                    channel.Pipeline.AddLast(new InfoPacketHandler(InfoAsync));
                }));

        }

        public NATSClient(IOptions<NATSOptions> options) : this(options.Value)
        {

        }

        public bool IsOpen => _channel?.Open ?? false;
    }
}
