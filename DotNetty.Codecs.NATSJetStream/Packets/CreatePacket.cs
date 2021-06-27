using DotNetty.Buffers;
using DotNetty.Codecs.NATS.Packets;
using DotNetty.Codecs.NATSJetStream.Protocol;
using DotNetty.Common;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace DotNetty.Codecs.NATSJetStream.Packets
{
    [DataContract]
    public class CreatePacket : MessagePacket<JetStreamConfig>
    {
        public CreatePacket(string inboxId, string subject, JetStreamConfig jetStreamConfig)
        {
            Subject = $"{ProtocolSignatures.JSAPI_STREAM_CREATE}.{jetStreamConfig.Name}";
            ReplyTo = $"{NATSJetStreamInboxs.CreateResponse}{inboxId}.{Guid.NewGuid():N}";
            Message = jetStreamConfig;
        }

        public override NATSPacketType PacketType => NATSPacketType.CREATE;
    }
}
