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
using Hunter.NATS.Client.JetStream;
using DotNetty.Transport.Channels;

namespace Hunter.NATS.Client
{
    public partial class NATSClient
    {
        /// <summary>
        /// 订阅消息处理器集合
        /// </summary>
        private readonly List<ConsumerMessageHandler> _consumerMessageHandler;


        private async Task SubscriptionConsumerMessageAsync()
        {
            foreach (var consumerMessageHandler in _consumerMessageHandler)
            {
                _logger.LogDebug($"[消费者]开始设置主题处理器 Subject = {consumerMessageHandler.SubscriptionConfig.StreamName}");

                await InternalConsumerSubscribeAsync(
                    consumerMessageHandler.SubscriptionConfig.StreamName,
                    consumerMessageHandler.SubscriptionConfig.Config,
                    consumerMessageHandler, 
                    consumerMessageHandler.SubscriptionConfig.SubscribeId);

                _logger.LogDebug($"[消费者]完成设置主题处理器 Subject = {consumerMessageHandler.SubscriptionConfig.StreamName}");
            }
        }


        private async Task SubscribeReplyInboxAsync()
        {
            _logger.LogDebug($"开始设置消息队列收件箱 ReplyInboxId = {_replyInboxId}");

            await _embed_channel.WriteAndFlushAsync(new InboxPacket(DateTime.Now.Ticks.ToString(), _replyInboxId));

            _logger.LogDebug($"结束设置消息队列收件箱 ReplyInboxId = {_replyInboxId}");
        }

        /// <summary>
        /// 异步发送
        /// </summary>
        /// <param name="subject">主体</param>
        /// <param name="data">数据</param>
        /// <param name="headers">消息头部</param>
        /// <returns></returns>
        public Task PublishAsync(string subject, byte[] data, IDictionary<string, string> headers)
        {
            return PublishAsync(subject, data, headers, null);
        }

        /// <summary>
        /// 异步发送
        /// </summary>
        /// <param name="subject">主体</param>
        /// <param name="data">数据</param>
        /// <param name="publishOptions">发布选项</param>
        /// <returns></returns>
        public Task PublishAsync(string subject, byte[] data, PublishOptions publishOptions)
        {
            return PublishAsync(subject, data, null, publishOptions);
        }

        /// <summary>
        /// 异步发送
        /// </summary>
        /// <param name="subject">主体</param>
        /// <param name="data">数据</param>
        /// <param name="headers">消息头部</param>
        /// <param name="publishOptions">发布选项</param>
        /// <returns></returns>
        public async Task PublishAsync(string subject, byte[] data, IDictionary<string, string> headers = null, PublishOptions publishOptions = null)
        {
            if (headers != null || publishOptions != null)
            {
                if (publishOptions != null)
                {
                    var messageHeaders = headers != null ? new Dictionary<string, string>(headers) : new Dictionary<string, string>();

                    if (publishOptions.ExpectedSequence.HasValue)
                    {
                        messageHeaders.Add(NATSJetStreamConstants.EXPECTED_LAST_SEQ_HDR, publishOptions.ExpectedSequence.ToString());
                    }

                    if (!string.IsNullOrEmpty(publishOptions.ExpectedMessageId))
                    {
                        messageHeaders.Add(NATSJetStreamConstants.EXPECTED_LAST_SEQ_HDR, publishOptions.ExpectedMessageId);
                    }

                    if (!string.IsNullOrEmpty(publishOptions.ExpectedStreamName))
                    {
                        messageHeaders.Add(NATSJetStreamConstants.EXPECTED_LAST_SEQ_HDR, publishOptions.ExpectedStreamName);
                    }

                    if (!string.IsNullOrEmpty(publishOptions.MessageId))
                    {
                        messageHeaders.Add(NATSJetStreamConstants.EXPECTED_LAST_SEQ_HDR, publishOptions.MessageId);
                    }

                    await _policy.ExecuteAsync(async (content) =>
                    {
                        await ConnectAsync();

                        if(!_info.JetStream)
                            throw new NATSNotSupportedException("Headers are not supported by the server.");

                        var Packet = new PublishHigherPacket(_replyInboxId, subject, data, messageHeaders);

                        await _embed_channel.WriteAndFlushAsync(Packet);

                    }, new Dictionary<string, object>() { { "hld", "PublishHigherAsync" } });
                }
                else
                {
                    await _policy.ExecuteAsync(async (content) =>
                    {
                        await ConnectAsync();

                        if (!_info.JetStream)
                            throw new NATSNotSupportedException("Headers are not supported by the server.");

                        var Packet = new PublishHigherPacket(_replyInboxId, subject, data, headers);

                        await _embed_channel.WriteAndFlushAsync(Packet);

                    }, new Dictionary<string, object>() { { "hld", "PublishHigherAsync" } });
                }
            }
            else
            {
                await _policy.ExecuteAsync(async (content) =>
                {
                    await ConnectAsync();

                    var Packet = new PublishPacket(_replyInboxId, subject,  data);

                    await _embed_channel.WriteAndFlushAsync(Packet);
                }, new Dictionary<string, object>() { { "hld", "PublishAsync" } });
            }
        }

        #region Stream;

        public async Task<InfoResponse> StreamInfoAsync(string name)
        {
            var jetStreamConfig = JetStreamConfig.Builder().SetName(name).Build();

            var Packet = new DotNetty.Codecs.NATSJetStream.Packets.InfoPacket(
                _replyInboxId,
                jetStreamConfig.Name,
                Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(jetStreamConfig, _jetStreamSetting)));

            return await _policy.ExecuteAsync(async (content) =>
            {
                await ConnectAsync();

                if (!_info.JetStream)
                    throw new NATSNotSupportedException("Headers are not supported by the server.");

                var InfoResponseReady = new TaskCompletionSource<InfoResponsePacket>();

                var Handler = new ReplyPacketHandler<InfoResponsePacket>(Packet.ReplyTo, InfoResponseReady);

                _embed_channel.Pipeline.AddLast(Handler);

                await _embed_channel.WriteAndFlushAsync(Packet);

                var InfoResponse = await InfoResponseReady.Task;

                _embed_channel.Pipeline.Remove(Handler);

                //TODO:待优化
                if (InfoResponse == null) throw new ArgumentNullException();

                return InfoResponse.Message;

            }, new Dictionary<string, object>() { { "hld", "StreamInfoAsync" } });


        }

        public async Task<CreateResponse> StreamCreateAsync(JetStreamConfig jetStreamConfig, Dictionary<string, object> contentData = null)
        {
            var Packet = new CreatePacket(
                _replyInboxId,
                jetStreamConfig.Name,
                Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(jetStreamConfig, _jetStreamSetting)));

            return await _policy.ExecuteAsync(async (content) =>
            {
                await ConnectAsync();

                if (!_info.JetStream)
                    throw new NATSNotSupportedException("Headers are not supported by the server.");

                var CreateResponseReady = new TaskCompletionSource<CreateResponsePacket>();

                var Handler = new ReplyPacketHandler<CreateResponsePacket>(Packet.ReplyTo, CreateResponseReady);

                _embed_channel.Pipeline.AddLast(Handler);

                await _embed_channel.WriteAndFlushAsync(Packet);

                var CreateResponse = await CreateResponseReady.Task;

                _embed_channel.Pipeline.Remove(Handler);

                //TODO:待优化
                if (CreateResponse == null) throw new ArgumentNullException();

                return CreateResponse.Message;

            }, contentData ?? new Dictionary<string, object>() { { "hld", "StreamCreateAsync" } });
        }

        public async Task<InfoResponse> StreamCreateOrGetAsync(JetStreamConfig jetStreamConfig)
        {
            var streamInfo = await StreamInfoAsync(jetStreamConfig.Name);

            if (streamInfo.Error == null) return streamInfo;

            return await StreamCreateAsync(jetStreamConfig, new Dictionary<string, object>() { { "hld", "StreamCreateAsync" } });
        }

        public async Task<UpdateResponse> StreamUpdateAsync(JetStreamConfig jetStreamConfig)
        {
            var Packet = new UpdatePacket(
                _replyInboxId,
                jetStreamConfig.Name,
                Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(jetStreamConfig, _jetStreamSetting)));

            return await _policy.ExecuteAsync(async (content) =>
            {
                await ConnectAsync();

                if (!_info.JetStream)
                    throw new NATSNotSupportedException("Headers are not supported by the server.");

                var UpdateResponseReady = new TaskCompletionSource<UpdateResponsePacket>();

                var Handler = new ReplyPacketHandler<UpdateResponsePacket>(Packet.ReplyTo, UpdateResponseReady);

                _embed_channel.Pipeline.AddLast(Handler);

                await _embed_channel.WriteAndFlushAsync(Packet);

                var UpdateResponseCancellationToken = new CancellationTokenSource(TimeSpan.FromSeconds(15)).Token.Register(() =>
                {
                    UpdateResponseReady.TrySetResult(null);
                });

                var UpdateResponse = await UpdateResponseReady.Task;

                await UpdateResponseCancellationToken.DisposeAsync();

                _embed_channel.Pipeline.Remove(Handler);

                //TODO:待优化
                if (UpdateResponse == null) throw new ArgumentNullException();

                return UpdateResponse.Message;

            }, new Dictionary<string, object>() { { "hld", "StreamUpdateAsync" } });
        }

        public async Task<NamesResponse> StreamNamesAsync()
        {
            var Packet = new NamesPacket(
                _replyInboxId,
                Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(new IterableRequest(), _jetStreamSetting)));

            return await _policy.ExecuteAsync(async (content) =>
            {
                await ConnectAsync();

                if (!_info.JetStream)
                    throw new NATSNotSupportedException("Headers are not supported by the server.");

                var NamesResponseReady = new TaskCompletionSource<NamesResponsePacket>();

                var Handler = new ReplyPacketHandler<NamesResponsePacket>(Packet.ReplyTo, NamesResponseReady);

                _embed_channel.Pipeline.AddLast(Handler);

                await _embed_channel.WriteAndFlushAsync(Packet);

                var NamesResponseCancellationToken = new CancellationTokenSource(TimeSpan.FromSeconds(15)).Token.Register(() =>
                {
                    NamesResponseReady.TrySetResult(null);
                });

                var NamesResponse = await NamesResponseReady.Task;

                await NamesResponseCancellationToken.DisposeAsync();

                _embed_channel.Pipeline.Remove(Handler);

                //TODO:待优化
                if (NamesResponse == null) throw new ArgumentNullException();

                return NamesResponse.Message;

            }, new Dictionary<string, object>() { { "hld", "StreamNamesAsync" } });
        }

        public async Task<ListResponse> StreamListAsync()
        {
            return await _policy.ExecuteAsync(async (content) =>
            {
                await ConnectAsync();

                var Packet = new ListPacket(
                _replyInboxId,
                Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(new IterableRequest(), _jetStreamSetting)));

                var ListResponseReady = new TaskCompletionSource<ListResponsePacket>();

                var Handler = new ReplyPacketHandler<ListResponsePacket>(Packet.ReplyTo, ListResponseReady);

                _embed_channel.Pipeline.AddLast(Handler);

                await _embed_channel.WriteAndFlushAsync(Packet);

                var ListResponseCancellationToken = new CancellationTokenSource(TimeSpan.FromSeconds(15)).Token.Register(() =>
                {
                    ListResponseReady.TrySetResult(null);
                });

                var ListResponse = await ListResponseReady.Task;

                await ListResponseCancellationToken.DisposeAsync();

                _embed_channel.Pipeline.Remove(Handler);

                //TODO:待优化
                if (ListResponse == null) throw new ArgumentNullException();

                return ListResponse.Message;

            }, new Dictionary<string, object>() { { "hld", "StreamListAsync" } });
        }

        public async Task<GetMessageResponse> StreamReadMessageAsync(string name, long sequence)
        {
            var MessageRequest = new GetMessageRequest(sequence);

            var Packet = new GetMessagePacket(
                _replyInboxId,
                name,
                Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(MessageRequest, _jetStreamSetting)));

            return await _policy.ExecuteAsync(async (content) =>
            {
                await ConnectAsync();

                if (!_info.JetStream)
                    throw new NATSNotSupportedException("Headers are not supported by the server.");

                var GetMessageResponseReady = new TaskCompletionSource<GetMessageResponsePacket>();

                var Handler = new ReplyPacketHandler<GetMessageResponsePacket>(Packet.ReplyTo, GetMessageResponseReady);

                _embed_channel.Pipeline.AddLast(Handler);

                await _embed_channel.WriteAndFlushAsync(Packet);

                var GetMessageResponseCancellationToken = new CancellationTokenSource(TimeSpan.FromSeconds(15)).Token.Register(() =>
                {
                    GetMessageResponseReady.TrySetResult(null);
                });

                var GetMessageResponse = await GetMessageResponseReady.Task;

                await GetMessageResponseCancellationToken.DisposeAsync();

                _embed_channel.Pipeline.Remove(Handler);

                //TODO:待优化
                if (GetMessageResponse == null) throw new ArgumentNullException();

                return GetMessageResponse.Message;

            }, new Dictionary<string, object>() { { "hld", "StreamReadMessageAsync" } });
        }

        public async Task<GetMessageResponse> StreamReadMessageAsync(string name, string subject)
        {
            var MessageRequest = new GetMessageRequest(subject);

            var Packet = new GetMessagePacket(
                _replyInboxId,
                name,
                Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(MessageRequest, _jetStreamSetting)));

            return await _policy.ExecuteAsync(async (content) =>
            {
                await ConnectAsync();

                if (!_info.JetStream)
                    throw new NATSNotSupportedException("Headers are not supported by the server.");

                var GetMessageResponseReady = new TaskCompletionSource<GetMessageResponsePacket>();

                var Handler = new ReplyPacketHandler<GetMessageResponsePacket>(Packet.ReplyTo, GetMessageResponseReady);

                _embed_channel.Pipeline.AddLast(Handler);

                await _embed_channel.WriteAndFlushAsync(Packet);

                var GetMessageResponseCancellationToken = new CancellationTokenSource(TimeSpan.FromSeconds(15)).Token.Register(() =>
                {
                    GetMessageResponseReady.TrySetResult(null);
                });

                var GetMessageResponse = await GetMessageResponseReady.Task;

                await GetMessageResponseCancellationToken.DisposeAsync();

                _embed_channel.Pipeline.Remove(Handler);

                //TODO:待优化
                if (GetMessageResponse == null) throw new ArgumentNullException();

                return GetMessageResponse.Message;

            }, new Dictionary<string, object>() { { "hld", "StreamReadMessageAsync" } });
        }

        public async Task<DeleteResponse> StreamDeleteAsync(string name)
        {
            var jetStreamConfig = JetStreamConfig.Builder().SetName(name).Build();

            var Packet = new DeletePacket(
                _replyInboxId,
                jetStreamConfig.Name);

            return await _policy.ExecuteAsync(async (content) =>
            {
                await ConnectAsync();

                if (!_info.JetStream)
                    throw new NATSNotSupportedException("Headers are not supported by the server.");

                var DeleteResponseReady = new TaskCompletionSource<DeleteResponsePacket>();

                var Handler = new ReplyPacketHandler<DeleteResponsePacket>(Packet.ReplyTo, DeleteResponseReady);

                _embed_channel.Pipeline.AddLast(Handler);

                await _embed_channel.WriteAndFlushAsync(Packet);

                var DeleteResponse = await DeleteResponseReady.Task;

                _embed_channel.Pipeline.Remove(Handler);

                //TODO:待优化
                if (DeleteResponse == null) throw new ArgumentNullException();

                return DeleteResponse.Message;

            }, new Dictionary<string, object>() { { "hld", "StreamDeleteAsync" } });


        }

        #endregion;

        #region Consumer;

        public Task<ConsumerCreateResponse> ConsumerCreateAsync(
            string streamName,
            ConsumerConfigBuilder consumerConfigBuilder,
            Func<NATSJetStreamMsgContent, ValueTask> handler)
        {
            var consumer_inbox = Guid.NewGuid().ToString("n");
            consumerConfigBuilder.SetDeliverSubject(consumer_inbox);
            var consumerConfig = consumerConfigBuilder.Build();
            ConsumerSubscribeAsync(streamName, consumerConfig, handler);
            return ConsumerCreateAsync(new ConsumerCreateRequest(streamName, consumerConfig));
        }

        public Task<ConsumerCreateResponse> ConsumerCreateAsync(
            string streamName,
            ConsumerConfigBuilder consumerConfigBuilder,
            Action<NATSJetStreamMsgContent> handler)
        {
            var consumer_inbox = Guid.NewGuid().ToString("n");
            consumerConfigBuilder.SetDeliverSubject(consumer_inbox);
            var consumerConfig = consumerConfigBuilder.Build();
            ConsumerSubscribeAsync(streamName, consumerConfig, handler);
            return ConsumerCreateAsync(new ConsumerCreateRequest(streamName, consumerConfig));
        }

        public Task<ConsumerCreateResponse> ConsumerCreateAsync(
            string streamName,
            ConsumerConfigBuilder consumerConfigBuilder,
            Func<NATSJetStreamMsgContent, ValueTask<MessageAck>> handler)
        {
            var consumer_inbox = Guid.NewGuid().ToString("n");
            consumerConfigBuilder.SetDeliverSubject(consumer_inbox);
            var consumerConfig = consumerConfigBuilder.Build();
            ConsumerSubscribeAsync(streamName, consumerConfig, handler);
            return ConsumerCreateAsync(new ConsumerCreateRequest(streamName, consumerConfig));
        }

        public Task<ConsumerCreateResponse> ConsumerCreateAsync(
            string streamName,
            ConsumerConfigBuilder consumerConfigBuilder,
            Func<NATSJetStreamMsgContent, MessageAck> handler)
        {
            var consumer_inbox = Guid.NewGuid().ToString("n");
            consumerConfigBuilder.SetDeliverSubject(consumer_inbox);
            var consumerConfig = consumerConfigBuilder.Build();
            ConsumerSubscribeAsync(streamName, consumerConfig, handler);
            return ConsumerCreateAsync(new ConsumerCreateRequest(streamName, consumerConfig));
        }


        public async Task<ConsumerCreateResponse> ConsumerCreateAsync(ConsumerCreateRequest createRequest, Dictionary<string, object> contentData = null)
        {
            var Packet = new ConsumerCreatePacket(
                _replyInboxId,
                createRequest.Stream,
                createRequest.Config.DurableName,
                Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(createRequest, _jetStreamSetting)));

            return await _policy.ExecuteAsync(async (content) =>
            {
                await ConnectAsync();

                if (!_info.JetStream)
                    throw new NATSNotSupportedException("Headers are not supported by the server.");

                var ConsumerCreateResponseReady = new TaskCompletionSource<ConsumerCreateResponsePacket>();

                var Handler = new ReplyPacketHandler<ConsumerCreateResponsePacket>(Packet.ReplyTo, ConsumerCreateResponseReady);

                _embed_channel.Pipeline.AddLast(Handler);

                await _embed_channel.WriteAndFlushAsync(Packet);

                var ConsumerCreateResponse = await ConsumerCreateResponseReady.Task;

                _embed_channel.Pipeline.Remove(Handler);

                //TODO:待优化
                if (ConsumerCreateResponse == null) throw new ArgumentNullException();

                return ConsumerCreateResponse.Message;

            }, contentData ?? new Dictionary<string, object>() { { "hld", "ConsumerCreateAsync" } });
        }

        public async Task<ConsumerInfoResponse> ConsumerCreateOrGetAsync(
            string streamName,
            ConsumerConfigBuilder consumerConfigBuilder,
            Func<NATSJetStreamMsgContent, ValueTask> handler)
        {
            var consumerInfo = await ConsumerInfoAsync(streamName, consumerConfigBuilder.GetDurable(),
                new Dictionary<string, object>() { { "hld", "ConsumerCreateOrGetAsync => ConsumerInfoAsync" } });
            if (consumerInfo.Error == null)
            {
                await ConsumerSubscribeAsync(streamName, consumerInfo.Config, handler);
                return consumerInfo;
            }

            var consumer_inbox = Guid.NewGuid().ToString("n");
            consumerConfigBuilder.SetDeliverSubject(consumer_inbox);
            var consumerConfig = consumerConfigBuilder.Build();

            await ConsumerSubscribeAsync(streamName, consumerConfig, handler);
            return await ConsumerCreateAsync(new ConsumerCreateRequest(streamName, consumerConfig),
                new Dictionary<string, object>() { { "hld", "ConsumerCreateOrGetAsync" } });
        }

        public async Task<ConsumerInfoResponse> ConsumerCreateOrGetAsync(
            string streamName,
            ConsumerConfigBuilder consumerConfigBuilder,
            Action<NATSJetStreamMsgContent> handler)
        {
            var consumerInfo = await ConsumerInfoAsync(streamName, consumerConfigBuilder.GetDurable(),
                new Dictionary<string, object>() { { "hld", "ConsumerCreateOrGetAsync => ConsumerInfoAsync" } });
            if (consumerInfo.Error == null)
            {
                await ConsumerSubscribeAsync(streamName, consumerInfo.Config, handler);
                return consumerInfo;
            }

            var consumer_inbox = Guid.NewGuid().ToString("n");
            consumerConfigBuilder.SetDeliverSubject(consumer_inbox);
            var consumerConfig = consumerConfigBuilder.Build();

            await ConsumerSubscribeAsync(streamName, consumerConfig, handler);
            return await ConsumerCreateAsync(new ConsumerCreateRequest(streamName, consumerConfig),
                new Dictionary<string, object>() { { "hld", "ConsumerCreateOrGetAsync" } });
        }

        public async Task<ConsumerInfoResponse> ConsumerCreateOrGetAsync(
            string streamName,
            ConsumerConfigBuilder consumerConfigBuilder,
            Func<NATSJetStreamMsgContent, ValueTask<MessageAck>> handler)
        {
            var consumerInfo = await ConsumerInfoAsync(streamName, consumerConfigBuilder.GetDurable(),
                new Dictionary<string, object>() { { "hld", "ConsumerCreateOrGetAsync => ConsumerInfoAsync" } });
            if (consumerInfo.Error == null)
            {
                await ConsumerSubscribeAsync(streamName, consumerInfo.Config, handler);
                return consumerInfo;
            }

            var consumer_inbox = Guid.NewGuid().ToString("n");
            consumerConfigBuilder.SetDeliverSubject(consumer_inbox);
            var consumerConfig = consumerConfigBuilder.Build();

            await ConsumerSubscribeAsync(streamName, consumerConfig, handler);
            return await ConsumerCreateAsync(new ConsumerCreateRequest(streamName, consumerConfig),
                new Dictionary<string, object>() { { "hld", "ConsumerCreateOrGetAsync" } });
        }


        public async Task<ConsumerInfoResponse> ConsumerCreateOrGetAsync(
            string streamName,
            ConsumerConfigBuilder consumerConfigBuilder,
            Func<NATSJetStreamMsgContent, MessageAck> handler)
        {
            var consumerInfo = await ConsumerInfoAsync(streamName, consumerConfigBuilder.GetDurable(),
                new Dictionary<string, object>() { { "hld", "ConsumerCreateOrGetAsync => ConsumerInfoAsync" } });
            if (consumerInfo.Error == null)
            {
                await ConsumerSubscribeAsync(streamName, consumerInfo.Config, handler);
                return consumerInfo;
            }

            var consumer_inbox = Guid.NewGuid().ToString("n");
            consumerConfigBuilder.SetDeliverSubject(consumer_inbox);
            var consumerConfig = consumerConfigBuilder.Build();

            await ConsumerSubscribeAsync(streamName, consumerConfig, handler);
            return await ConsumerCreateAsync(new ConsumerCreateRequest(streamName, consumerConfig),
                new Dictionary<string, object>() { { "hld", "ConsumerCreateOrGetAsync" } });
        }

        public async Task<ConsumerInfoResponse> ConsumerInfoAsync(string streamName, string consumerName,
            Dictionary<string, object> contentData = null)
        {
            var Packet = new ConsumerInfoPacket(
                _replyInboxId,
                streamName,
                consumerName);

            return await _policy.ExecuteAsync(async (content) =>
            {
                await ConnectAsync();

                if (!_info.JetStream)
                    throw new NATSNotSupportedException("Headers are not supported by the server.");

                var ConsumerInfoResponseReady = new TaskCompletionSource<ConsumerInfoResponsePacket>();

                var Handler = new ReplyPacketHandler<ConsumerInfoResponsePacket>(Packet.ReplyTo, ConsumerInfoResponseReady);

                _embed_channel.Pipeline.AddLast(Handler);

                await _embed_channel.WriteAndFlushAsync(Packet);

                var ConsumerInfoResponse = await ConsumerInfoResponseReady.Task;

                _embed_channel.Pipeline.Remove(Handler);

                //TODO:待优化
                if (ConsumerInfoResponse == null) throw new ArgumentNullException();

                return ConsumerInfoResponse.Message;

            }, contentData ?? new Dictionary<string, object>() { { "hld", "ConsumerInfoAsync" } });
        }


        public async Task<ConsumerNamesResponse> ConsumerNamesAsync(string consumerName)
        {
            var Packet = new ConsumerNamesPacket(
                _replyInboxId,
                consumerName);

            return await _policy.ExecuteAsync(async (content) =>
            {
                await ConnectAsync();

                if (!_info.JetStream)
                    throw new NATSNotSupportedException("Headers are not supported by the server.");

                var ConsumerNamesResponseReady = new TaskCompletionSource<ConsumerNamesResponsePacket>();

                var Handler = new ReplyPacketHandler<ConsumerNamesResponsePacket>(Packet.ReplyTo, ConsumerNamesResponseReady);

                _embed_channel.Pipeline.AddLast(Handler);

                await _embed_channel.WriteAndFlushAsync(Packet);

                var ConsumerNamesResponse = await ConsumerNamesResponseReady.Task;

                _embed_channel.Pipeline.Remove(Handler);

                //TODO:待优化
                if (ConsumerNamesResponse == null) throw new ArgumentNullException();

                return ConsumerNamesResponse.Message;

            }, new Dictionary<string, object>() { { "hld", "ConsumerNamesAsync" } });
        }

        public async Task<ConsumerListResponse> ConsumerListAsync(string consumerName)
        {
            var Packet = new ConsumerListPacket(
                _replyInboxId,
                consumerName);

            return await _policy.ExecuteAsync(async (content) =>
            {
                await ConnectAsync();

                if (!_info.JetStream)
                    throw new NATSNotSupportedException("Headers are not supported by the server.");

                var ConsumerListResponseReady = new TaskCompletionSource<ConsumerListResponsePacket>();

                var Handler = new ReplyPacketHandler<ConsumerListResponsePacket>(Packet.ReplyTo, ConsumerListResponseReady);

                _embed_channel.Pipeline.AddLast(Handler);

                await _embed_channel.WriteAndFlushAsync(Packet);

                var ConsumerListResponse = await ConsumerListResponseReady.Task;

                _embed_channel.Pipeline.Remove(Handler);

                //TODO:待优化
                if (ConsumerListResponse == null) throw new ArgumentNullException();

                return ConsumerListResponse.Message;

            }, new Dictionary<string, object>() { { "hld", "ConsumerListAsync" } });
        }

        public async Task<ConsumerDeleteResponse> ConsumerDeleteAsync(string streamName, string consumerName)
        {
            var Packet = new ConsumerDeletePacket(
                _replyInboxId,
                streamName,
                consumerName);

            return await _policy.ExecuteAsync(async (content) =>
            {
                await ConnectAsync();

                if (!_info.JetStream)
                    throw new NATSNotSupportedException("Headers are not supported by the server.");

                var ConsumerDeleteResponseReady = new TaskCompletionSource<ConsumerDeleteResponsePacket>();

                var Handler = new ReplyPacketHandler<ConsumerDeleteResponsePacket>(Packet.ReplyTo, ConsumerDeleteResponseReady);

                _embed_channel.Pipeline.AddLast(Handler);

                await _embed_channel.WriteAndFlushAsync(Packet);

                var ConsumerDeleteResponse = await ConsumerDeleteResponseReady.Task;

                _embed_channel.Pipeline.Remove(Handler);

                //TODO:待优化
                if (ConsumerDeleteResponse == null) throw new ArgumentNullException();

                return ConsumerDeleteResponse.Message;

            }, new Dictionary<string, object>() { { "hld", "ConsumerDeleteAsync" } });
        }

        #endregion;

        #region ConsumerSubscribe;

        public async Task<string> HandleConsumerSubscribeAsync(string streamName, ConsumerConfig config,
            Func<NATSConsumerSubscriptionConfig, ConsumerMessageHandler> messageHandlerSetup, string subscribeId = null)
        {
            return await _policy.ExecuteAsync(async (content) =>
            {
                return await InternalConsumerSubscribeAsync(streamName, config, messageHandlerSetup, subscribeId);

            }, new Dictionary<string, object>() { { "hld", "HandleConsumerSubscribeAsync" } });
        }



        public async Task<string> InternalConsumerSubscribeAsync(string streamName, ConsumerConfig config,
            Func<NATSConsumerSubscriptionConfig, ConsumerMessageHandler> messageHandlerSetup, string subscribeId = null)
        {
            await ConnectAsync();

            var SubscribeId = subscribeId ?? $"sid{Interlocked.Increment(ref _subscribeId)}";

            _logger.LogDebug($"[消费者]设置订阅消息队列订阅编号 Subject = {streamName} SubscribeId = {SubscribeId}");

            var SubscriptionConfig = new NATSConsumerSubscriptionConfig(streamName, config, SubscribeId);

            //处理订阅响应的管道
            var messageHandler = messageHandlerSetup(SubscriptionConfig);

            _logger.LogDebug($"[消费者]开始添加消息队列处理器 Subject = {streamName}  SubscribeId = {SubscribeId}");

            //添加订阅响应管道
            _embed_channel.Pipeline.AddLast(messageHandler);

            _logger.LogDebug($"[消费者]结束添加消息队列处理器 Subject = {streamName}  SubscribeId = {SubscribeId}");


            _logger.LogDebug($"[消费者]开始发送订阅请求 订阅主题 {streamName } 订阅编号 {SubscribeId}");

            var SubscribePacketMsg = new SubscribePacket(SubscribeId, config.DeliverSubject);

            await _embed_channel.WriteAndFlushAsync(SubscribePacketMsg);

            _logger.LogDebug($"[消费者]结束发送订阅请求 订阅主题 {streamName } 订阅编号 {SubscribeId}");

            //添加消息处理到消息处理集合
            _consumerMessageHandler.Add(messageHandler);

            return SubscribeId;
        }


        public async Task<string> InternalConsumerSubscribeAsync(string streamName, ConsumerConfig config,
            ConsumerMessageHandler messageHandler, string subscribeId = null)
        {
            await ConnectAsync();

            var SubscribeId = subscribeId ?? $"sid{Interlocked.Increment(ref _subscribeId)}";

            _logger.LogDebug($"[消费者]开始添加消息队列处理器 Subject = {streamName} QueueGroup = {config.DurableName} SubscribeId = {SubscribeId}");

            //添加订阅响应管道
            _embed_channel.Pipeline.AddLast(messageHandler);

            _logger.LogDebug($"[消费者]结束添加消息队列处理器 Subject = {streamName} QueueGroup = {config.DurableName} SubscribeId = {SubscribeId}");


            _logger.LogDebug($"[消费者]开始发送订阅请求 订阅主题 {streamName } 订阅编号 {SubscribeId}");

            var SubscribePacketMsg = new SubscribePacket(SubscribeId, config.DeliverSubject);

            await _embed_channel.WriteAndFlushAsync(SubscribePacketMsg);

            _logger.LogDebug($"[消费者]结束发送订阅请求 订阅主题 {streamName } 订阅编号 {SubscribeId}");

            return SubscribeId;
        }


        #region 订阅自动确认 异步处理消息


        public Task<string> ConsumerSubscribeAsync(string streamName, ConsumerConfig config, Func<NATSJetStreamMsgContent, ValueTask> handler, string subscribeId = null)
        {
            return HandleConsumerSubscribeAsync(streamName, config, (config) =>
                new ConsumerMessageAsynHandler(_logger, config, handler, AckAsync), subscribeId: subscribeId);
        }

        #endregion;

        #region 订阅自动确认 同步处理消息

        public Task<string> ConsumerSubscribeAsync(string streamName, ConsumerConfig config, Action<NATSJetStreamMsgContent> handler, string subscribeId = null)
        {
            return HandleConsumerSubscribeAsync(streamName, config, (config) =>
                new ConsumerMessageSyncHandler(_logger, config, handler, AckAsync), subscribeId: subscribeId);
        }

        #endregion;

        #region 订阅手动确认 异步处理消息

        public Task<string> ConsumerSubscribeAsync(string streamName, ConsumerConfig config, Func<NATSJetStreamMsgContent, ValueTask<MessageAck>> handler, string subscribeId = null)
        {
            return HandleConsumerSubscribeAsync(streamName, config, (config) =>
                new ConsumerMessageAckAsynHandler(_logger, config, handler, AckAsync), subscribeId: subscribeId);
        }

        #endregion;

        #region 订阅手动确认 同步处理消息


        public Task<string> ConsumerSubscribeAsync(string streamName, ConsumerConfig config, Func<NATSJetStreamMsgContent, MessageAck> handler, string subscribeId = null)
        {
            return HandleConsumerSubscribeAsync(streamName, config, (config) =>
                new ConsumerMessageAckSyncHandler(_logger, config, handler, AckAsync), subscribeId: subscribeId);
        }

        #endregion;


        #region 消息发送确认


        /// <summary>
        /// 发送消息成功处理确认
        /// </summary>
        /// <param name="subscriptionConfig">订阅配置</param>
        /// <param name="msg">消息</param>
        /// <param name="msgAck">是否确认</param>
        private async ValueTask AckAsync(NATSConsumerSubscriptionConfig subscriptionConfig, MessagePacket msg, MessageAck msgAck = MessageAck.Ack)
        {
            AckPacket ackPacket;
            switch (msgAck)
            {
                case MessageAck.Ack:
                    ackPacket = new AckAckPacket(msg.ReplyTo);
                    break;
                case MessageAck.Nak:
                    ackPacket = new AckNakPacket(msg.ReplyTo);
                    break;
                case MessageAck.Progress:
                    ackPacket = new AckProgressPacket(msg.ReplyTo);
                    break;
                case MessageAck.Next:
                    ackPacket = new AckNextPacket(msg.ReplyTo);
                    break;
                case MessageAck.Term:
                    ackPacket = new AckTermPacket(msg.ReplyTo);
                    break;
                default:
                    ackPacket = new AckAckPacket(msg.ReplyTo);
                    break;
            }

            await _policy.ExecuteAsync(async (content) =>
            {

                await ConnectAsync();

                await _embed_channel.WriteAndFlushAsync(ackPacket);

            }, new Dictionary<string, object>() { { "hld", "AckAsync" } });
        }

        #endregion;


        #endregion;

        #region Consumer Pull;

        public Task<ConsumerCreateResponse> ConsumerCreateAsync(
            string streamName,
            ConsumerConfigBuilder consumerConfigBuilder)
        {
            var consumer_inbox = Guid.NewGuid().ToString("n");
            consumerConfigBuilder.SetDeliverSubject(consumer_inbox);
            //ConsumerSubscribeAsync(consumer_inbox, handler);
            return ConsumerCreateAsync(new ConsumerCreateRequest(streamName, consumerConfigBuilder.Build()));
        }

        #endregion;
    }
}
