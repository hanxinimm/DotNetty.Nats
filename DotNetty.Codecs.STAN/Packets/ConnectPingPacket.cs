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
            string subRequests,
            string connectID)
        {
            Subject = subRequests;
            ReplyTo = $"{STANInboxs.PingResponse}{inboxId}.{Guid.NewGuid():N}";
            Message = new Ping()
            {
                ConnID = ByteString.CopyFrom(Encoding.UTF8.GetBytes(connectID)),
            };
        }

        [IgnoreDataMember]
        public override STANPacketType PacketType => STANPacketType.ConnectPing;
    }
}
