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
    }
}
