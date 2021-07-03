using DotNetty.Codecs.NATS.Packets;
using DotNetty.Codecs.NATSJetStream.Protocol;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;

namespace DotNetty.Codecs.NATSJetStream.Packets
{
    [DataContract]
    public class ConsumerNextMessageResponsePacket : MessagePacket
    {
        public override NATSPacketType PacketType => NATSPacketType.CONSUMER_MSG_NEXT_RESPONSE;

    }
}
