using DotNetty.Codecs.STAN.Protocol;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;

namespace DotNetty.Codecs.STAN.Packets
{
    [DataContract]
    public class SubscriptionResponsePacket : MessagePacket<SubscriptionResponse>
    {
        public override STANPacketType PacketType => STANPacketType.SubscriptionResponse;

    }
}
