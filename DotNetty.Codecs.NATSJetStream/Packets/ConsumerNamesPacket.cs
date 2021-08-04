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
    public class ConsumerNamesPacket : DirectivePacket
    {
        public ConsumerNamesPacket(string inboxId, string subject)
        {
            Subject = $"{NATSJetStreamSignatures.JSAPI_CONSUMER_NAMES}.{subject}";
            ReplyTo = $"{NATSJetStreamInboxs.ConsumerNamesResponse}{inboxId}.{Guid.NewGuid():N}";
        }

        public override NATSPacketType PacketType => NATSPacketType.CONSUMER_NAMES;
    }
}
