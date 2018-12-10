using DotNetty.Codecs.STAN;
using DotNetty.Codecs.STAN.Packets;
using DotNetty.Codecs.STAN.Protocol;
using DotNetty.Handlers.STAN;
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

namespace Hunter.STAN.Client
{
    public class STANClient
    {
        /// <summary>
        /// STAN配置
        /// </summary>
        private readonly STANOptions _options;

        /// <summary>
        /// 通道引导
        /// </summary>
        private readonly Bootstrap _bootstrap;

        /// <summary>
        /// 心跳收件箱
        /// </summary>
        private readonly string _heartbeatInboxId;

        /// <summary>
        /// 消息应答收件箱
        /// </summary>
        private readonly string _replyInboxId;

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
        private STANConnectionConfig _config;

        /// <summary>
        /// 等待发送消息确认安排表
        /// </summary>
        private ConcurrentDictionary<string, TaskCompletionSource<PubAckPacket>> _waitPubAckTaskSchedule;

        /// <summary>
        /// 等待订阅消息响应安排表
        /// </summary>
        private ConcurrentDictionary<string, TaskCompletionSource<SubscriptionResponsePacket>> _waitSubResponseTaskSchedule;

        /// <summary>
        /// 本地订阅配置
        /// </summary>
        private ConcurrentDictionary<string, STANSubscriptionConfig> _localSubscriptionConfig;


        private static readonly Regex _subjectRegex = new Regex(@"^[a-zA-Z\d]+(\.(\*|\>|[a-zA-Z\d]+))*$", RegexOptions.Compiled);
        private static readonly Regex _subscribeSubjectRegex = new Regex(@"^[a-zA-Z\d]+(\.(\*|\>|[a-zA-Z\d]+))*$", RegexOptions.Compiled);

        public STANClient(STANOptions options)
        {
            _options = options;
            _heartbeatInboxId = GenerateInboxId();
            _replyInboxId = GenerateInboxId();
            _waitPubAckTaskSchedule = new ConcurrentDictionary<string, TaskCompletionSource<PubAckPacket>>();
            _waitSubResponseTaskSchedule = new ConcurrentDictionary<string, TaskCompletionSource<SubscriptionResponsePacket>>();
            _localSubscriptionConfig = new ConcurrentDictionary<string, STANSubscriptionConfig>();
            _bootstrap = new Bootstrap()
                .Group(new MultithreadEventLoopGroup())
                .Channel<TcpSocketChannel>()
                .Option(ChannelOption.TcpNodelay, false)
                .Handler(new ActionChannelInitializer<ISocketChannel>(channel =>
                {
                    channel.Pipeline.AddLast(STANEncoder.Instance, new STANDecoder());
                    channel.Pipeline.AddLast(new ErrorPacketHandler());
                    channel.Pipeline.AddLast(new HeartbeatPacketHandler(_heartbeatInboxId, HeartbeatACKAsync));
                    channel.Pipeline.AddLast(new MessagePacketHandler(AckAsync));
                    channel.Pipeline.AddLast(new PubAckPacketSyncHandler(_waitPubAckTaskSchedule));
                    channel.Pipeline.AddLast(new PubAckPacketAsynHandler((ack) => { }));
                    channel.Pipeline.AddLast(new SubscriptionResponsePacketSyncHandler(_waitSubResponseTaskSchedule));
                    channel.Pipeline.AddLast(new SubscriptionResponsePacketAsynHandler((subReps) => { }));
                }));

        }

        public async Task ContentcAsync(string clusterID, string clientId)
        {
            _channel = await _bootstrap.ConnectAsync(new IPEndPoint(IPAddress.Parse("192.168.0.226"), 4222));

            _clusterId = clusterID;
            _clientId = clientId;

            await SubscribeHeartBeatInboxAsync();

            await SubscribeReplyInboxAsync();

            _config = await ConnectRequestAsync();
        }

        private async Task<STANConnectionConfig> ConnectRequestAsync()
        {

            var Packet = new ConnectRequestPacket(_replyInboxId, _clusterId, _clientId);

            var ConnectResponseReady = new TaskCompletionSource<ConnectResponsePacket>();

            var Handler = new ReplyPacketHandler<ConnectResponsePacket>(Packet.ReplyTo, ConnectResponseReady);

            _channel.Pipeline.AddLast(Handler);

            await _channel.WriteAndFlushAsync(Packet);

            var ConnectResponse = await ConnectResponseReady.Task;

            _channel.Pipeline.Remove(Handler);

            return new STANConnectionConfig(
                ConnectResponse.Message.PubPrefix,
                ConnectResponse.Message.SubRequests,
                ConnectResponse.Message.UnsubRequests,
                ConnectResponse.Message.CloseRequests,
                ConnectResponse.Message.SubCloseRequests,
                ConnectResponse.Message.PublicKey);
        }

        private async Task SubscribeHeartBeatInboxAsync()
        {
            var Packet = new HeartbeatInboxPacket(_heartbeatInboxId);

            await _channel.WriteAndFlushAsync(Packet);
        }

        private void HeartbeatACKAsync(MsgProtoPacket packet)
        {
            _channel.WriteAndFlushAsync(new HeartbeatAckPacket(packet.Message.Reply));
        }

        private async Task SubscribeReplyInboxAsync()
        {
            await _channel.WriteAndFlushAsync(new InboxPacket(DateTime.Now.Ticks.ToString(), _replyInboxId));
        }

        public SubscriptionResponsePacket Subscription(string subject, string queueGroup, Action<byte[]> handler)
        {
            var SubscribePacket = new SubscribePacket(DateTime.Now.Ticks.ToString());

            //订阅侦听消息
            _channel.WriteAndFlushAsync(SubscribePacket).GetAwaiter().GetResult();

            var Packet = new SubscriptionRequestPacket(_replyInboxId, _config.SubRequests, _clientId, subject, queueGroup, SubscribePacket.Subject, 1024, 3, "KeepLast", StartPosition.LastReceived);

            Console.WriteLine($"订阅命令消息回复的收件箱 {Packet.ReplyTo}");
            Console.WriteLine($"订阅的消息收件箱 {Packet.Message.Inbox}");

            var SubscriptionResponseReady = new TaskCompletionSource<SubscriptionResponsePacket>();

            _waitSubResponseTaskSchedule[Packet.ReplyTo] = SubscriptionResponseReady;
            
            //发送订阅请求
            _channel.WriteAndFlushAsync(Packet).GetAwaiter().GetResult();

            var SubscriptionResponseResult = SubscriptionResponseReady.Task.GetAwaiter().GetResult();

            _localSubscriptionConfig[Packet.Message.Inbox] = new STANSubscriptionConfig(subject, Packet.Message.Inbox, SubscriptionResponseResult.Message.AckInbox, handler);

            return SubscriptionResponseResult;
        }

        public async Task<SubscriptionResponsePacket> SubscriptionAsync(string subject, string queueGroup)
        {
            var SubscribePacket = new SubscribePacket(DateTime.Now.Ticks.ToString());

            //订阅侦听消息
            await _channel.WriteAndFlushAsync(SubscribePacket);

            var Packet = new SubscriptionRequestPacket(_replyInboxId, _config.SubRequests, _clientId, subject, queueGroup, SubscribePacket.Subject, 1024, 3, "KeepLast", StartPosition.LastReceived);

            var SubscriptionResponseReady = new TaskCompletionSource<SubscriptionResponsePacket>();

            var Handler = new ReplyPacketHandler<SubscriptionResponsePacket>(Packet.ReplyTo, SubscriptionResponseReady);

            _channel.Pipeline.AddLast(Handler);

            //发送订阅请求
            await _channel.WriteAndFlushAsync(Packet);

            var SubscriptionResponse = await SubscriptionResponseReady.Task;

            _channel.Pipeline.Remove(Handler);

            return SubscriptionResponse;
        }

        /// <summary>
        /// 同步发送
        /// </summary>
        /// <param name="subject">主体</param>
        /// <param name="data">数据</param>
        /// <returns></returns>
        public PubAckPacket Publish(string subject, byte[] data)
        {

            var Packet = new PubMsgPacket(_replyInboxId, _config.PubPrefix, _clientId, subject, data);

            var PubAckReady = new TaskCompletionSource<PubAckPacket>();

            _waitPubAckTaskSchedule[Packet.ReplyTo] = PubAckReady;

            //发送订阅请求
             _channel.WriteAndFlushAsync(Packet).GetAwaiter().GetResult();

            var AckResult = PubAckReady.Task.GetAwaiter().GetResult();

            return AckResult;
        }

        /// <summary>
        /// 异步发送
        /// </summary>
        /// <param name="subject">主体</param>
        /// <param name="data">数据</param>
        /// <returns></returns>
        public Task PublishAsync(string subject, byte[] data)
        {
            var Packet = new PubMsgPacket(_replyInboxId, _config.PubPrefix, _clientId, subject, data);

            return _channel.WriteAndFlushAsync(Packet);
        }

        public Task AckAsync(IChannel bootstrapChannel, string subject, ulong sequence)
        {
            var AckInbox = string.Empty;

            var Packet = new AckPacket(AckInbox, subject, sequence);

            //发送消息成功处理
            return bootstrapChannel.WriteAndFlushAsync(Packet);
        }

        public void AckAsync(IChannel channel, MsgProtoPacket msg)
        {
            if (_localSubscriptionConfig.TryGetValue(msg.Subject, out var subscriptionConfig))
            {

                subscriptionConfig.Handler(msg.Message.Data.ToByteArray());

                var Packet = new AckPacket(subscriptionConfig.AckInbox, msg.Message.Subject, msg.Message.Sequence);

                //发送消息成功处理
                _channel.WriteAndFlushAsync(Packet);

                //channel.Pipeline.FireChannelReadComplete();
            }
        }

        public async Task<CloseResponsePacket> CloseRequestAsync(string inboxId)
        {

            var Packet = new CloseRequestPacket(inboxId, _config.CloseRequests, _clientId);

            var CloseRequestReady = new TaskCompletionSource<CloseResponsePacket>();

            var Handler = new ReplyPacketHandler<CloseResponsePacket>(Packet.ReplyTo, CloseRequestReady);

            _channel.Pipeline.AddLast(Handler);

            //发送关闭
            await _channel.WriteAndFlushAsync(Packet);

            var Result = await CloseRequestReady.Task;

            _channel.Pipeline.Remove(Handler);

            return Result;
        }

        private static string GenerateInboxId()
        {
            return Guid.NewGuid().ToString("N");
        }

    }
}
