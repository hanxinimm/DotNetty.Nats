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
    public class ListResponsePacket : MessagePacket<ListResponse>
    {
        public override NATSPacketType PacketType => NATSPacketType.STREAM_LIST_RESPONSE;

    }
}
