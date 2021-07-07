using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace DotNetty.Codecs.NATS.Packets
{
    [DataContract]
    public class AckNakPacket : AckPacket
    {
        public AckNakPacket() { }

        public AckNakPacket(string subject)
        {
            Subject = subject;
        }

        [IgnoreDataMember]
        public override NATSPacketType PacketType => NATSPacketType.ACK_NAK;
    }
}
