using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace DotNetty.Codecs.STAN.Packets
{
    [DataContract]
    public class SubscribePacket : STANSubscribePacket
    {
        [IgnoreDataMember]
        public override STANPacketType PacketType => STANPacketType.SUB;

        public SubscribePacket(string subject)
        {
            Id = DateTime.Now.Ticks.ToString();
            Subject = subject;
        }

        public SubscribePacket()
        {
            Id = DateTime.Now.Ticks.ToString();
            Subject = $"{STANInboxs.MsgProto}{Guid.NewGuid():N}";
        }
    }
}
