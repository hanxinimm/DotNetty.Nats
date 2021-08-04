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
    public class UpdatePacket : PublishPacket
    {
        public UpdatePacket(string inboxId, string subject, byte[] payload)
        {
            Subject = $"{NATSJetStreamSignatures.JSAPI_STREAM_UPDATE}.{subject}";
            ReplyTo = $"{NATSJetStreamInboxs.UpdateResponse}{inboxId}.{Guid.NewGuid():N}";
            Payload = payload;
        }

        public override NATSPacketType PacketType => NATSPacketType.STREAM_UPDATE;
    }
}
