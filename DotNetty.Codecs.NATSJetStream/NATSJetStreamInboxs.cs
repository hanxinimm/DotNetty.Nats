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
        public const string PurgeResponse = InboxPrefix + "JTSM_PERE.";
        public const string DeleteMessageResponse = InboxPrefix + "JTSM_DMRE.";
        public const string GetMessageResponse = InboxPrefix + "JTSM_GMRE.";
        public const string SnapshotResponse = InboxPrefix + "JTSM_STRE.";
        public const string RestoreResponse = InboxPrefix + "JTSM_RERE.";
        public const string RemovePeerResponse = InboxPrefix + "JTSM_RPRE.";
        public const string LeaderStepDownResponse = InboxPrefix + "JTSM_LSDR.";

        public const string ConsumerCreateResponse = InboxPrefix + "CSER_CERE.";
        public const string ConsumerNamesResponse = InboxPrefix + "CSER_CNRE.";
        public const string ConsumerListResponse = InboxPrefix + "CSER_CLRE.";
        public const string ConsumerInfoResponse = InboxPrefix + "CSER_CIRE.";

        public const string ConsumerDeleteResponse = InboxPrefix + "CSER_CDRE.";
        public const string ConsumerMessageNextResponse = InboxPrefix + "CSER_CMNR.";
        public const string ConsumerLeaderStepDownResponse = InboxPrefix + "CSER_CLSR.";
    }
}
