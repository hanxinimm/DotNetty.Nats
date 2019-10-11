﻿using DotNetty.Codecs.STAN;
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
    public partial class STANClient
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
                channel.Pipeline.AddLast(new MessagePacketHandler(MsgAck));
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

        #region 订阅 同步回调

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

        #region; 订阅指定消息数量自动取消订阅 异步回调

        public string Subscribe(string subject, int maxMsg, Func<STANMsgContent, ValueTask<bool>> handler)
        {
            return Subscribe(subject, string.Empty, string.Empty, maxMsg, new STANSubscribeOptions() { Position = StartPosition.NewOnly }, handler);
        }

        public string Subscribe(string subject, int maxMsg, STANSubscribeOptions subscribeOptions, Func<STANMsgContent, ValueTask<bool>> handler)
        {
            return Subscribe(subject, string.Empty, string.Empty, maxMsg, subscribeOptions, handler);
        }

        public string Subscribe(string subject, string queueGroup, int maxMsg, Func<STANMsgContent, ValueTask<bool>> handler)
        {
            return Subscribe(subject, queueGroup, string.Empty, maxMsg, new STANSubscribeOptions() { Position = StartPosition.NewOnly }, handler);
        }

        public string Subscribe(string subject, string queueGroup, int maxMsg, STANSubscribeOptions subscribeOptions, Func<STANMsgContent, ValueTask<bool>> handler)
        {
            return Subscribe(subject, queueGroup, string.Empty, maxMsg, subscribeOptions, handler);
        }

        public string Subscribe(string subject, string queueGroup, string durableName, int maxMsg, STANSubscribeOptions subscribeOptions, Func<STANMsgContent, ValueTask<bool>> handler)
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

            var SubscriptionResponseResult = SubscriptionResponseReady.Task.GetAwaiter().GetResult(); ;

            _localSubscriptionAsyncConfig[Packet.Message.Inbox] = new STANSubscriptionAsyncConfig(subject, Packet.Message.Inbox, SubscriptionResponseResult.Message.AckInbox, maxMsg, durableName, handler);

            return Packet.Message.Inbox;
        }

        #endregion;

        #region; 读取消息

        public void Read(string subject, long sequence, Func<STANMsgContent, ValueTask<bool>> handler)
        {
            Subscribe(subject, 1, new STANSubscribeOptions() { Position = StartPosition.SequenceStart, StartSequence = (ulong)(sequence < 1 ? 1 : sequence), MaxInFlight = 1 }, handler);
        }

        public void Read(string subject, long start, int count, Func<STANMsgContent, ValueTask<bool>> handler)
        {
            Subscribe(subject, count, new STANSubscribeOptions() { Position = StartPosition.SequenceStart, StartSequence = (ulong)(start < 1 ? 1 : start), MaxInFlight = count }, handler);
        }

        public STANMsgContent Read(string subject, long sequence)
        {
            var MessageReady = new TaskCompletionSource<STANMsgContent>();

            Subscribe(subject, 1, new STANSubscribeOptions() { Position = StartPosition.SequenceStart, StartSequence = (ulong)(sequence < 1 ? 1 : sequence), MaxInFlight = 1 },
                (msg) =>
                {
                    MessageReady.SetResult(msg);
                    return _ackSuccessResult;
                });

            return MessageReady.Task.GetAwaiter().GetResult();
        }

        public IEnumerable<STANMsgContent> Read(string subject, long start, int count)
        {
            var MessageReady = new TaskCompletionSource<string>();
            var MessageList = new List<STANMsgContent>();

            Subscribe(subject, count, new STANSubscribeOptions() { Position = StartPosition.SequenceStart, StartSequence = (ulong)(start < 1 ? 1 : start), MaxInFlight = count },
                (msg) =>
                {
                    MessageList.Add(msg);
                    if (MessageList.Count >= count) MessageReady.SetResult(string.Empty);
                    return _ackSuccessResult;
                });

            MessageReady.Task.GetAwaiter().GetResult();

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

        #region 消息发送确认

        protected void PubAckCallback(PubAckPacket pubAck)
        {
            //Console.WriteLine($"GUID = {pubAck.Message.Guid} Error = {pubAck.Message.Error}");
        }

        protected void MsgAck(IChannel channel, MsgProtoPacket msg)
        {
            if (_localSubscriptionConfig.TryGetValue(msg.Subject, out var subscriptionConfig))
            {
                if (AutoUnSubscribe(subscriptionConfig))
                {
                    if (subscriptionConfig.IsAutoAck)
                    {
                        if (subscriptionConfig.IsAsyncCallback)
                        {
                            subscriptionConfig.AutoAckAsyncHandler(PackMsgContent(msg)).GetAwaiter().GetResult();
                            //发送消息成功处理
                            Ack(subscriptionConfig, msg);
                        }
                        else
                        {
                            subscriptionConfig.AutoAckHandler(PackMsgContent(msg));
                            //发送消息成功处理
                            Ack(subscriptionConfig, msg);
                        }
                    }
                    else
                    {
                        if (subscriptionConfig.IsAsyncCallback)
                        {
                            var HandlerResult = subscriptionConfig.AsyncHandler(PackMsgContent(msg)).GetAwaiter().GetResult();
                            //发送消息成功处理
                            Ack(subscriptionConfig, msg, HandlerResult);
                        }
                        else
                        {
                            var HandlerResult = subscriptionConfig.Handler(PackMsgContent(msg));
                            //发送消息成功处理
                            Ack(subscriptionConfig, msg, HandlerResult);
                        }
                    }
                }
                else
                {
                    //发送消息成功处理
                    Ack(subscriptionConfig, msg);
                }
            }
            else if (_localSubscriptionAsyncConfig.TryGetValue(msg.Subject, out var subscriptionAsyncConfig))
            {

                if (AutoUnSubscribeAsync(subscriptionAsyncConfig))
                {
                    if (subscriptionAsyncConfig.IsAutoAck)
                    {
                        if (subscriptionAsyncConfig.IsAsyncCallback)
                        {
                            subscriptionAsyncConfig.AutoAckAsyncHandler(PackMsgContent(msg)).GetAwaiter().GetResult();
                        }
                        else
                        {
                            subscriptionAsyncConfig.AutoAckHandler(PackMsgContent(msg));
                        }
                        //发送消息成功处理
                        AckAsync(subscriptionAsyncConfig, msg);
                    }
                    else
                    {
                        if (subscriptionAsyncConfig.IsAsyncCallback)
                        {
                            var AsyncHandlerResult = subscriptionAsyncConfig.AsyncHandler(PackMsgContent(msg)).GetAwaiter().GetResult();
                            //发送消息成功处理
                            AckAsync(subscriptionAsyncConfig, msg, AsyncHandlerResult);
                        }
                        else
                        {
                            var HandlerResult = subscriptionAsyncConfig.Handler(PackMsgContent(msg));
                            //发送消息成功处理
                            AckAsync(subscriptionAsyncConfig, msg, HandlerResult);
                        }

                    }

                }
                else
                {

                    AckAsync(subscriptionAsyncConfig, msg);
                }
            }
        }

        protected STANMsgContent PackMsgContent(MsgProtoPacket msg)
        {
            return new STANMsgContent()
            {
                Sequence = msg.Message.Sequence,
                Subject = msg.Message.Subject,
                Reply = msg.Message.Reply,
                Data = msg.Message.Data.ToByteArray(),
                Timestamp = msg.Message.Timestamp,
                Redelivered = msg.Message.Redelivered,
                CRC32 = msg.Message.CRC32
            };
        }

        /// <summary>
        /// 发送消息成功处理确认
        /// </summary>
        /// <param name="subscriptionConfig">订阅配置</param>
        /// <param name="msg">消息</param>
        /// <param name="isAck">是否确认</param>
        protected void Ack(STANSubscriptionConfig subscriptionConfig, MsgProtoPacket msg, bool isAck = true)
        {
            if (isAck)
            {
                _channel.WriteAndFlushAsync(new AckPacket(subscriptionConfig.AckInbox, msg.Message.Subject, msg.Message.Sequence));
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

        #endregion;

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
