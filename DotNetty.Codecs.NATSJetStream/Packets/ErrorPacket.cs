using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace DotNetty.Codecs.NATSJetStream.Packets
{
    [DataContract]
    public class ErrorPacket : NATSJetStreamPacket
    {
        [IgnoreDataMember]
        public override NATSJetStreamPacketType PacketType => NATSJetStreamPacketType.MINUS_ERR;
    }
}
