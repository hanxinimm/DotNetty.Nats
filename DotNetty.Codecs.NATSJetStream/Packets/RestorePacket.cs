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
    public class RestorePacket : PublishPacket
    {
        public RestorePacket(string inboxId, string subject, byte[] payload)
        {
            Subject = $"{NATSJetStreamSignatures.JSAPI_STREAM_RESTORE}.{subject}";
            ReplyTo = $"{NATSJetStreamInboxs.RestoreResponse}{inboxId}.{Guid.NewGuid():N}";
            Payload = payload;
        }

        public override NATSPacketType PacketType => NATSPacketType.STREAM_RESTORE;
    }
}
