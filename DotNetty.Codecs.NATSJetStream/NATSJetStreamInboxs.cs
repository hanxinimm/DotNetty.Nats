using System;
using System.Collections.Generic;
using System.Text;

namespace DotNetty.Codecs.NATSJetStream
{
    public class NATSJetStreamInboxs
    {
        public const string InboxPrefix = "_INBOX.";
        public const string Heartbeat = "_INBOX.HTBT.";

        public const string CreateResponse = "_INBOX.JM.CERE.";

        public const string SubscriptionResponse = "_INBOX.SNRE.";
        public const string UnSubscriptionResponse = "_INBOX.USRE.";
        public const string PubAck = "_INBOX.PBAK.";
        public const string PingResponse = "_INBOX.PGRE.";
        public const string MsgProto = "_INBOX.MGPO.";
        public const string CloseResponse = "_INBOX.CERP.";
    }
}
