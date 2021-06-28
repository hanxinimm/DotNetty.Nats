using DotNetty.Codecs.NATS.Packets;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace DotNetty.Codecs.NATSJetStream.Packets
{
    [DataContract]
    public class InboxPacket : SubscribePacket
    {
        [IgnoreDataMember]
        public override NATSPacketType PacketType => NATSPacketType.INBOX;

        public InboxPacket(string id, string subject) : base(id, subject)
        {
            Id = id;
            Subject = $"{NATSJetStreamInboxs.InboxPrefix}*.{subject}.*";
        }
    }
}
