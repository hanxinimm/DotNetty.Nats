using System;
using System.Collections.Generic;
using System.Text;

namespace DotNetty.Codecs.STAN.Packets
{
    public class HeartbeatAckPacket : STANSubscribePacket
    {
        public HeartbeatAckPacket(string subject)
        {
            Id = "Id" + Guid.NewGuid().ToString("N");
            Subject = subject;
        }

        public override STANPacketType PacketType => STANPacketType.HeartbeatAck;
    }
}
