﻿using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace DotNetty.Codecs.NATS.Packets
{
    [DataContract]
    public class AckProgressPacket : AckPacket
    {
        [IgnoreDataMember]
        public override NATSPacketType PacketType => NATSPacketType.ACK_PROGRESS;
    }
}
