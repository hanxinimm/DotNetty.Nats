using System;
using System.Collections.Generic;
using System.Text;

namespace DotNetty.Codecs.STAN.Packets
{
    public class HeartbeatInboxPacket : STANSubscribePacket
    {
        public HeartbeatInboxPacket()
        {
            Subject = "_INBOX." + Guid.NewGuid().ToString("N");
        }

        public override STANPacketType PacketType => STANPacketType.Heartbeat;
    }
}
