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

        public SubscribePacket(string id, string subject,string group)
        {
            Id = id;
            Subject = subject;
            Group = group;
        }

        public SubscribePacket(string id, string subject)
        {
            Id = id;
            Subject = subject;
        }
    }
}
