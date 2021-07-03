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
    public class ConsumerNextMessagePacket : DirectivePacket
    {
        public ConsumerNextMessagePacket(string inboxId, string subject, string consumerName)
        {
            Subject = $"{ProtocolSignatures.JSAPI_CONSUMER_MSG_NEXT}.{subject}.{consumerName}";
            ReplyTo = $"{NATSJetStreamInboxs.ConsumerMessageNextResponse}{inboxId}.{Guid.NewGuid():N}";
        }

        public override NATSPacketType PacketType => NATSPacketType.CONSUMER_MSG_NEXT;
    }
}
