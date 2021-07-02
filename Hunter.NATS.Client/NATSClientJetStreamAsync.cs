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

namespace Hunter.NATS.Client
{
    public partial class NATSClient
    {
        public async Task<InfoResponse> StreamInfoAsync(string name)
        {
            var jetStreamConfig = JetStreamConfig.Builder().SetName(name).Build();

            var Packet = new InfoPacket(
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


        public Task<ConsumerCreateResponse> ConsumerCreateAsync(string streamName, ConsumerConfig consumerConfig)
        {
            return ConsumerCreateAsync(new ConsumerCreateRequest(streamName, consumerConfig));
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
    }
}
