using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace DotNetty.Codecs.STAN.Packets
{
    [DataContract]
    public class InboxPacket : STANSubscribePacket
    {
        [IgnoreDataMember]
        public override STANPacketType PacketType => STANPacketType.INBOX;

        public InboxPacket(string id, string subject)
        {
            Id = id;
            Subject = $"{STANInboxs.InboxPrefix}*.{subject}.*";
        }
    }
}
