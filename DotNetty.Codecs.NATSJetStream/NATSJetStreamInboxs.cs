using System;
using System.Collections.Generic;
using System.Text;

namespace DotNetty.Codecs.NATSJetStream
{
    public class NATSJetStreamInboxs
    {
        public const string InboxPrefix = "_INBOX.";
        public const string Heartbeat = InboxPrefix + "NATS_HTBT.";

        public const string CreateResponse = InboxPrefix + "JTSM_CERE.";
    }
}
