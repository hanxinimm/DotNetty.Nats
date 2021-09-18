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
    public class DeletePacket : PublishPacket
    {
        public DeletePacket(string inboxId, string subject)
        {
            Subject = $"{NATSJetStreamSignatures.JSAPI_STREAM_DELETE}.{subject}";
            ReplyTo = $"{NATSJetStreamInboxs.DeleteResponse}{inboxId}.{Guid.NewGuid():N}";
        }

        public override NATSPacketType PacketType => NATSPacketType.STREAM_DELETE;
    }
}
