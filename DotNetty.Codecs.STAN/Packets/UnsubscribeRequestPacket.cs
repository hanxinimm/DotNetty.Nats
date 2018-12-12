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
    public class UnsubscribeRequestPacket : MessagePacket<UnsubscribeRequest>
    {
        public override STANPacketType PacketType => STANPacketType.UnsubscribeRequest;

        public UnsubscribeRequestPacket(string inboxId, string unsubRequests, string clientID, string subject, string inbox, string durableName)
        {
            Subject = unsubRequests;
            ReplyTo = $"{STANInboxs.UnSubscriptionResponse}{inboxId}.{Guid.NewGuid().ToString("N")}";
            Message = new UnsubscribeRequest
            {
                ClientID = clientID,
                Subject = subject,
                Inbox = inbox,
                DurableName = durableName ?? string.Empty
            };
        }
    }
}
