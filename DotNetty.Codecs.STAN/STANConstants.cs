using System;
using System.Collections.Generic;
using System.Text;

namespace DotNetty.Codecs.STAN
{
    public class STANConstants
    {
        internal const byte FIELDDELIMITER_SPACES = 0x20;

        internal const byte FIELDDELIMITER_TAB = 0x09;

        internal const byte NEWLINES_CR = 0x0D;

        internal const byte NEWLINES_LF = 0x0A;

        internal const string InboxPrefix = "_INBOX.";

        /// <summary>
        /// NATS C# streaming client version.
        /// </summary>
	    internal const string Version = "0.0.1";

        /// <summary>
        /// NatsURL is the default URL the client connects to.
        /// </summary>
        internal const string NatsURL = "nats://localhost:4222";

        /// <summary>
        /// ConnectWait is the default timeout used for the connect operation.
        /// </summary>
        internal const int ConnectWait = 2000;

        /// <summary>
        /// DiscoverPrefix is the prefix subject used to connect to the NATS Streaming server.
        /// </summary>
        internal const string DiscoverPrefix = "_STAN.discover";

        /// <summary>
        /// ACKPrefix is the prefix subject used to send ACKs to the NATS Streaming server.
        /// </summary>
        internal const string ACKPrefix = "_STAN.acks";

        /// <summary>
        /// MaxPubAcksInflight is the default maximum number of published messages
	    /// without outstanding ACKs from the server.
        /// </summary>
        internal const long MaxPubAcksInflight = 16384;

        /// <summary>
        /// AckWait indicates how long the server should wait for an ACK before resending a message.
        /// </summary>
        internal const long AckWait = 30000;

        /// <summary>
        /// MaxInflight indicates how many messages with outstanding ACKs the server can send.
        /// </summary>
        internal const int MaxInflight = 1024;
    }
}
