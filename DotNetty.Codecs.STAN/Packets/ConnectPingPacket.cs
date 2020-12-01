using DotNetty.Codecs.STAN.Protocol;
using Google.Protobuf;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace DotNetty.Codecs.STAN.Packets
{
    [DataContract]
    public class ConnectPingPacket : MessagePacket<Ping>
    {
        public ConnectPingPacket(
            string inboxId,
            string pingRequests,
            ByteString connectID)
        {
            Subject = pingRequests;
            ReplyTo = $"{STANInboxs.PingResponse}{inboxId}.{Guid.NewGuid():N}";
            Message = new Ping()
            {
                ConnID = connectID,
            };
        }

        [IgnoreDataMember]
        public override STANPacketType PacketType => STANPacketType.ConnectPing;
    }
}
