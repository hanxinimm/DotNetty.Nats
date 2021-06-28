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
        public async Task<CreateResponse> StreamInfoAsync(string name)
        {
            var ConnectId = Guid.NewGuid().ToString("N");

            var jetStreamConfig = JetStreamConfig.Builder().SetName(name).Build();

            var Packet = new InfoPacket(
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

        public async Task<CreateResponse> StreamCreateAsync(JetStreamConfig jetStreamConfig)
        {
            var ConnectId = Guid.NewGuid().ToString("N");

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
    }
}
