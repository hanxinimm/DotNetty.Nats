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
using System.Linq;

namespace Hunter.STAN.Client
{
    public class STANClient
    {
        /// <summary>
        /// STAN配置
        /// </summary>
        private readonly STANOptions _options;

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

        /// <summary>
        /// 本地异步订阅配置
        /// </summary>
        private ConcurrentDictionary<string, STANSubscriptionAsyncConfig> _localSubscriptionAsyncConfig;

        public STANClient(STANOptions options)
        {
            _options = options;
            _heartbeatInboxId = GenerateInboxId();
            _replyInboxId = GenerateInboxId();
            _waitPubAckTaskSchedule = new ConcurrentDictionary<string, TaskCompletionSource<PubAckPacket>>();
            _waitSubResponseTaskSchedule = new ConcurrentDictionary<string, TaskCompletionSource<SubscriptionResponsePacket>>();
            _waitUnSubResponseTaskSchedule = new ConcurrentDictionary<string, TaskCompletionSource<UnSubscriptionResponsePacket>>();
            _localSubscriptionConfig = new ConcurrentDictionary<string, STANSubscriptionConfig>();
            _localSubscriptionAsyncConfig = new ConcurrentDictionary<string, STANSubscriptionAsyncConfig>();
            _bootstrap = InitBootstrap();

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
                channel.Pipeline.AddLast(new ReconnectChannelHandler(ReconnectIfNeedAsync));
                channel.Pipeline.AddLast(STANEncoder.Instance, STANDecoder.Instance);
                channel.Pipeline.AddLast(new ErrorPacketHandler());
                channel.Pipeline.AddLast(new HeartbeatPacketHandler());
                channel.Pipeline.AddLast(new MessagePacketHandler(AckAsync));
                channel.Pipeline.AddLast(new PubAckPacketSyncHandler(_waitPubAckTaskSchedule));
                channel.Pipeline.AddLast(new PubAckPacketAsynHandler(PubAckCallback));
                channel.Pipeline.AddLast(new SubscriptionResponsePacketSyncHandler(_waitSubResponseTaskSchedule));
                channel.Pipeline.AddLast(new SubscriptionResponsePacketAsynHandler((subReps) => { }));
                channel.Pipeline.AddLast(new UnSubscriptionResponsePacketSyncHandler(_waitUnSubResponseTaskSchedule));
                channel.Pipeline.AddLast(new UnSubscriptionResponsePacketAsynHandler((unSubReps) => { }));
                channel.Pipeline.AddLast(new PingPacketHandler());
                channel.Pipeline.AddLast(new PongPacketHandler());
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
                //await this.semaphoreSlim.WaitAsync();
                try
                {
                    if (this.IsChannelInactive)
                    {
                        while (true)
                        {
                            try
                            {
                                _channel = await _bootstrap.ConnectAsync(socketAddress);
                                break;
                            }
                            catch (Exception ex)
                            {
                                await Task.Delay(TimeSpan.FromSeconds(3));
                            }
                        }
                       // this.clientRpcHandler = channel.Pipeline.Get<RpcClientHandler>();
                    }
                }
                finally
                {
                    //this.semaphoreSlim.Release();
                }
            }
        }


        public async Task ContentcAsync()
        {
            //var _bootstrap = InitBootstrap();

            if (!_options.ClusterNodes.Any())
            {
                IPHostEntry hostInfo = Dns.GetHostEntry(_options.Host);
                _options.ClusterNodes.AddRange(hostInfo.AddressList.Select(v => new IPEndPoint(v, _options.Port)));
            }

            var ClusterNode = _options.ClusterNodes.First();

            _channel = await _bootstrap.ConnectAsync(ClusterNode);

            await SubscribeHeartBeatInboxAsync();

            await SubscribeReplyInboxAsync();

            _config = await ConnectRequestAsync();
        }

        private async Task<STANConnectionConfig> ConnectRequestAsync()
        {

            var Packet = new ConnectRequestPacket(_replyInboxId, _options.ClusterID, _options.ClientId, _heartbeatInboxId);

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

        #region 订阅

        public string Subscribe(string subject, string queueGroup, Func<STANMsgContent, bool> handler)
        {
            CheckSubject(subject);
            CheckQueueGroup(queueGroup);
            return Subscribe(subject, queueGroup, string.Empty, new STANSubscribeOptions() { Position = StartPosition.NewOnly }, handler);
        }

        public string Subscribe(string subject, string queueGroup, STANSubscribeOptions subscribeOptions, Func<STANMsgContent, bool> handler)
        {
            CheckSubject(subject);
            CheckQueueGroup(queueGroup);
            return Subscribe(subject, queueGroup, string.Empty, subscribeOptions, handler);
        }

        public string SubscribeDurable(string subject, string durableName, Func<STANMsgContent, bool> handler)
        {
            CheckSubject(subject);
            return Subscribe(subject, string.Empty, durableName, new STANSubscribeOptions() { Position = StartPosition.LastReceived }, handler);
        }

        public string SubscribeDurable(string subject, string queueGroup, string durableName, Func<STANMsgContent, bool> handler)
        {
            CheckSubject(subject);
            CheckQueueGroup(queueGroup);
            return Subscribe(subject, queueGroup, durableName, new STANSubscribeOptions() { Position = StartPosition.LastReceived }, handler);
        }

        public string Subscribe(string subject, string queueGroup, string durableName, STANSubscribeOptions subscribeOptions, Func<STANMsgContent, bool> handler)
        {
            var SubscribePacket = new SubscribePacket(DateTime.Now.Ticks.ToString());

            //订阅侦听消息
            _channel.WriteAndFlushAsync(SubscribePacket).GetAwaiter().GetResult();

            var Packet = new SubscriptionRequestPacket(
                    _replyInboxId,
                    _config.SubRequests,
                    _options.ClientId,
                    subject,
                    queueGroup,
                    SubscribePacket.Subject,
                    subscribeOptions.MaxInFlight ?? 1024,
                    subscribeOptions.AckWaitInSecs ?? 30,
                    durableName,
                    subscribeOptions.Position);

            if (subscribeOptions.StartSequence.HasValue)
            {
                Packet.Message.StartSequence = subscribeOptions.StartSequence.Value;
            }

            if (subscribeOptions.StartTimeDelta.HasValue)
            {
                Packet.Message.StartTimeDelta = subscribeOptions.StartTimeDelta.Value;
            }

            var SubscriptionResponseReady = new TaskCompletionSource<SubscriptionResponsePacket>();

            _waitSubResponseTaskSchedule[Packet.ReplyTo] = SubscriptionResponseReady;

            //发送订阅请求
            _channel.WriteAndFlushAsync(Packet).GetAwaiter().GetResult();

            var SubscriptionResponseResult = SubscriptionResponseReady.Task.GetAwaiter().GetResult();

            _localSubscriptionConfig[Packet.Message.Inbox] = new STANSubscriptionConfig(subject, Packet.Message.Inbox, SubscriptionResponseResult.Message.AckInbox, handler);

            return Packet.Message.Inbox;
        }

        #endregion;

        #region; 异步订阅

        public Task<string> SubscribeAsync(string subject, string queueGroup, Func<STANMsgContent, ValueTask<bool>> handler)
        {
            CheckSubject(subject);
            CheckQueueGroup(queueGroup);
            return SubscribeAsync(subject, queueGroup, string.Empty, new STANSubscribeOptions() { Position = StartPosition.NewOnly }, handler);
        }
        public Task<string> SubscribeAsync(string subject, string queueGroup, STANSubscribeOptions subscribeOptions, Func<STANMsgContent, ValueTask<bool>> handler)
        {
            CheckSubject(subject);
            CheckQueueGroup(queueGroup);
            return SubscribeAsync(subject, queueGroup, string.Empty, subscribeOptions, handler);
        }
        public Task<string> SubscribeDurableAsync(string subject, string durableName, Func<STANMsgContent, ValueTask<bool>> handler)
        {
            return SubscribeAsync(subject, string.Empty, durableName, new STANSubscribeOptions() { Position = StartPosition.LastReceived }, handler);
        }
        public Task<string> SubscribeDurableAsync(string subject, string durableName, STANSubscribeOptions subscribeOptions, Func<STANMsgContent, ValueTask<bool>> handler)
        {
            return SubscribeAsync(subject, string.Empty, durableName, subscribeOptions, handler);
        }
        public Task<string> SubscribeDurableAsync(string subject, string queueGroup, string durableName, Func<STANMsgContent, ValueTask<bool>> handler)
        {
            CheckSubject(subject);
            CheckQueueGroup(queueGroup);
            return SubscribeAsync(subject, queueGroup, durableName, new STANSubscribeOptions() { Position = StartPosition.LastReceived }, handler);
        }
        public async Task<string> SubscribeAsync(string subject, string queueGroup, string durableName, STANSubscribeOptions subscribeOptions, Func<STANMsgContent, ValueTask<bool>> handler)
        {
            var SubscribePacket = new SubscribePacket(DateTime.Now.Ticks.ToString());

            //订阅侦听消息
            await _channel.WriteAndFlushAsync(SubscribePacket);

            var Packet = new SubscriptionRequestPacket(
                    _replyInboxId,
                    _config.SubRequests,
                    _options.ClientId,
                    subject,
                    queueGroup,
                    SubscribePacket.Subject,
                    subscribeOptions.MaxInFlight ?? 1024,
                    subscribeOptions.AckWaitInSecs ?? 30,
                    durableName,
                    subscribeOptions.Position);

            if (subscribeOptions.StartSequence.HasValue)
            {
                Packet.Message.StartSequence = subscribeOptions.StartSequence.Value;
            }

            if (subscribeOptions.StartTimeDelta.HasValue)
            {
                Packet.Message.StartTimeDelta = subscribeOptions.StartTimeDelta.Value;
            }

            var SubscriptionResponseReady = new TaskCompletionSource<SubscriptionResponsePacket>();

            _waitSubResponseTaskSchedule[Packet.ReplyTo] = SubscriptionResponseReady;

            //发送订阅请求
            await _channel.WriteAndFlushAsync(Packet);

            var SubscriptionResponseResult = await SubscriptionResponseReady.Task;

            _localSubscriptionAsyncConfig[Packet.Message.Inbox] = new STANSubscriptionAsyncConfig(subject, Packet.Message.Inbox, SubscriptionResponseResult.Message.AckInbox, handler);

            return Packet.Message.Inbox;
        }

        #endregion;

        #region; 订阅指定消息数量自动取消订阅

        public Task<string> SubscribeAsync(string subject, int maxMsg, Func<STANMsgContent, ValueTask<bool>> handler)
        {
            return SubscribeAsync(subject, string.Empty, string.Empty, maxMsg, new STANSubscribeOptions() { Position = StartPosition.NewOnly }, handler);
        }

        public Task<string> SubscribeAsync(string subject, int maxMsg, STANSubscribeOptions subscribeOptions, Func<STANMsgContent, ValueTask<bool>> handler)
        {
            return SubscribeAsync(subject, string.Empty, string.Empty, maxMsg, subscribeOptions, handler);
        }

        public Task<string> SubscribeAsync(string subject, string queueGroup, int maxMsg, Func<STANMsgContent, ValueTask<bool>> handler)
        {
            return SubscribeAsync(subject, queueGroup, string.Empty, maxMsg, new STANSubscribeOptions() { Position = StartPosition.NewOnly }, handler);
        }

        public Task<string> SubscribeAsync(string subject, string queueGroup, int maxMsg, STANSubscribeOptions subscribeOptions, Func<STANMsgContent, ValueTask<bool>> handler)
        {
            return SubscribeAsync(subject, queueGroup, string.Empty, maxMsg, subscribeOptions, handler);
        }

        public async Task<string> SubscribeAsync(string subject, string queueGroup, string durableName, int maxMsg, STANSubscribeOptions subscribeOptions, Func<STANMsgContent, ValueTask<bool>> handler)
        {
            var SubscribePacket = new SubscribePacket(DateTime.Now.Ticks.ToString());

            //订阅侦听消息
            await _channel.WriteAndFlushAsync(SubscribePacket);

            var Packet = new SubscriptionRequestPacket(
                    _replyInboxId,
                    _config.SubRequests,
                    _options.ClientId,
                    subject,
                    queueGroup,
                    SubscribePacket.Subject,
                    subscribeOptions.MaxInFlight ?? 1024,
                    subscribeOptions.AckWaitInSecs ?? 30,
                    durableName,
                    subscribeOptions.Position);

            if (subscribeOptions.StartSequence.HasValue)
            {
                Packet.Message.StartSequence = subscribeOptions.StartSequence.Value;
            }

            if (subscribeOptions.StartTimeDelta.HasValue)
            {
                Packet.Message.StartTimeDelta = subscribeOptions.StartTimeDelta.Value;
            }

            var SubscriptionResponseReady = new TaskCompletionSource<SubscriptionResponsePacket>();

            _waitSubResponseTaskSchedule[Packet.ReplyTo] = SubscriptionResponseReady;

            //发送订阅请求
            await _channel.WriteAndFlushAsync(Packet);

            var SubscriptionResponseResult = await SubscriptionResponseReady.Task;

            _localSubscriptionAsyncConfig[Packet.Message.Inbox] = new STANSubscriptionAsyncConfig(subject, Packet.Message.Inbox, SubscriptionResponseResult.Message.AckInbox, maxMsg, durableName, handler);

            return Packet.Message.Inbox;
        }

        #endregion;

        #region; 读取消息

        public Task<string> ReadAsync(string subject, long sequence, Func<STANMsgContent, ValueTask<bool>> handler)
        {
            return SubscribeAsync(subject, 1, new STANSubscribeOptions() { Position = StartPosition.SequenceStart, StartSequence = (ulong)(sequence < 1 ? 1 : sequence), MaxInFlight = 1 }, handler);
        }

        public Task<string> ReadAsync(string subject, long start, int count, Func<STANMsgContent, ValueTask<bool>> handler)
        {
            return SubscribeAsync(subject, count, new STANSubscribeOptions() { Position = StartPosition.SequenceStart, StartSequence = (ulong)(start < 1 ? 1 : start), MaxInFlight = count }, handler);
        }

        public Task<STANMsgContent> ReadAsync(string subject, long sequence)
        {
            var MessageReady = new TaskCompletionSource<STANMsgContent>();

            SubscribeAsync(subject, 1, new STANSubscribeOptions() { Position = StartPosition.SequenceStart, StartSequence = (ulong)(sequence < 1 ? 1 : sequence), MaxInFlight = 1 },
                (msg) =>
                {
                    MessageReady.SetResult(msg);
                    return _ackSuccessResult;
                });

            return MessageReady.Task;
        }

        public async Task<IEnumerable<STANMsgContent>> ReadAsync(string subject, long start, int count)
        {
            var MessageReady = new TaskCompletionSource<string>();
            var MessageList = new List<STANMsgContent>();

            await SubscribeAsync(subject, count, new STANSubscribeOptions() { Position = StartPosition.SequenceStart, StartSequence = (ulong)(start < 1 ? 1 : start), MaxInFlight = count },
                (msg) =>
                {
                    MessageList.Add(msg);
                    if (MessageList.Count >= count) MessageReady.SetResult(string.Empty);
                    return _ackSuccessResult;
                });

            await MessageReady.Task;

            return MessageList;
        }

        #endregion;

        public void UnSubscribe(string subscribeId,string durableName)
        {
            if (_localSubscriptionConfig.TryRemove(subscribeId, out var subscriptionConfig))
            {
                //var SubscribePacket = new UnsubscribeRequestPacket( _config.UnsubRequests,_clientId,subject, _replyInboxId,durableName);

                ////订阅侦听消息
                //_channel.WriteAndFlushAsync(SubscribePacket).GetAwaiter().GetResult();

                var Packet = new UnsubscribeRequestPacket(_replyInboxId, _config.UnsubRequests, _options.ClientId, subscriptionConfig.Subject, subscriptionConfig.AckInbox, durableName);

                var UnSubscriptionRequestReady = new TaskCompletionSource<UnSubscriptionResponsePacket>();

                _waitUnSubResponseTaskSchedule[Packet.ReplyTo] = UnSubscriptionRequestReady;

                //发送订阅请求
                _channel.WriteAndFlushAsync(Packet).GetAwaiter().GetResult();

                var UnSubscriptionResult = UnSubscriptionRequestReady.Task.GetAwaiter().GetResult();

                //_localSubscriptionConfig[Packet.Message.Inbox] = new STANSubscriptionConfig(subject, Packet.Message.Inbox, SubscriptionResponseResult.Message.AckInbox, handler);

            }
        }

        public Task UnSubscribeAsync(string subscribeId, string durableName)
        {
            if (_localSubscriptionAsyncConfig.TryRemove(subscribeId, out var subscriptionConfig))
            {
                var Packet = new UnsubscribeRequestPacket(_replyInboxId, _config.UnsubRequests, _options.ClientId, subscriptionConfig.Subject, subscriptionConfig.AckInbox, durableName);

                //发送取消订阅请求
                return _channel.WriteAndFlushAsync(Packet);
            }

            return Task.CompletedTask;
        }

        /// <summary>
        /// 同步发送
        /// </summary>
        /// <param name="subject">主体</param>
        /// <param name="data">数据</param>
        /// <returns></returns>
        public PubAckPacket Publish(string subject, byte[] data)
        {

            var Packet = new PubMsgPacket(_replyInboxId, _config.PubPrefix, _options.ClientId, subject, data);

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
        public IEnumerable<PubAckPacket> Publish(string subject, IEnumerable<byte[]> datas)
        {
            var MessageLength = datas.Count();
            var WaitPubAckTaskSchedules = new Task<PubAckPacket>[MessageLength];

            for (var i = 0; i < MessageLength; i++)
            {
                var Packet = new PubMsgPacket(_replyInboxId, _config.PubPrefix, _options.ClientId, subject, datas.ElementAt(i));

                var PubAckReady = new TaskCompletionSource<PubAckPacket>();

                _waitPubAckTaskSchedule[Packet.ReplyTo] = PubAckReady;

                WaitPubAckTaskSchedules[i] = PubAckReady.Task;

                //发送订阅请求
                _channel.WriteAsync(Packet).GetAwaiter().GetResult();
            }

            _channel.Flush();

            Task.WaitAll(WaitPubAckTaskSchedules.ToArray());

            return WaitPubAckTaskSchedules.Select(v => v.Result);
        }

        /// <summary>
        /// 同步发送
        /// </summary>
        /// <param name="subject">主体</param>
        /// <param name="data">数据</param>
        /// <returns></returns>
        public async Task<PubAckPacket> PublishWaitAckAsync(string subject, byte[] data)
        {

            var Packet = new PubMsgPacket(_replyInboxId, _config.PubPrefix, _options.ClientId, subject, data);

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
        public void PublishWaitAck(string subject, params byte[][] datas)
        {

            if (!_channel.Open) return;

            var Packet = new PubMultipleMsgPacket(_replyInboxId);
            var PackWait = new List<Task>(datas.Length);

            foreach (var data in datas)
            {
                var PubAckReady = new TaskCompletionSource<PubAckPacket>();

                var MsgPacket = new PubMsgPacket(_replyInboxId, _config.PubPrefix, _options.ClientId, subject, data);

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
            var Packet = new PubMsgPacket(_replyInboxId, _config.PubPrefix, _options.ClientId, subject, data);

            return _channel.WriteAndFlushAsync(Packet);
        }

        #region 消息发送确认

        protected void PubAckCallback(PubAckPacket pubAck)
        {
            //Console.WriteLine($"GUID = {pubAck.Message.Guid} Error = {pubAck.Message.Error}");
        }
        protected Task AckAsync(IChannel bootstrapChannel, string subject, ulong sequence)
        {
            var AckInbox = string.Empty;

            var Packet = new AckPacket(AckInbox, subject, sequence);

            //发送消息成功处理
            return bootstrapChannel.WriteAndFlushAsync(Packet);
        }
        protected void AckAsync(IChannel channel, MsgProtoPacket msg)
        {
            if (_localSubscriptionConfig.TryGetValue(msg.Subject, out var subscriptionConfig))
            {
                if (AutoUnSubscribe(subscriptionConfig))
                {
                    var HandlerResult = subscriptionConfig.Handler(new STANMsgContent()
                    {
                        Sequence = msg.Message.Sequence,
                        Subject = msg.Message.Subject,
                        Reply = msg.Message.Reply,
                        Data = msg.Message.Data.ToByteArray(),
                        Timestamp = msg.Message.Timestamp,
                        Redelivered = msg.Message.Redelivered,
                        CRC32 = msg.Message.CRC32
                    });

                    if (HandlerResult)
                    {
                        //发送消息成功处理
                        _channel.WriteAndFlushAsync(new AckPacket(subscriptionConfig.AckInbox, msg.Message.Subject, msg.Message.Sequence));
                    }
                }
                else
                {
                    //发送消息成功处理
                    _channel.WriteAndFlushAsync(new AckPacket(subscriptionConfig.AckInbox, msg.Message.Subject, msg.Message.Sequence));
                }
            }
            else if (_localSubscriptionAsyncConfig.TryGetValue(msg.Subject, out var subscriptionAsyncConfig))
            {

                if (AutoUnSubscribeAsync(subscriptionAsyncConfig))
                {

                    var AsyncHandlerResult = subscriptionAsyncConfig.Handler(new STANMsgContent()
                    {
                        Sequence = msg.Message.Sequence,
                        Subject = msg.Message.Subject,
                        Reply = msg.Message.Reply,
                        Data = msg.Message.Data.ToByteArray(),
                        Timestamp = msg.Message.Timestamp,
                        Redelivered = msg.Message.Redelivered,
                        CRC32 = msg.Message.CRC32
                    }).GetAwaiter().GetResult();


                    if (AsyncHandlerResult)
                    {
                        //发送消息成功处理
                        _channel.WriteAndFlushAsync(new AckPacket(subscriptionAsyncConfig.AckInbox, msg.Message.Subject, msg.Message.Sequence));
                    }
                }
                else
                {
                    //发送消息成功处理
                    _channel.WriteAndFlushAsync(new AckPacket(subscriptionAsyncConfig.AckInbox, msg.Message.Subject, msg.Message.Sequence));
                }
            }
        }
        protected bool AutoUnSubscribe(STANSubscriptionConfig subscriptionConfig)
        {
            if (subscriptionConfig.MaxMsg.HasValue)
            {
                subscriptionConfig.MaxMsg--;

                if (subscriptionConfig.MaxMsg < 0)
                {
                    UnSubscribe(subscriptionConfig.Inbox, subscriptionConfig.DurableName);
                    return true;
                }
                else
                {
                    return false;
                }
            }
            return true;
        }
        protected bool AutoUnSubscribeAsync(STANSubscriptionAsyncConfig subscriptionConfig)
        {

            if (subscriptionConfig.MaxMsg.HasValue)
            {
                subscriptionConfig.MaxMsg--;

                if (subscriptionConfig.MaxMsg < 0)
                {
                    UnSubscribeAsync(subscriptionConfig.Inbox, subscriptionConfig.DurableName).GetAwaiter().GetResult();
                    return false;
                }
                else
                {
                    return true;
                }
            }
            return true;
        }

        #endregion;

        public async Task<CloseResponsePacket> CloseRequestAsync()
        {

            var Packet = new CloseRequestPacket(_replyInboxId, _config.CloseRequests, _options.ClientId);

            var CloseRequestReady = new TaskCompletionSource<CloseResponsePacket>();

            var Handler = new ReplyPacketHandler<CloseResponsePacket>(Packet.ReplyTo, CloseRequestReady);

            _channel.Pipeline.AddLast(Handler);

            //发送关闭
            await _channel.WriteAndFlushAsync(Packet);

            var Result = await CloseRequestReady.Task;

            _channel.Pipeline.Remove(Handler);

            return Result;
        }

        private void CheckSubject(string subject)
        {
            if (string.IsNullOrEmpty(subject)) throw new ArgumentNullException(nameof(subject));
        }

        private void CheckQueueGroup(string queueGroup)
        {
            if (string.IsNullOrEmpty(queueGroup)) throw new ArgumentNullException(nameof(queueGroup));
        }

        private static string GenerateInboxId()
        {
            return Guid.NewGuid().ToString("N");
        }

    }
}
