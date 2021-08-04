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
    public class InfoPacket : PublishPacket
    {
        public InfoPacket(string inboxId, string subject, byte[] payload)
        {
            Subject = $"{NATSJetStreamSignatures.JSAPI_STREAM_INFO}.{subject}";
            ReplyTo = $"{NATSJetStreamInboxs.InfoResponse}{inboxId}.{Guid.NewGuid():N}";
            Payload = payload;
        }

        public override NATSPacketType PacketType => NATSPacketType.STREAM_INFO;
    }
}
