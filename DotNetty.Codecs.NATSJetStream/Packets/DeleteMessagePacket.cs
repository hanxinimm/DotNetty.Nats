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
    public class DeleteMessagePacket : PublishPacket
    {
        public DeleteMessagePacket(string inboxId, byte[] payload)
        {
            Subject = $"{ProtocolSignatures.JSAPI_STREAM_DELETE}";
            ReplyTo = $"{NATSJetStreamInboxs.DeleteMessageResponse}{inboxId}.{Guid.NewGuid():N}";
            Payload = payload;
        }

        public override NATSPacketType PacketType => NATSPacketType.STREAM_MSG_DELETE;
    }
}
