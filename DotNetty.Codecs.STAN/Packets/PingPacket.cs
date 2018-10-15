using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace DotNetty.Codecs.STAN.Packets
{
    [DataContract]
    public class PingPacket : STANPacket
    {
        [IgnoreDataMember]
        public override STANPacketType PacketType => STANPacketType.PING;
    }
}
