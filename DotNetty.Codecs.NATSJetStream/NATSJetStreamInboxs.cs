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
        public const string UpdateResponse = InboxPrefix + "JTSM_UERE.";
        public const string DeleteResponse = InboxPrefix + "JTSM_DERE.";
        public const string InfoResponse = InboxPrefix + "JTSM_IORE.";
        public const string NamesResponse = InboxPrefix + "JTSM_NSRE.";
        public const string ListResponse = InboxPrefix + "JTSM_LTRE.";

        public const string ConsumerCreateResponse = InboxPrefix + "CSER_CERE.";

    }
}
