using DotNetty.Codecs.STAN.Protocol;
using System;
using System.Collections.Generic;
using System.Text;

namespace DotNetty.Codecs.STAN.Packets
{
    public class SubscriptionRequestPacket : STANPublishPacket<SubscriptionRequest>
    {
        public SubscriptionRequestPacket(string clientID, string subject, string queueGroup, string inbox, int maxInFlight,
            int ackWaitInSeconds, string durableName, StartPosition startPosition)
        {
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

        public SubscriptionRequestPacket(string clientID, string subject, string queueGroup, string inbox, int maxInFlight,
            int ackWaitInSeconds, string durableName, ulong startSequence)
        {
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

        public SubscriptionRequestPacket(string clientID, string subject, string queueGroup, string inbox, int maxInFlight,
            int ackWaitInSeconds, string durableName, long startTimeDelta)
        {
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
