using DotNetty.Codecs.NATSJetStream.Packets;
using DotNetty.Codecs.NATSJetStream.Protocol;
using DotNetty.Handlers.NATS;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using DotNetty.Codecs.NATSJetStream;
using static DotNetty.Codecs.NATSJetStream.Protocol.ConsumerConfig;
using Microsoft.Extensions.Logging;
using DotNetty.Codecs.NATS.Packets;
using DotNetty.Codecs.Protocol;

namespace Hunter.NATS.Client
{
    public partial class NATSClient
    {
        /// <summary>
        /// 订阅消息处理器集合
        /// </summary>
        private readonly List<ConsumerMessageHandler> _consumerMessageHandler;

        #region Stream;

        public async Task<InfoResponse> StreamInfoAsync(string name)
        {
            var jetStreamConfig = JetStreamConfig.Builder().SetName(name).Build();

            var Packet = new DotNetty.Codecs.NATSJetStream.Packets.InfoPacket(
                _replyInboxId,
                jetStreamConfig.Name,
                Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(jetStreamConfig, _jetStreamSetting)));

            var InfoResponseReady = new TaskCompletionSource<InfoResponsePacket>();

            var Handler = new ReplyPacketHandler<InfoResponsePacket>(Packet.ReplyTo, InfoResponseReady);

            _channel.Pipeline.AddLast(Handler);

            await _channel.WriteAndFlushAsync(Packet);

            var InfoResponseCancellationToken = new CancellationTokenSource(TimeSpan.FromSeconds(15)).Token.Register(() =>
            {
                InfoResponseReady.TrySetResult(null);
            });

            var InfoResponse = await InfoResponseReady.Task;

            await InfoResponseCancellationToken.DisposeAsync();

            _channel.Pipeline.Remove(Handler);

            //TODO:待优化
            if (InfoResponse == null) throw new ArgumentNullException();

            return InfoResponse.Message;
        }

        public async Task<CreateResponse> StreamCreateAsync(JetStreamConfig jetStreamConfig)
        {
            var Packet = new CreatePacket(
                _replyInboxId,
                jetStreamConfig.Name,
                Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(jetStreamConfig, _jetStreamSetting)));

            var CreateResponseReady = new TaskCompletionSource<CreateResponsePacket>();

            var Handler = new ReplyPacketHandler<CreateResponsePacket>(Packet.ReplyTo, CreateResponseReady);

            _channel.Pipeline.AddLast(Handler);

            await _channel.WriteAndFlushAsync(Packet);

            var CreateResponseCancellationToken = new CancellationTokenSource(TimeSpan.FromSeconds(15)).Token.Register(() =>
            {
                CreateResponseReady.TrySetResult(null);
            });

            var CreateResponse = await CreateResponseReady.Task;

            await CreateResponseCancellationToken.DisposeAsync();

            _channel.Pipeline.Remove(Handler);

            //TODO:待优化
            if (CreateResponse == null) throw new ArgumentNullException();

            return CreateResponse.Message;
        }

        public async Task<UpdateResponse> StreamUpdateAsync(JetStreamConfig jetStreamConfig)
        {
            var Packet = new UpdatePacket(
                _replyInboxId,
                jetStreamConfig.Name,
                Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(jetStreamConfig, _jetStreamSetting)));

            var UpdateResponseReady = new TaskCompletionSource<UpdateResponsePacket>();

            var Handler = new ReplyPacketHandler<UpdateResponsePacket>(Packet.ReplyTo, UpdateResponseReady);

            _channel.Pipeline.AddLast(Handler);

            await _channel.WriteAndFlushAsync(Packet);

            var UpdateResponseCancellationToken = new CancellationTokenSource(TimeSpan.FromSeconds(15)).Token.Register(() =>
            {
                UpdateResponseReady.TrySetResult(null);
            });

            var UpdateResponse = await UpdateResponseReady.Task;

            await UpdateResponseCancellationToken.DisposeAsync();

            _channel.Pipeline.Remove(Handler);

            //TODO:待优化
            if (UpdateResponse == null) throw new ArgumentNullException();

            return UpdateResponse.Message;
        }

        public async Task<NamesResponse> StreamNamesAsync()
        {
            var Packet = new NamesPacket(
                _replyInboxId,
                Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(new IterableRequest(), _jetStreamSetting)));

            var NamesResponseReady = new TaskCompletionSource<NamesResponsePacket>();

            var Handler = new ReplyPacketHandler<NamesResponsePacket>(Packet.ReplyTo, NamesResponseReady);

            _channel.Pipeline.AddLast(Handler);

            await _channel.WriteAndFlushAsync(Packet);

            var NamesResponseCancellationToken = new CancellationTokenSource(TimeSpan.FromSeconds(15)).Token.Register(() =>
            {
                NamesResponseReady.TrySetResult(null);
            });

            var NamesResponse = await NamesResponseReady.Task;

            await NamesResponseCancellationToken.DisposeAsync();

            _channel.Pipeline.Remove(Handler);

            //TODO:待优化
            if (NamesResponse == null) throw new ArgumentNullException();

            return NamesResponse.Message;
        }

        public async Task<ListResponse> StreamListAsync()
        {
            var Packet = new ListPacket(
                _replyInboxId,
                Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(new IterableRequest(), _jetStreamSetting)));

            var ListResponseReady = new TaskCompletionSource<ListResponsePacket>();

            var Handler = new ReplyPacketHandler<ListResponsePacket>(Packet.ReplyTo, ListResponseReady);

            _channel.Pipeline.AddLast(Handler);

            await _channel.WriteAndFlushAsync(Packet);

            var ListResponseCancellationToken = new CancellationTokenSource(TimeSpan.FromSeconds(15)).Token.Register(() =>
            {
                ListResponseReady.TrySetResult(null);
            });

            var ListResponse = await ListResponseReady.Task;

            await ListResponseCancellationToken.DisposeAsync();

            _channel.Pipeline.Remove(Handler);

            //TODO:待优化
            if (ListResponse == null) throw new ArgumentNullException();

            return ListResponse.Message;
        }

        #endregion;

        #region Consumer;

        public Task<ConsumerCreateResponse> ConsumerCreateAsync(
            string streamName,
            ConsumerConfigBuilder consumerConfigBuilder,
            Func<NATSMsgContent, ValueTask> handler)
        {
            var consumer_inbox = Guid.NewGuid().ToString("n");
            consumerConfigBuilder.SetDeliverSubject(consumer_inbox);
            SubscribeAsync(consumer_inbox, handler);
            return ConsumerCreateAsync(new ConsumerCreateRequest(streamName, consumerConfigBuilder.Build()));
        }

        public async Task<ConsumerCreateResponse> ConsumerCreateAsync(ConsumerCreateRequest createRequest)
        {
            var Packet = new ConsumerCreatePacket(
                _replyInboxId,
                createRequest.Stream,
                createRequest.Config.DurableName,
                Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(createRequest, _jetStreamSetting)));

            var ConsumerCreateResponseReady = new TaskCompletionSource<ConsumerCreateResponsePacket>();

            var Handler = new ReplyPacketHandler<ConsumerCreateResponsePacket>(Packet.ReplyTo, ConsumerCreateResponseReady);

            _channel.Pipeline.AddLast(Handler);

            await _channel.WriteAndFlushAsync(Packet);

            var ConsumerCreateResponseCancellationToken = new CancellationTokenSource(TimeSpan.FromSeconds(15)).Token.Register(() =>
            {
                ConsumerCreateResponseReady.TrySetResult(null);
            });

            var ConsumerCreateResponse = await ConsumerCreateResponseReady.Task;

            await ConsumerCreateResponseCancellationToken.DisposeAsync();

            _channel.Pipeline.Remove(Handler);

            //TODO:待优化
            if (ConsumerCreateResponse == null) throw new ArgumentNullException();

            return ConsumerCreateResponse.Message;
        }

        public async Task<ConsumerNamesResponse> ConsumerNamesAsync(string consumerName)
        {
            var Packet = new ConsumerNamesPacket(
                _replyInboxId,
                consumerName);

            var ConsumerNamesResponseReady = new TaskCompletionSource<ConsumerNamesResponsePacket>();

            var Handler = new ReplyPacketHandler<ConsumerNamesResponsePacket>(Packet.ReplyTo, ConsumerNamesResponseReady);

            _channel.Pipeline.AddLast(Handler);

            await _channel.WriteAndFlushAsync(Packet);

            var ConsumerNamesResponseCancellationToken = new CancellationTokenSource(TimeSpan.FromSeconds(15)).Token.Register(() =>
            {
                ConsumerNamesResponseReady.TrySetResult(null);
            });

            var ConsumerNamesResponse = await ConsumerNamesResponseReady.Task;

            await ConsumerNamesResponseCancellationToken.DisposeAsync();

            _channel.Pipeline.Remove(Handler);

            //TODO:待优化
            if (ConsumerNamesResponse == null) throw new ArgumentNullException();

            return ConsumerNamesResponse.Message;
        }

        public async Task<ConsumerListResponse> ConsumerListAsync(string consumerName)
        {
            var Packet = new ConsumerListPacket(
                _replyInboxId,
                consumerName);

            var ConsumerListResponseReady = new TaskCompletionSource<ConsumerListResponsePacket>();

            var Handler = new ReplyPacketHandler<ConsumerListResponsePacket>(Packet.ReplyTo, ConsumerListResponseReady);

            _channel.Pipeline.AddLast(Handler);

            await _channel.WriteAndFlushAsync(Packet);

            var ConsumerListResponseCancellationToken = new CancellationTokenSource(TimeSpan.FromSeconds(15)).Token.Register(() =>
            {
                ConsumerListResponseReady.TrySetResult(null);
            });

            var ConsumerListResponse = await ConsumerListResponseReady.Task;

            await ConsumerListResponseCancellationToken.DisposeAsync();

            _channel.Pipeline.Remove(Handler);

            //TODO:待优化
            if (ConsumerListResponse == null) throw new ArgumentNullException();

            return ConsumerListResponse.Message;
        }

        #endregion;

        #region ConsumerSubscribe;

        public async Task<string> HandleConsumerSubscribeAsync(string subject, string queueGroup,
            Func<NATSConsumerSubscriptionConfig, ConsumerMessageHandler> messageHandlerSetup, string subscribeId = null)
        {
            return await _policy.ExecuteAsync(async () =>
            {
                await CheckConnectAsync();
                return await InternalConsumerSubscribeAsync(subject, queueGroup, messageHandlerSetup, subscribeId);
            });
        }



        public async Task<string> InternalConsumerSubscribeAsync(string subject, string queueGroup,
            Func<NATSConsumerSubscriptionConfig, ConsumerMessageHandler> messageHandlerSetup, string subscribeId = null)
        {
            var SubscribeId = subscribeId ?? $"sid{Interlocked.Increment(ref _subscribeId)}";

            _logger.LogDebug($"设置订阅消息队列订阅编号 Subject = {subject} QueueGroup = {queueGroup} SubscribeId = {SubscribeId}");

            var SubscriptionConfig = new NATSConsumerSubscriptionConfig(subject, SubscribeId, queueGroup);

            //处理订阅响应的管道
            var messageHandler = messageHandlerSetup(SubscriptionConfig);

            _logger.LogDebug($"开始添加消息队列处理器 Subject = {subject} QueueGroup = {queueGroup} SubscribeId = {SubscribeId}");

            //添加订阅响应管道
            _channel.Pipeline.AddLast(messageHandler);

            _logger.LogDebug($"结束添加消息队列处理器 Subject = {subject} QueueGroup = {queueGroup} SubscribeId = {SubscribeId}");


            _logger.LogDebug($"开始发送订阅请求 订阅主题 {subject } 订阅编号 {SubscribeId}");

            var SubscribePacketMsg = new SubscribePacket(SubscribeId, subject, queueGroup);

            await _channel.WriteAndFlushAsync(SubscribePacketMsg);

            _logger.LogDebug($"结束发送订阅请求 订阅主题 {subject } 订阅编号 {SubscribeId}");

            //添加消息处理到消息处理集合
            _consumerMessageHandler.Add(messageHandler);

            return SubscribeId;
        }


        #region 异步处理消息

        public Task<string> ConsumerSubscribeAsync(string subject, string queueGroup, Func<NATSMsgContent, ValueTask> handler, string subscribeId = null)
        {
            return HandleConsumerSubscribeAsync(subject, queueGroup, (config) =>
                new ConsumerMessageAsynHandler(_logger, config, handler, AckAsync), subscribeId: subscribeId);
        }

        public Task<string> ConsumerSubscribeAsync(string subject, Func<NATSMsgContent, ValueTask> handler, string subscribeId = null)
        {
            return HandleConsumerSubscribeAsync(subject, null, (config) =>
                new ConsumerMessageAsynHandler(_logger, config, handler, AckAsync), subscribeId: subscribeId);
        }

        #endregion;

        #region 同步处理消息

        public Task<string> ConsumerSubscribeAsync(string subject, string queueGroup, Action<NATSMsgContent> handler, string subscribeId = null)
        {
            return HandleConsumerSubscribeAsync(subject, queueGroup, (config) =>
                new ConsumerMessageSyncHandler(_logger, config, handler, AckAsync), subscribeId: subscribeId);
        }

        public Task<string> ConsumerSubscribeAsync(string subject, Action<NATSMsgContent> handler, string subscribeId = null)
        {
            return HandleConsumerSubscribeAsync(subject, null, (config) =>
                new ConsumerMessageSyncHandler(_logger, config, handler, AckAsync), subscribeId: subscribeId);
        }

        #endregion;


        #region 消息发送确认


        /// <summary>
        /// 发送消息成功处理确认
        /// </summary>
        /// <param name="subscriptionConfig">订阅配置</param>
        /// <param name="msg">消息</param>
        /// <param name="msgAck">是否确认</param>
        private async Task AckAsync(NATSConsumerSubscriptionConfig subscriptionConfig, MessagePacket msg, MessageAck msgAck = MessageAck.Ack)
        {
            AckPacket ackPacket;
            switch (msgAck)
            {
                case MessageAck.Ack:
                    ackPacket = new AckAckPacket();
                    break;
                case MessageAck.Nak:
                    ackPacket = new AckAckPacket();
                    break;
                case MessageAck.Progress:
                    ackPacket = new AckAckPacket();
                    break;
                case MessageAck.Next:
                    ackPacket = new AckAckPacket();
                    break;
                case MessageAck.Term:
                    ackPacket = new AckAckPacket();
                    break;
                default:
                    ackPacket = new AckAckPacket();
                    break;
            }

            await _channel.WriteAndFlushAsync(ackPacket);
        }

        #endregion;


        #endregion;
    }
}
