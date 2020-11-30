using DotNetty.Codecs.STAN.Protocol;
using Google.Protobuf;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace DotNetty.Codecs.STAN.Packets
{
    [DataContract]
    public class ConnectPingResponsePacket : MessagePacket<PingResponse>
    {
        [IgnoreDataMember]
        public override STANPacketType PacketType => STANPacketType.ConnectPingResponse;
    }
}
