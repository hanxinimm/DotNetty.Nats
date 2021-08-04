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
    public class ConsumerListPacket : DirectivePacket
    {
        public ConsumerListPacket(string inboxId, string subject)
        {
            Subject = $"{NATSJetStreamSignatures.JSAPI_CONSUMER_LIST}.{subject}";
            ReplyTo = $"{NATSJetStreamInboxs.ConsumerListResponse}{inboxId}.{Guid.NewGuid():N}";
        }

        public override NATSPacketType PacketType => NATSPacketType.CONSUMER_LIST;
    }
}
