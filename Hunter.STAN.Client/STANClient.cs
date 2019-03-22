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
using Hunter.STAN.Client.Handlers;

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
        /// 等待取消订阅消息响应安排表
        /// </summary>
        private ConcurrentDictionary<string, TaskCompletionSource<UnSubscriptionResponsePacket>> _waitUnSubResponseTaskSchedule;

        /// <summary>
        /// 本地订阅配置
        /// </summary>
        private ConcurrentDictionary<string, STANSubscriptionConfig> _localSubscriptionConfig;

        public STANClient(STANOptions options)
        {
            _options = options;
            _heartbeatInboxId = GenerateInboxId();
            _replyInboxId = GenerateInboxId();
            _waitPubAckTaskSchedule = new ConcurrentDictionary<string, TaskCompletionSource<PubAckPacket>>();
            _waitSubResponseTaskSchedule = new ConcurrentDictionary<string, TaskCompletionSource<SubscriptionResponsePacket>>();
            _waitUnSubResponseTaskSchedule = new ConcurrentDictionary<string, TaskCompletionSource<UnSubscriptionResponsePacket>>();
            _localSubscriptionConfig = new ConcurrentDictionary<string, STANSubscriptionConfig>();
            _bootstrap = new Bootstrap()
                .Group(new MultithreadEventLoopGroup())
                .Channel<TcpSocketChannel>()
                .Option(ChannelOption.TcpNodelay, false)
                .Handler(new ActionChannelInitializer<ISocketChannel>(channel =>
                {
                    channel.Pipeline.AddLast(new ReconnectChannelHandler());
                    channel.Pipeline.AddLast(STANEncoder.Instance, STANDecoder.Instance);
                    channel.Pipeline.AddLast(new ErrorPacketHandler());
                    channel.Pipeline.AddLast(new HeartbeatPacketHandler());
                    channel.Pipeline.AddLast(new MessagePacketHandler(AckAsync));
                    channel.Pipeline.AddLast(new PubAckPacketSyncHandler(_waitPubAckTaskSchedule));
                    channel.Pipeline.AddLast(new PubAckPacketAsynHandler(PubAckCallback));
                    channel.Pipeline.AddLast(new SubscriptionResponsePacketSyncHandler(_waitSubResponseTaskSchedule));
                    channel.Pipeline.AddLast(new SubscriptionResponsePacketAsynHandler((subReps) => { }));
                    channel.Pipeline.AddLast(new UnSubscriptionResponsePacketSyncHandler(_waitUnSubResponseTaskSchedule));
                    channel.Pipeline.AddLast(new PingPacketHandler());
                    channel.Pipeline.AddLast(new PongPacketHandler());
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

            var Packet = new ConnectRequestPacket(_replyInboxId, _clusterId, _clientId, _heartbeatInboxId);

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

        private async Task SubscribeReplyInboxAsync()
        {
            await _channel.WriteAndFlushAsync(new InboxPacket(DateTime.Now.Ticks.ToString(), _replyInboxId));
        }

        public string Subscribe(string subject, string queueGroup, string durableName, Action<byte[]> handler)
        {
            var SubscribePacket = new SubscribePacket(DateTime.Now.Ticks.ToString());

            //订阅侦听消息
            _channel.WriteAndFlushAsync(SubscribePacket).GetAwaiter().GetResult();

            var Packet = new SubscriptionRequestPacket(_replyInboxId, _config.SubRequests, _clientId, subject, queueGroup, SubscribePacket.Subject, 1024, 30, durableName, StartPosition.LastReceived);

            var SubscriptionResponseReady = new TaskCompletionSource<SubscriptionResponsePacket>();

            _waitSubResponseTaskSchedule[Packet.ReplyTo] = SubscriptionResponseReady;

            //发送订阅请求
            _channel.WriteAndFlushAsync(Packet).GetAwaiter().GetResult();

            var SubscriptionResponseResult = SubscriptionResponseReady.Task.GetAwaiter().GetResult();

            _localSubscriptionConfig[Packet.Message.Inbox] = new STANSubscriptionConfig(subject, Packet.Message.Inbox, SubscriptionResponseResult.Message.AckInbox, handler);

            return Packet.Message.Inbox;
        }

        public async Task<string> SubscribeAsync(string subject, string queueGroup, string durableName, Action<byte[]> handler)
        {
            var SubscribePacket = new SubscribePacket(DateTime.Now.Ticks.ToString());

            //订阅侦听消息
            await _channel.WriteAndFlushAsync(SubscribePacket);

            var Packet = new SubscriptionRequestPacket(_replyInboxId, _config.SubRequests, _clientId, subject, queueGroup, SubscribePacket.Subject, 1024, 3, durableName, StartPosition.LastReceived);

            var SubscriptionResponseReady = new TaskCompletionSource<SubscriptionResponsePacket>();

            _waitSubResponseTaskSchedule[Packet.ReplyTo] = SubscriptionResponseReady;

            //发送订阅请求
            await _channel.WriteAndFlushAsync(Packet);

            var SubscriptionResponseResult = await SubscriptionResponseReady.Task;

            _localSubscriptionConfig[Packet.Message.Inbox] = new STANSubscriptionConfig(subject, Packet.Message.Inbox, SubscriptionResponseResult.Message.AckInbox, handler);

            return Packet.Message.Inbox;
        }

        public void UnSubscribe(string subscribeInbox)
        {
            if (_localSubscriptionConfig.TryGetValue(subscribeInbox, out var subscriptionConfig))
            {
                //var SubscribePacket = new UnsubscribeRequestPacket( _config.UnsubRequests,_clientId,subject, _replyInboxId,durableName);

                ////订阅侦听消息
                //_channel.WriteAndFlushAsync(SubscribePacket).GetAwaiter().GetResult();

                var Packet = new UnsubscribeRequestPacket(_replyInboxId, _config.UnsubRequests, _clientId, subscriptionConfig.Subject, subscriptionConfig.AckInbox, "KeepLast");

                var UnSubscriptionRequestReady = new TaskCompletionSource<UnSubscriptionResponsePacket>();

                _waitUnSubResponseTaskSchedule[Packet.ReplyTo] = UnSubscriptionRequestReady;

                //发送订阅请求
                _channel.WriteAndFlushAsync(Packet).GetAwaiter().GetResult();

                var UnSubscriptionResult = UnSubscriptionRequestReady.Task.GetAwaiter().GetResult();

                //_localSubscriptionConfig[Packet.Message.Inbox] = new STANSubscriptionConfig(subject, Packet.Message.Inbox, SubscriptionResponseResult.Message.AckInbox, handler);

            }
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


        //TODO:待优化同时发布多个消息
        /// <summary>
        /// 同步发送
        /// </summary>
        /// <param name="subject">主体</param>
        /// <param name="datas">数据</param>
        /// <returns></returns>
        public PubAckPacket Publish(string subject,IEnumerable< byte[]> datas)
        {

            foreach (var data in datas)
            {
                var Packet = new PubMsgPacket(_replyInboxId, _config.PubPrefix, _clientId, subject, data);

                var PubAckReady = new TaskCompletionSource<PubAckPacket>();

                _waitPubAckTaskSchedule[Packet.ReplyTo] = PubAckReady;

                //发送订阅请求
                _channel.WriteAsync(Packet).GetAwaiter().GetResult();
            }

            _channel.Flush();

            return null;

            //var AckResult = PubAckReady.Task.GetAwaiter().GetResult();

            //return AckResult;
        }

        /// <summary>
        /// 同步发送
        /// </summary>
        /// <param name="subject">主体</param>
        /// <param name="data">数据</param>
        /// <returns></returns>
        public async Task<PubAckPacket> PublishWaitAckAsync(string subject, byte[] data)
        {

            if (!_channel.Open) return null;

            var Packet = new PubMsgPacket(_replyInboxId, _config.PubPrefix, _clientId, subject, data);

            var PubAckReady = new TaskCompletionSource<PubAckPacket>();

            _waitPubAckTaskSchedule[Packet.ReplyTo] = PubAckReady;

            var PublishTask = _channel.WriteAndFlushAsync(Packet);

            //发送订阅请求
            await PublishTask.ContinueWith(task => { if (task.Status != TaskStatus.RanToCompletion) PubAckReady.SetResult(null); });

            return await PubAckReady.Task;
        }

        /// <summary>
        /// 同步发送
        /// </summary>
        /// <param name="subject">主体</param>
        /// <param name="data">数据</param>
        /// <returns></returns>
        public async Task PublishWaitAckAsync(string subject, params byte[][] datas)
        {

            if (!_channel.Open) return;

            var Packet = new PubMultipleMsgPacket(_replyInboxId);
            var PackWait = new List<Task>(datas.Length);

            foreach (var data in datas)
            {
                var PubAckReady = new TaskCompletionSource<PubAckPacket>();

                var MsgPacket = new PubMsgPacket(_replyInboxId, _config.PubPrefix, _clientId, subject, data);

                _waitPubAckTaskSchedule[MsgPacket.ReplyTo] = PubAckReady;

                PackWait.Add(PubAckReady.Task);

                Packet.MessagePackets.Add(MsgPacket);
            }


            var PublishTask = _channel.WriteAndFlushAsync(Packet);

            //发送订阅请求
            //await PublishTask.ContinueWith(task => { if (task.Status != TaskStatus.RanToCompletion) PubAckReady.SetResult(null); });

            Task.WaitAll(PackWait.ToArray());
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

        public void PubAckCallback(PubAckPacket pubAck)
        {
            //Console.WriteLine($"GUID = {pubAck.Message.Guid} Error = {pubAck.Message.Error}");
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
