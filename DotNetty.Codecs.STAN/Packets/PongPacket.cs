﻿using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace DotNetty.Codecs.STAN.Packets
{
    [DataContract]
    public class PongPacket : Packet
    {
        [IgnoreDataMember]
        public override PacketType PacketType => PacketType.PONG;
    }
}
