using DotNetty.Codecs.STAN.Protocol;
using System;
using System.Collections.Generic;
using System.Text;

namespace DotNetty.Codecs.STAN.Packets
{
    public class SubscriptionRequestPacket : MessagePacket<SubscriptionRequest>
    {
        public SubscriptionRequestPacket(string inboxId, string subRequests, string clientID, string subject, string queueGroup, string inbox, int maxInFlight,
            int ackWaitInSeconds, string durableName, StartPosition startPosition)
        {
            Subject = subRequests;
            ReplyTo = $"{STANInboxs.SubscriptionResponse}{inboxId}.{Guid.NewGuid().ToString("N")}";
            Message = new SubscriptionRequest
            {
                ClientID = clientID,
                Subject = subject,
                QGroup = queueGroup ?? string.Empty,
                Inbox = inbox,
                MaxInFlight = maxInFlight,
                AckWaitInSecs = ackWaitInSeconds,
                StartPosition = startPosition,
                DurableName = durableName ?? string.Empty
            };
        }

        public SubscriptionRequestPacket(string inboxId, string subRequests, string clientID, string subject, string queueGroup, string inbox, int maxInFlight,
            int ackWaitInSeconds, string durableName, ulong startSequence)
        {
            Subject = subRequests;
            ReplyTo = $"{STANInboxs.SubscriptionResponse}{inboxId}.{Guid.NewGuid().ToString("N")}";
            Message = new SubscriptionRequest
            {
                ClientID = clientID,
                Subject = subject,
                QGroup = queueGroup ?? string.Empty,
                Inbox = inbox,
                MaxInFlight = maxInFlight,
                AckWaitInSecs = ackWaitInSeconds,
                StartPosition = StartPosition.SequenceStart,
                DurableName = durableName ?? string.Empty,

                // Conditionals
                StartSequence = startSequence
            };
        }

        public SubscriptionRequestPacket(string inboxId, string subRequests, string clientID, string subject, string queueGroup, string inbox, int maxInFlight,
            int ackWaitInSeconds, string durableName, long startTimeDelta)
        {
            Subject = subRequests;
            ReplyTo = $"{STANInboxs.SubscriptionResponse}{inboxId}.{Guid.NewGuid().ToString("N")}";
            Message = new SubscriptionRequest
            {
                ClientID = clientID,
                Subject = subject,
                QGroup = queueGroup ?? string.Empty,
                Inbox = inbox,
                MaxInFlight = maxInFlight,
                AckWaitInSecs = ackWaitInSeconds,
                StartPosition = StartPosition.TimeDeltaStart,
                DurableName = durableName ?? string.Empty,

                // Conditionals
                StartTimeDelta = startTimeDelta
            };
        }


        public override STANPacketType PacketType => STANPacketType.SubscriptionRequest;

        internal static long ConvertTimeSpan(TimeSpan ts)
        {
            return ts.Ticks * 100;
        }
    }
}
