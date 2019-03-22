using System;
using System.Collections.Generic;
using System.Text;

namespace DotNetty.Codecs.STAN.Packets
{
    public class HeartbeatInboxPacket : STANSubscribePacket
    {
        public HeartbeatInboxPacket(string subject)
        {
            Id = "Id" + Guid.NewGuid().ToString("N");
            Subject = $"{STANInboxs.Heartbeat}{subject}";
        }

        public override STANPacketType PacketType => STANPacketType.HeartbeatInbox;
    }
}
