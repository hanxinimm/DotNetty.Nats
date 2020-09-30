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

        public SubscribePacket(string id, string group)
        {
            Id = id;
            Subject = $"{STANInboxs.MsgProto}{Guid.NewGuid():N}";
            Group = group;
        }

        public SubscribePacket(string id)
        {
            Id = id;
            Subject = $"{STANInboxs.MsgProto}{Guid.NewGuid():N}";
        }
    }
}
