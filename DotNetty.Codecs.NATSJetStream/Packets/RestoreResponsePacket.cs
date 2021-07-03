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
    public class RestoreResponsePacket : MessagePacket<RestoreResponse>
    {
        public override NATSPacketType PacketType => NATSPacketType.STREAM_RESTORE_RESPONSE;

    }
}
