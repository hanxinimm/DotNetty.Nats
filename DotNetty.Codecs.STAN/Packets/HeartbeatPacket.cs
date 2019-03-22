using System;
using System.Collections.Generic;
using System.Text;

namespace DotNetty.Codecs.STAN.Packets
{
    public class HeartbeatPacket : MessagePacket
    {
        public override STANPacketType PacketType => STANPacketType.Heartbeat;
    }
}
