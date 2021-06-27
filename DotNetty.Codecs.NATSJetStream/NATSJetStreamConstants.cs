using System;
using System.Collections.Generic;
using System.Text;

namespace DotNetty.Codecs.NATSJetStream
{
    public static class NATSJetStreamConstants
    {
        internal const string MSG_ID_HDR = "Nats-Msg-Id";
        internal const string EXPECTED_STREAM_HDR = "Nats-Expected-Stream";
        internal const string EXPECTED_LAST_SEQ_HDR = "Nats-Expected-Last-Sequence";
        internal const string EXPECTED_LAST_MSG_ID_HDR = "Nats-Expected-Last-Msg-Id";

        internal const int MAX_PULL_SIZE = 256;
    }
}
