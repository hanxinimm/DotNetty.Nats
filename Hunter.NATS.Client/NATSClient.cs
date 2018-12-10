using DotNetty.Codecs.NATS;
using DotNetty.Codecs.NATS.Packets;
using DotNetty.Handlers.NATS;
using DotNetty.Transport.Bootstrapping;
using DotNetty.Transport.Channels;
using DotNetty.Transport.Channels.Sockets;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Text.RegularExpressions;
using DotNetty.Buffers;
using System.Threading;

namespace Hunter.NATS.Client
{
    public class NATSClient
    {
        /// <summary>
        /// NATS配置
        /// </summary>
        private readonly NATSOptions _options;

        /// <summary>
        /// 通道引导
        /// </summary>
        private readonly Bootstrap _bootstrap;

        /// <summary>
        /// 订阅编号
        /// </summary>
        private int _subscribeId;

        /// <summary>
        /// 集群编号
        /// </summary>
        private string _clusterId;

        /// <summary>
        /// 客户端编号
        /// </summary>
        private string _clientId;

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
            _localSubscriptionConfig = new Dictionary<string, NATSSubscriptionConfig>();
            _bootstrap = new Bootstrap()
                .Group(new MultithreadEventLoopGroup())
                .Channel<TcpSocketChannel>()
                .Option(ChannelOption.TcpNodelay, false)
                .Handler(new ActionChannelInitializer<ISocketChannel>(channel =>
                {
                    channel.Pipeline.AddLast(NATSNewEncoder.Instance, NATSDecoder.Instance);
                    channel.Pipeline.AddLast(new ErrorPacketHandler());
                    channel.Pipeline.AddLast(new MessagePacketHandler(MessageAsync));
                    channel.Pipeline.AddLast(new PingPacketHandler());
                    channel.Pipeline.AddLast(new PongPacketHandler());
                    channel.Pipeline.AddLast(new OKPacketHandler());
                    channel.Pipeline.AddLast(new InfoPacketHandler(InfoAsync));
                }));

        }

        public async Task ContentcAsync(string clusterID, string clientId)
        {
            _channel = await _bootstrap.ConnectAsync(new IPEndPoint(IPAddress.Parse("192.168.0.226"), 4221));

            _clusterId = clusterID;
            _clientId = clientId;

            _info = await ConnectAsync();
        }

        private async Task<InfoPacket> ConnectAsync()
        {

            var Packet = new ConnectPacket(false, false, false, null, null, "test-client", null);

            _infoTaskCompletionSource = new TaskCompletionSource<InfoPacket>();

            await _channel.WriteAndFlushAsync(Packet);

            var ConnectInfoResult = await _infoTaskCompletionSource.Task;

            return ConnectInfoResult;

        }

        public async Task<string> SubscriptionAsync(string subject, string queueGroup, Action<byte[]> handler)
        {
            var SubscribeId = $"SUB{Interlocked.Increment(ref _subscribeId)}";

            _localSubscriptionConfig[SubscribeId] = new NATSSubscriptionConfig(subject, SubscribeId, handler);

            var SubscribePacket = new SubscribePacket(SubscribeId, subject, queueGroup);

            await _channel.WriteAndFlushAsync(SubscribePacket);

            return SubscribeId;

        }

        public async Task UnSubscriptionAsync(string subscribeId)
        {
            if (_localSubscriptionConfig.Remove(subscribeId))
            {
                var UnSubscribePacket = new UnSubscribePacket(subscribeId);

                await _channel.WriteAndFlushAsync(UnSubscribePacket);
            }
        }

        public async Task UnSubscriptionAsync(string subscribeId, int max_msgs)
        {
            if (_localSubscriptionConfig.Remove(subscribeId))
            {
                var UnSubscribePacket = new UnSubscribePacket(subscribeId, max_msgs);

                await _channel.WriteAndFlushAsync(UnSubscribePacket);
            }
        }

        /// <summary>
        /// 异步发送
        /// </summary>
        /// <param name="subject">主体</param>
        /// <param name="data">数据</param>
        /// <returns></returns>
        public Task PublishAsync(string subject, byte[] data)
        {
            var Packet = new PublishPacket(subject, data);

            return _channel.WriteAndFlushAsync(Packet);
        }

        protected void InfoAsync(InfoPacket info)
        {
            _infoTaskCompletionSource.TrySetResult(info);
        }

        protected void MessageAsync(MessagePacket message)
        {
            if (_localSubscriptionConfig.TryGetValue(message.SubscribeId, out var subscriptionConfig))
            {
                subscriptionConfig.Handler(message.Payload);
            }
        }
    }
}
