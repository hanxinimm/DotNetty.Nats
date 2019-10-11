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
    public partial class STANClient
    {
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


        /// <summary>
        /// 发送消息成功处理确认
        /// </summary>
        /// <param name="subscriptionConfig">订阅配置</param>
        /// <param name="msg">消息</param>
        /// <param name="isAck">是否确认</param>
        protected void AckAsync(STANSubscriptionAsyncConfig subscriptionConfig, MsgProtoPacket msg, bool isAck = true)
        {
            if (isAck)
            {
                _channel.WriteAndFlushAsync(new AckPacket(subscriptionConfig.AckInbox, msg.Message.Subject, msg.Message.Sequence));
            }
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

    }
}
