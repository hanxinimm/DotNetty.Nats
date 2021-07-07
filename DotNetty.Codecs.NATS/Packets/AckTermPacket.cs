using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace DotNetty.Codecs.NATS.Packets
{
    [DataContract]
    public class AckTermPacket : AckPacket
    {
        public AckTermPacket() { }

        public AckTermPacket(string subject)
        {
            Subject = subject;
        }

        [IgnoreDataMember]
        public override NATSPacketType PacketType => NATSPacketType.ACK_TERM;
    }
}
