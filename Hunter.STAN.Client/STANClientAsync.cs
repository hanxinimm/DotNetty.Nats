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
using System.Threading;

namespace Hunter.STAN.Client
{
    public sealed partial class STANClient
    {
        public async Task ContentcAsync()
        {
            if (!_options.ClusterNodes.Any())
            {
                IPHostEntry hostInfo = Dns.GetHostEntry(_options.Host);
                _options.ClusterNodes.AddRange(hostInfo.AddressList.Select(v => new IPEndPoint(v, _options.Port)));
            }

            var ClusterNode = _options.ClusterNodes.First();

            if (_config == null)
            {
                _channel = await _bootstrap.ConnectAsync(ClusterNode);

                await SubscribeHeartBeatInboxAsync();

                await SubscribeReplyInboxAsync();

                _config = await ConnectRequestAsync();
            }
            else
            {
                _channel = await _bootstrap.ConnectAsync(ClusterNode);
            }
        }

        private async Task<STANConnectionConfig> ConnectRequestAsync()
        {

            var Packet = new ConnectRequestPacket(_replyInboxId, _options.ClusterID, _clientId, _heartbeatInboxId);

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


        #region 订阅自动确认 异步回调

        public Task<string> SubscribeAsync(string subject, Func<STANMsgContent, Task> handler)
        {
            CheckSubject(subject);
            return SubscribeAsync(subject, string.Empty, string.Empty, new STANSubscribeOptions() { Position = StartPosition.NewOnly }, handler);
        }
        public Task<string> SubscribeAsync(string subject, STANSubscribeOptions subscribeOptions, Func<STANMsgContent, Task> handler)
        {
            CheckSubject(subject);
            return SubscribeAsync(subject, string.Empty, string.Empty, subscribeOptions, handler);
        }

        public Task<string> SubscribeAsync(string subject, string queueGroup, Func<STANMsgContent, Task> handler)
        {
            CheckSubject(subject);
            CheckQueueGroup(queueGroup);
            return SubscribeAsync(subject, queueGroup, string.Empty, new STANSubscribeOptions() { Position = StartPosition.NewOnly }, handler);
        }
        public Task<string> SubscribeAsync(string subject, string queueGroup, STANSubscribeOptions subscribeOptions, Func<STANMsgContent, Task> handler)
        {
            CheckSubject(subject);
            CheckQueueGroup(queueGroup);
            return SubscribeAsync(subject, queueGroup, string.Empty, subscribeOptions, handler);
        }
        

        public Task<string> SubscribeDurableAsync(string subject, string durableName, Func<STANMsgContent, Task> handler)
        {
            return SubscribeAsync(subject, string.Empty, durableName, new STANSubscribeOptions() { Position = StartPosition.LastReceived }, handler);
        }
        public Task<string> SubscribeDurableAsync(string subject, string durableName, STANSubscribeOptions subscribeOptions, Func<STANMsgContent, Task> handler)
        {
            return SubscribeAsync(subject, string.Empty, durableName, subscribeOptions, handler);
        }
        public Task<string> SubscribeDurableAsync(string subject, string queueGroup, string durableName, Func<STANMsgContent, Task> handler)
        {
            CheckSubject(subject);
            CheckQueueGroup(queueGroup);
            return SubscribeAsync(subject, queueGroup, durableName, new STANSubscribeOptions() { Position = StartPosition.LastReceived }, handler);
        }
        
        public async Task<string> SubscribeAsync(string subject, string queueGroup, string durableName, STANSubscribeOptions subscribeOptions, Func<STANMsgContent, Task> handler)
        {
            var SubscribePacket = new SubscribePacket(DateTime.Now.Ticks.ToString());

            //订阅侦听消息
            await _channel.WriteAndFlushAsync(SubscribePacket);

            var Packet = new SubscriptionRequestPacket(
                    _replyInboxId,
                    _config.SubRequests,
                    _clientId,
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

            var stanSubscriptionManager = new STANSubscriptionAsyncManager();

            _subscriptionMessageQueue[Packet.Message.Inbox] = stanSubscriptionManager;

            var SubscriptionResponseReady = new TaskCompletionSource<SubscriptionResponsePacket>();

            _waitSubResponseTaskSchedule[Packet.ReplyTo] = SubscriptionResponseReady;

            //发送订阅请求
            await _channel.WriteAndFlushAsync(Packet);

            var SubscriptionResponseResult = await SubscriptionResponseReady.Task;

            stanSubscriptionManager.SubscriptionConfig = new STANSubscriptionAsyncConfig(subject, Packet.Message.Inbox, SubscriptionResponseResult.Message.AckInbox, handler);

            ThreadPool.QueueUserWorkItem(new WaitCallback(MessageProcessingChannelAsyncConfigAsync), Packet.Message.Inbox);

            return Packet.Message.Inbox;
        }

        #endregion;

        #region 订阅手动确认 同步回调

        public Task<string> SubscribeAsync(string subject, Func<STANMsgContent, bool> handler)
        {
            CheckSubject(subject);
            return SubscribeAsync(subject, string.Empty, string.Empty, new STANSubscribeOptions() { Position = StartPosition.NewOnly }, handler);
        }
        public Task<string> SubscribeAsync(string subject, STANSubscribeOptions subscribeOptions, Func<STANMsgContent, bool> handler)
        {
            CheckSubject(subject);
            return SubscribeAsync(subject, string.Empty, string.Empty, subscribeOptions, handler);
        }

        public Task<string> SubscribeAsync(string subject, string queueGroup, Func<STANMsgContent, bool> handler)
        {
            CheckSubject(subject);
            CheckQueueGroup(queueGroup);
            return SubscribeAsync(subject, queueGroup, string.Empty, new STANSubscribeOptions() { Position = StartPosition.NewOnly }, handler);
        }
        public Task<string> SubscribeAsync(string subject, string queueGroup, STANSubscribeOptions subscribeOptions, Func<STANMsgContent, bool> handler)
        {
            CheckSubject(subject);
            CheckQueueGroup(queueGroup);
            return SubscribeAsync(subject, queueGroup, string.Empty, subscribeOptions, handler);
        }
        
        public Task<string> SubscribeDurableAsync(string subject, string durableName, Func<STANMsgContent, bool> handler)
        {
            return SubscribeAsync(subject, string.Empty, durableName, new STANSubscribeOptions() { Position = StartPosition.LastReceived }, handler);
        }
        public Task<string> SubscribeDurableAsync(string subject, string durableName, STANSubscribeOptions subscribeOptions, Func<STANMsgContent, bool> handler)
        {
            return SubscribeAsync(subject, string.Empty, durableName, subscribeOptions, handler);
        }
        public Task<string> SubscribeDurableAsync(string subject, string queueGroup, string durableName, Func<STANMsgContent, bool> handler)
        {
            CheckSubject(subject);
            CheckQueueGroup(queueGroup);
            return SubscribeAsync(subject, queueGroup, durableName, new STANSubscribeOptions() { Position = StartPosition.LastReceived }, handler);
        }

        public async Task<string> SubscribeAsync(string subject, string queueGroup, string durableName, STANSubscribeOptions subscribeOptions, Func<STANMsgContent, bool> handler)
        {
            var SubscribePacket = new SubscribePacket(DateTime.Now.Ticks.ToString());

            //订阅侦听消息
            await _channel.WriteAndFlushAsync(SubscribePacket);

            var Packet = new SubscriptionRequestPacket(
                    _replyInboxId,
                    _config.SubRequests,
                    _clientId,
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

            var stanSubscriptionManager = new STANSubscriptionAsyncManager();

            _subscriptionMessageQueue[Packet.Message.Inbox] = stanSubscriptionManager;

            var SubscriptionResponseReady = new TaskCompletionSource<SubscriptionResponsePacket>();

            _waitSubResponseTaskSchedule[Packet.ReplyTo] = SubscriptionResponseReady;

            //发送订阅请求
            await _channel.WriteAndFlushAsync(Packet);

            var SubscriptionResponseResult = await SubscriptionResponseReady.Task;

            stanSubscriptionManager.SubscriptionConfig = new STANSubscriptionSyncConfig(subject, Packet.Message.Inbox, SubscriptionResponseResult.Message.AckInbox, handler);

            ThreadPool.QueueUserWorkItem(new WaitCallback(MessageProcessingChannelSyncConfigAsync), Packet.Message.Inbox);

            return Packet.Message.Inbox;
        }

        #endregion;


        #region 订阅自动确认 同步回调

        public Task<string> SubscribeAsync(string subject, Action<STANMsgContent> handler)
        {
            CheckSubject(subject);
            return SubscribeAsync(subject, string.Empty, string.Empty, new STANSubscribeOptions() { Position = StartPosition.NewOnly }, handler);
        }
        public Task<string> SubscribeAsync(string subject, STANSubscribeOptions subscribeOptions, Action<STANMsgContent> handler)
        {
            CheckSubject(subject);
            return SubscribeAsync(subject, string.Empty, string.Empty, subscribeOptions, handler);
        }

        public Task<string> SubscribeAsync(string subject, string queueGroup, Action<STANMsgContent> handler)
        {
            CheckSubject(subject);
            CheckQueueGroup(queueGroup);
            return SubscribeAsync(subject, queueGroup, string.Empty, new STANSubscribeOptions() { Position = StartPosition.NewOnly }, handler);
        }
        public Task<string> SubscribeAsync(string subject, string queueGroup, STANSubscribeOptions subscribeOptions, Action<STANMsgContent> handler)
        {
            CheckSubject(subject);
            CheckQueueGroup(queueGroup);
            return SubscribeAsync(subject, queueGroup, string.Empty, subscribeOptions, handler);
        }
        
        public Task<string> SubscribeDurableAsync(string subject, string durableName, Action<STANMsgContent> handler)
        {
            return SubscribeAsync(subject, string.Empty, durableName, new STANSubscribeOptions() { Position = StartPosition.LastReceived }, handler);
        }
        public Task<string> SubscribeDurableAsync(string subject, string durableName, STANSubscribeOptions subscribeOptions, Action<STANMsgContent> handler)
        {
            return SubscribeAsync(subject, string.Empty, durableName, subscribeOptions, handler);
        }
        public Task<string> SubscribeDurableAsync(string subject, string queueGroup, string durableName, Action<STANMsgContent> handler)
        {
            CheckSubject(subject);
            CheckQueueGroup(queueGroup);
            return SubscribeAsync(subject, queueGroup, durableName, new STANSubscribeOptions() { Position = StartPosition.LastReceived }, handler);
        } 
        public async Task<string> SubscribeAsync(string subject, string queueGroup, string durableName, STANSubscribeOptions subscribeOptions, Action<STANMsgContent> handler)
        {
            var SubscribePacket = new SubscribePacket(DateTime.Now.Ticks.ToString());

            //订阅侦听消息
            await _channel.WriteAndFlushAsync(SubscribePacket);

            var Packet = new SubscriptionRequestPacket(
                    _replyInboxId,
                    _config.SubRequests,
                    _clientId,
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

            var stanSubscriptionManager = new STANSubscriptionAsyncManager();

            _subscriptionMessageQueue[Packet.Message.Inbox] = stanSubscriptionManager;

            var SubscriptionResponseReady = new TaskCompletionSource<SubscriptionResponsePacket>();

            _waitSubResponseTaskSchedule[Packet.ReplyTo] = SubscriptionResponseReady;

            //发送订阅请求
            await _channel.WriteAndFlushAsync(Packet);

            var SubscriptionResponseResult = await SubscriptionResponseReady.Task;

            stanSubscriptionManager.SubscriptionConfig = new STANSubscriptionSyncConfig(subject, Packet.Message.Inbox, SubscriptionResponseResult.Message.AckInbox, handler);

            ThreadPool.QueueUserWorkItem(new WaitCallback(MessageProcessingChannelSyncConfigAsync), Packet.Message.Inbox);

            return Packet.Message.Inbox;
        }

        #endregion;

        #region 订阅手动确认 异步回调

        public Task<string> SubscribeAsync(string subject, Func<STANMsgContent, ValueTask<bool>> handler)
        {
            CheckSubject(subject);
            return SubscribeAsync(subject, string.Empty, string.Empty, new STANSubscribeOptions() { Position = StartPosition.NewOnly }, handler);
        }
        public Task<string> SubscribeAsync(string subject, STANSubscribeOptions subscribeOptions, Func<STANMsgContent, ValueTask<bool>> handler)
        {
            CheckSubject(subject);
            return SubscribeAsync(subject, string.Empty, string.Empty, subscribeOptions, handler);
        }

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
                    _clientId,
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

            var stanSubscriptionManager = new STANSubscriptionAsyncManager();

            _subscriptionMessageQueue[Packet.Message.Inbox] = stanSubscriptionManager;

            var SubscriptionResponseReady = new TaskCompletionSource<SubscriptionResponsePacket>();

            _waitSubResponseTaskSchedule[Packet.ReplyTo] = SubscriptionResponseReady;

            //发送订阅请求
            await _channel.WriteAndFlushAsync(Packet);

            var SubscriptionResponseResult = await SubscriptionResponseReady.Task;

            stanSubscriptionManager.SubscriptionConfig = new STANSubscriptionAsyncConfig(subject, Packet.Message.Inbox, SubscriptionResponseResult.Message.AckInbox, handler);

            //TODO;订阅太多可能参数性能影响,考虑优化队列消息处理的逻辑
            ThreadPool.QueueUserWorkItem(new WaitCallback(MessageProcessingChannelAsyncConfig), Packet.Message.Inbox);

            return Packet.Message.Inbox;
        }

        #endregion;


        #region; 订阅指定消息数量自动取消订阅

        public Task SubscribeAsync(string subject, int maxMsg, Func<STANMsgContent, ValueTask<bool>> handler)
        {
            return SubscribeAsync(subject, string.Empty, string.Empty, maxMsg, new STANSubscribeOptions() { Position = StartPosition.NewOnly }, handler);
        }

        public Task SubscribeAsync(string subject, int maxMsg, STANSubscribeOptions subscribeOptions, Func<STANMsgContent, ValueTask<bool>> handler)
        {
            return SubscribeAsync(subject, string.Empty, string.Empty, maxMsg, subscribeOptions, handler);
        }

        public Task SubscribeAsync(string subject, string queueGroup, int maxMsg, Func<STANMsgContent, ValueTask<bool>> handler)
        {
            return SubscribeAsync(subject, queueGroup, string.Empty, maxMsg, new STANSubscribeOptions() { Position = StartPosition.NewOnly }, handler);
        }

        public Task SubscribeAsync(string subject, string queueGroup, int maxMsg, STANSubscribeOptions subscribeOptions, Func<STANMsgContent, ValueTask<bool>> handler)
        {
            return SubscribeAsync(subject, queueGroup, string.Empty, maxMsg, subscribeOptions, handler);
        }

        public async Task SubscribeAsync(string subject, string queueGroup, string durableName, int maxMsg, STANSubscribeOptions subscribeOptions, Func<STANMsgContent, ValueTask<bool>> handler)
        {
            var SubscribePacket = new SubscribePacket(DateTime.Now.Ticks.ToString());

            //订阅侦听消息
            await _channel.WriteAndFlushAsync(SubscribePacket);

            var Packet = new SubscriptionRequestPacket(
                    _replyInboxId,
                    _config.SubRequests,
                    _clientId,
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

            var stanSubscriptionManager = new STANSubscriptionAsyncManager();

            _subscriptionMessageQueue[Packet.Message.Inbox] = stanSubscriptionManager; 

            var SubscriptionResponseReady = new TaskCompletionSource<SubscriptionResponsePacket>();

            _waitSubResponseTaskSchedule[Packet.ReplyTo] = SubscriptionResponseReady;

            //发送订阅请求
            await _channel.WriteAndFlushAsync(Packet);

            var SubscriptionResponseResult = await SubscriptionResponseReady.Task;

            stanSubscriptionManager.SubscriptionConfig = new STANSubscriptionAsyncConfig(subject, Packet.Message.Inbox, SubscriptionResponseResult.Message.AckInbox, maxMsg, durableName, handler);

            await MessageProcessingChannelWithAutoUnSubscribeAsyncConfigAsync(stanSubscriptionManager);

        }

        #endregion;

        #region; 读取消息

        public Task<Queue<STANMsgContent>> ReadAsync(string subject, long sequence)
        {
            return ReadAsync(subject, sequence, 1, new STANSubscribeOptions() { Position = StartPosition.SequenceStart, StartSequence = (ulong)(sequence < 1 ? 1 : sequence), MaxInFlight = 1 });
        }

        public Task<Queue<STANMsgContent>> ReadAsync(string subject, long start, int count)
        {
            return ReadAsync(subject, start, count, new STANSubscribeOptions() { Position = StartPosition.SequenceStart, StartSequence = (ulong)(start < 1 ? 1 : start), MaxInFlight = count });
        }

        private async Task<Queue<STANMsgContent>> ReadAsync(string subject, long sequence, int count, STANSubscribeOptions subscribeOptions)
        {
            var SubscribePacket = new SubscribePacket(DateTime.Now.Ticks.ToString());

            //订阅侦听消息
            await _channel.WriteAndFlushAsync(SubscribePacket);

            var Packet = new SubscriptionRequestPacket(
                    _replyInboxId,
                    _config.SubRequests,
                    _clientId,
                    subject,
                    string.Empty,
                    SubscribePacket.Subject,
                    subscribeOptions.MaxInFlight ?? 1024,
                    subscribeOptions.AckWaitInSecs ?? 30,
                    string.Empty,
                    subscribeOptions.Position);

            if (subscribeOptions.StartSequence.HasValue)
            {
                Packet.Message.StartSequence = subscribeOptions.StartSequence.Value;
            }

            if (subscribeOptions.StartTimeDelta.HasValue)
            {
                Packet.Message.StartTimeDelta = subscribeOptions.StartTimeDelta.Value;
            }

            var stanSubscriptionManager = new STANSubscriptionAutomaticAsyncManager(count);

            _subscriptionMessageQueue[Packet.Message.Inbox] = stanSubscriptionManager;

            var SubscriptionResponseReady = new TaskCompletionSource<SubscriptionResponsePacket>();

            _waitSubResponseTaskSchedule[Packet.ReplyTo] = SubscriptionResponseReady;

            //发送订阅请求
            await _channel.WriteAndFlushAsync(Packet);

            var SubscriptionResponseResult = await SubscriptionResponseReady.Task;

            stanSubscriptionManager.SubscriptionConfig = new STANSubscriptionConfig(subject, Packet.Message.Inbox, SubscriptionResponseResult.Message.AckInbox, count);

            await MessageProcessingChannelWithAutoUnSubscribeAsync(stanSubscriptionManager);

            return stanSubscriptionManager.Messages;
        }

        #endregion;

        private async ValueTask UnSubscribeAsync(STANSubscriptionAsyncManager stanSubscriptionManager)
        {
            if (_subscriptionMessageQueue.TryRemove(stanSubscriptionManager.SubscriptionConfig.Inbox, out _))
            {
                var Packet = new UnsubscribeRequestPacket(_replyInboxId, 
                    _config.UnsubRequests,
                    _clientId, 
                    stanSubscriptionManager.SubscriptionConfig.Subject, 
                    stanSubscriptionManager.SubscriptionConfig.AckInbox,
                    stanSubscriptionManager.SubscriptionConfig.DurableName);

                //发送取消订阅请求
                await _channel.WriteAndFlushAsync(Packet);
            }
        }

        /// <summary>
        /// 同步发送
        /// </summary>
        /// <param name="subject">主体</param>
        /// <param name="data">数据</param>
        /// <returns></returns>
        public async Task<PubAckPacket> PublishWaitAckAsync(string subject, byte[] data)
        {

            var Packet = new PubMsgPacket(_replyInboxId, _config.PubPrefix, _clientId, subject, data);

            var PubAckReady = new TaskCompletionSource<PubAckPacket>();

            _waitPubAckTaskSchedule[Packet.ReplyTo] = PubAckReady;

            var PublishTask = _channel.WriteAndFlushAsync(Packet);

            //发送订阅请求
            await PublishTask.ContinueWith(task => { if (task.Status != TaskStatus.RanToCompletion) PubAckReady.SetResult(null); });

            return await PubAckReady.Task;
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

        #region 消息发送确认


        /// <summary>
        /// 发送消息成功处理确认
        /// </summary>
        /// <param name="subscriptionConfig">订阅配置</param>
        /// <param name="msg">消息</param>
        /// <param name="isAck">是否确认</param>
        private async ValueTask AckAsync(STANSubscriptionConfig subscriptionConfig, MsgProtoPacket msg, bool isAck = true)
        {
            if (isAck)
            {
                await _channel.WriteAndFlushAsync(new AckPacket(subscriptionConfig.AckInbox, msg.Message.Subject, msg.Message.Sequence));
            }
        }

        #endregion;

        public async Task<CloseResponsePacket> CloseRequestAsync()
        {

            var Packet = new CloseRequestPacket(_replyInboxId, _config.CloseRequests, _clientId);

            var CloseRequestReady = new TaskCompletionSource<CloseResponsePacket>();

            var Handler = new ReplyPacketHandler<CloseResponsePacket>(Packet.ReplyTo, CloseRequestReady);

            _channel.Pipeline.AddLast(Handler);

            //发送关闭
            await _channel.WriteAndFlushAsync(Packet);

            var Result = await CloseRequestReady.Task;

            _channel.Pipeline.Remove(Handler);

            return Result;
        }

        private void MessageProcessingChannelAsyncConfigAsync(object messageInBox)
        {
            var stanSubscriptionManager = _subscriptionMessageQueue[messageInBox.ToString()] as STANSubscriptionAsyncManager;
            var subscriptionConfig = stanSubscriptionManager.SubscriptionConfig as STANSubscriptionAsyncConfig;

            Task.Run(async () =>
            {
                while (true)
                {
                    stanSubscriptionManager.QueueEventWaitHandle.Reset();

                    if (stanSubscriptionManager.SubscriptionConfig.IsAutoAck)
                    {
                        await MessageProcessingChannelWithAutoAckAsync(stanSubscriptionManager, subscriptionConfig);
                    }
                    else
                    {
                        await MessageProcessingChannelWithCustomAckAsync(stanSubscriptionManager, subscriptionConfig);
                    }

                    stanSubscriptionManager.QueueEventWaitHandle.WaitOne();
                }
            });
        }

        private void MessageProcessingChannelSyncConfigAsync(object messageInBox)
        {
            var stanSubscriptionManager = _subscriptionMessageQueue[messageInBox.ToString()] as STANSubscriptionAsyncManager;
            var subscriptionConfig = stanSubscriptionManager.SubscriptionConfig as STANSubscriptionSyncConfig;

            Task.Run(async () =>
            {
                while (true)
                {
                    stanSubscriptionManager.QueueEventWaitHandle.Reset();

                    if (stanSubscriptionManager.SubscriptionConfig.IsAutoAck)
                    {
                        await MessageProcessingChannelWithAutoAckAsync(stanSubscriptionManager, subscriptionConfig);
                    }
                    else
                    {
                        await MessageProcessingChannelWithCustomAckAsync(stanSubscriptionManager, subscriptionConfig);
                    }

                    stanSubscriptionManager.QueueEventWaitHandle.WaitOne();
                }
            });
        }

        private async Task MessageProcessingChannelWithAutoAckAsync(STANSubscriptionAsyncManager stanSubscriptionManager, STANSubscriptionAsyncConfig subscriptionConfig)
        {
            while (stanSubscriptionManager.MessageQueues.TryDequeue(out var msgPacket))
            {
                await subscriptionConfig.AutoAckAsyncHandler(PackMsgContent(msgPacket));
                //发送消息成功处理
                await AckAsync(subscriptionConfig, msgPacket);
            }
        }
        private async Task MessageProcessingChannelWithCustomAckAsync(STANSubscriptionAsyncManager stanSubscriptionManager, STANSubscriptionAsyncConfig subscriptionConfig)
        {
            while (stanSubscriptionManager.MessageQueues.TryDequeue(out var msgPacket))
            {
                var AsyncHandlerResult = await subscriptionConfig.AsyncHandler(PackMsgContent(msgPacket));
                //发送消息成功处理
                await AckAsync(subscriptionConfig, msgPacket, AsyncHandlerResult);
            }
        }

        private async Task MessageProcessingChannelWithAutoAckAsync(STANSubscriptionAsyncManager stanSubscriptionManager, STANSubscriptionSyncConfig subscriptionConfig)
        {
            while (stanSubscriptionManager.MessageQueues.TryDequeue(out var msgPacket))
            {
                subscriptionConfig.AutoAckHandler(PackMsgContent(msgPacket));
                //发送消息成功处理
                await AckAsync(subscriptionConfig, msgPacket);
            }
        }
        private async Task MessageProcessingChannelWithCustomAckAsync(STANSubscriptionAsyncManager stanSubscriptionManager, STANSubscriptionSyncConfig subscriptionConfig)
        {
            while (stanSubscriptionManager.MessageQueues.TryDequeue(out var msgPacket))
            {
                var HandlerResult = subscriptionConfig.Handler(PackMsgContent(msgPacket));
                //发送消息成功处理
                await AckAsync(subscriptionConfig, msgPacket, HandlerResult);
            }
        }


        private Task MessageProcessingChannelWithAutoUnSubscribeAsyncConfigAsync(STANSubscriptionAsyncManager stanSubscriptionManager)
        {
            var subscriptionConfig = stanSubscriptionManager.SubscriptionConfig as STANSubscriptionSyncConfig;

            return Task.Factory.StartNew(async () =>
             {
                 if (stanSubscriptionManager.SubscriptionConfig.IsAutoAck)
                 {
                     await MessageProcessingChannelWithAutoUnSubscribeAndAutoAckAsync(stanSubscriptionManager, subscriptionConfig);
                 }
                 else
                 {
                     await MessageProcessingChannelWithAutoUnSubscribeAndCustomAckAsync(stanSubscriptionManager, subscriptionConfig);
                 }

                 await UnSubscribeAsync(stanSubscriptionManager);
             });
        }

        private Task MessageProcessingChannelWithAutoUnSubscribeSyncConfigAsync(STANSubscriptionAsyncManager stanSubscriptionManager)
        {
            var subscriptionConfig = stanSubscriptionManager.SubscriptionConfig as STANSubscriptionSyncConfig;

            return Task.Factory.StartNew(async () =>
            {
                if (stanSubscriptionManager.SubscriptionConfig.IsAutoAck)
                {
                    await MessageProcessingChannelWithAutoUnSubscribeAndAutoAckAsync(stanSubscriptionManager, subscriptionConfig);
                }
                else
                {
                    await MessageProcessingChannelWithAutoUnSubscribeAndCustomAckAsync(stanSubscriptionManager, subscriptionConfig);
                }

                await UnSubscribeAsync(stanSubscriptionManager);
            });
        }

        private async Task MessageProcessingChannelWithAutoUnSubscribeAndAutoAckAsync(STANSubscriptionAsyncManager stanSubscriptionManager, STANSubscriptionAsyncConfig subscriptionConfig)
        {

            for (int i = 0; i < subscriptionConfig.MaxMsg.Value; i++)
            {
                stanSubscriptionManager.QueueEventWaitHandle.WaitOne();

                if (stanSubscriptionManager.MessageQueues.TryDequeue(out var msgPacket))
                {
                    await subscriptionConfig.AutoAckAsyncHandler(PackMsgContent(msgPacket));
                    //发送消息成功处理
                    await AckAsync(subscriptionConfig, msgPacket);
                }
            }
        }

        private async Task MessageProcessingChannelWithAutoUnSubscribeAndCustomAckAsync(STANSubscriptionAsyncManager stanSubscriptionManager, STANSubscriptionAsyncConfig subscriptionConfig)
        {
            for (int i = 0; i < subscriptionConfig.MaxMsg.Value; i++)
            {
                stanSubscriptionManager.QueueEventWaitHandle.WaitOne();

                if (stanSubscriptionManager.MessageQueues.TryDequeue(out var msgPacket))
                {
                    var AsyncHandlerResult = await subscriptionConfig.AsyncHandler(PackMsgContent(msgPacket));
                    //发送消息成功处理
                    await AckAsync(subscriptionConfig, msgPacket, AsyncHandlerResult);
                }
            }
        }

        private async Task MessageProcessingChannelWithAutoUnSubscribeAndAutoAckAsync(STANSubscriptionAsyncManager stanSubscriptionManager, STANSubscriptionSyncConfig subscriptionConfig)
        {
            for (int i = 0; i < subscriptionConfig.MaxMsg.Value; i++)
            {
                stanSubscriptionManager.QueueEventWaitHandle.WaitOne();

                if (stanSubscriptionManager.MessageQueues.TryDequeue(out var msgPacket))
                {
                    subscriptionConfig.AutoAckHandler(PackMsgContent(msgPacket));
                    //发送消息成功处理
                    await AckAsync(subscriptionConfig, msgPacket);
                }
            }
        }

        private async Task MessageProcessingChannelWithAutoUnSubscribeAndCustomAckAsync(STANSubscriptionAsyncManager stanSubscriptionManager, STANSubscriptionSyncConfig subscriptionConfig)
        {
            for (int i = 0; i < subscriptionConfig.MaxMsg.Value; i++)
            {
                stanSubscriptionManager.QueueEventWaitHandle.WaitOne();

                if (stanSubscriptionManager.MessageQueues.TryDequeue(out var msgPacket))
                {
                    var HandlerResult = subscriptionConfig.Handler(PackMsgContent(msgPacket));
                    //发送消息成功处理
                    await AckAsync(subscriptionConfig, msgPacket, HandlerResult);
                }
            }
        }

        //TODO:还有另外一种写法
        private async Task MessageProcessingChannelWithAutoUnSubscribeAsync(STANSubscriptionAutomaticAsyncManager stanSubscriptionManager)
        {

            for (int i = 0; i < stanSubscriptionManager.SubscriptionConfig.MaxMsg.Value; i++)
            {

                if (stanSubscriptionManager.MessageQueues.TryDequeue(out var msgPacket))
                {

                    stanSubscriptionManager.Messages.Enqueue(PackMsgContent(msgPacket));

                    //发送消息成功处理
                    await AckAsync(stanSubscriptionManager.SubscriptionConfig, msgPacket);
                }
                else
                {
                    i--;
                    stanSubscriptionManager.QueueEventWaitHandle.WaitOne();
                }
            }

            await UnSubscribeAsync(stanSubscriptionManager);

        }
    }
}
