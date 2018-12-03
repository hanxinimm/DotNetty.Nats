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



        public STANClient(STANOptions options)
        {
            _options = options;
            _heartbeatInboxId = GenerateInboxId();
            _replyInboxId = GenerateInboxId();
            _bootstrap = new Bootstrap()
                .Group(new MultithreadEventLoopGroup())
                .Channel<TcpSocketChannel>()
                .Option(ChannelOption.TcpNodelay, false)
                .Handler(new ActionChannelInitializer<ISocketChannel>(channel =>
                {
                    channel.Pipeline.AddFirst(new STANDelimiterBasedFrameDecoder(4096));
                    channel.Pipeline.AddLast(STANEncoder.Instance, new STANDecoder());
                    channel.Pipeline.AddLast(new ErrorPacketHandler());
                    channel.Pipeline.AddLast(new HeartbeatPacketHandler(_heartbeatInboxId, HeartbeatACKAsync));
                    channel.Pipeline.AddLast(new MessagePacketHandler(AckAsync));
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

        public static async Task<PubAckPacket> PublishAsync(IChannel bootstrapChannel, ConnectResponse connectResponse, string clientId, string inboxId, byte[] data)
        {

            var PubAckReady = new TaskCompletionSource<PubAckPacket>();

            var Handler = new ReplyPacketHandler<PubAckPacket>(string.Empty, PubAckReady);

            bootstrapChannel.Pipeline.AddLast(Handler);


            for (int i = 0; i < 100; i++)
            {
                var Packet = new PubMsgPacket(inboxId, connectResponse.PubPrefix, clientId, "foo", data);

                //发送订阅请求
                await bootstrapChannel.WriteAndFlushAsync(Packet);

            }
            var Result = await PubAckReady.Task;

            bootstrapChannel.Pipeline.Remove(Handler);

            return Result;

            return null;
        }

        public static Task AckAsync(IChannel bootstrapChannel, string subject, ulong sequence)
        {
            var AckInbox = string.Empty;

            var Packet = new AckPacket(AckInbox, subject, sequence);

            //发送消息成功处理
            return bootstrapChannel.WriteAndFlushAsync(Packet);
        }

        public static async Task<CloseResponsePacket> CloseRequestAsync(IChannel bootstrapChannel, ConnectResponse connectResponse, string clientId, string inboxId)
        {

            var Packet = new CloseRequestPacket(inboxId, connectResponse.CloseRequests, clientId);

            var CloseRequestReady = new TaskCompletionSource<CloseResponsePacket>();

            var Handler = new ReplyPacketHandler<CloseResponsePacket>(Packet.ReplyTo, CloseRequestReady);

            bootstrapChannel.Pipeline.AddLast(Handler);

            //发送关闭
            await bootstrapChannel.WriteAndFlushAsync(Packet);

            var Result = await CloseRequestReady.Task;

            bootstrapChannel.Pipeline.Remove(Handler);

            return Result;
        }

        private static string GenerateInboxId()
        {
            return Guid.NewGuid().ToString("N");
        }

    }
}
