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
    public class GetMessagePacket : PublishPacket
    {
        public GetMessagePacket(string inboxId, string subject, byte[] payload)
        {
            Subject = $"{ProtocolSignatures.JSAPI_MSG_GET}.{subject}";
            ReplyTo = $"{NATSJetStreamInboxs.GetMessageResponse}{inboxId}.{Guid.NewGuid():N}";
            Payload = payload;
        }

        public override NATSPacketType PacketType => NATSPacketType.STREAM_MSG_GET;
    }
}
