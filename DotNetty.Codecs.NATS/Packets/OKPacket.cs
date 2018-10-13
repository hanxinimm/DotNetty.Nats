using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace DotNetty.Codecs.NATS.Packets
{
    [DataContract]
    public class OKPacket : Packet
    {
        [IgnoreDataMember]
        public override PacketType PacketType => PacketType.PLUS_OK;
    }
}
