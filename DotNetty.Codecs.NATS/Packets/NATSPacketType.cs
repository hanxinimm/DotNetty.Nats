using System;
using System.Collections.Generic;
using System.Text;

namespace DotNetty.Codecs.NATS.Packets
{
    public enum NATSPacketType
    {
        /// <summary>
        /// Server	Sent to client after initial TCP/IP connection
        /// </summary>
        INFO = 0,
        /// <summary>
        /// Client	Sent to server to specify connection information
        /// </summary>
        CONNECT = 1,
        /// <summary>
        /// Client	Publish a message to a subject, with optional reply subject
        /// </summary>
        PUB = 2,
        /// <summary>
        /// Client	Subscribe to a subject (or subject wildcard)
        /// </summary>
        SUB = 3,
        /// <summary>
        /// Client	Unsubscribe (or auto-unsubscribe) from subject
        /// </summary>
        UNSUB = 4,
        /// <summary>
        /// Server	Delivers a message payload to a subscriber
        /// </summary>
        MSG = 5,
        /// <summary>
        /// Both	PING keep-alive message
        /// </summary>
        PING = 6,
        /// <summary>
        /// Both	PONG keep-alive response
        /// </summary>
        PONG = 7,
        /// <summary>
        /// Server	Acknowledges well-formed protocol message in verbose mode
        /// </summary>
        PLUS_OK = 8,
        /// <summary>
        /// Server	Indicates a protocol error. May cause client disconnect.
        /// </summary>
        MINUS_ERR = 9,

        #region JetStream

        STREAM_INBOX,

        STREAM_CREATE,

        STREAM_CREATE_RESPONSE,

        STREAM_UPDATE,

        STREAM_UPDATE_RESPONSE,

        STREAM_INFO,

        STREAM_INFO_RESPONSE,

        STREAM_LIST,

        STREAM_LIST_RESPONSE,

        CONSUMER_CREATE,

        CONSUMER_CREATE_RESPONSE,

        #endregion;
    }
}
