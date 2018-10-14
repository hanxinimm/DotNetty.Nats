using System;
using System.Collections.Generic;
using System.Text;

namespace DotNetty.Codecs.STAN.Packets
{
    public class HeartbeatInboxPacket : STANSubscribePacket
    {
        public HeartbeatInboxPacket()
        {
            Id = "Id" + Guid.NewGuid().ToString("N");
            Subject = "_INBOX." + Guid.NewGuid().ToString("N");
        }

        public override STANPacketType PacketType => STANPacketType.Heartbeat;
    }
}
