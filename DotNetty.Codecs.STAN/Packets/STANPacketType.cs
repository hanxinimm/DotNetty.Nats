using System;
using System.Collections.Generic;
using System.Text;

namespace DotNetty.Codecs.STAN.Packets
{
    public enum STANPacketType
    {
        /// <summary>
        /// Server	Sent to client after initial TCP/IP connection
        /// </summary>
        INFO,
        /// <summary>
        /// heartbeat inbox
        /// </summary>
        Heartbeat,
        /// <summary>
        /// heartbeat inbox Ack
        /// </summary>
        HeartbeatAck,
        /// <summary>
        /// Server	Acknowledges well-formed protocol message in verbose mode
        /// </summary>
        PLUS_OK,
        /// <summary>
        /// Both	PING keep-alive message
        /// </summary>
        PING,
        /// <summary>
        /// Both	PONG keep-alive response
        /// </summary>
        PONG,
        /// <summary>
        /// Server	Indicates a protocol error. May cause client disconnect.
        /// </summary>
        MINUS_ERR,
        /// <summary>
        /// Client	Subscribe Inbox
        /// </summary>
        INBOX,
        /// <summary>
        /// Client	Subscribe to a subject (or subject wildcard)
        /// </summary>
        SUB,
        /// <summary>
        /// Server	Delivers a message payload to a subscriber
        /// </summary>
        MSG,
        /// <summary>
        /// Client	Request to connect to the NATS Streaming Server
        /// </summary>
        ConnectRequest,
        /// <summary>
        /// Server	Result of a connection request
        /// </summary>
        ConnectResponse,
        /// <summary>
        /// Client	Request sent to subscribe and retrieve data
        /// </summary>
        SubscriptionRequest,
        /// <summary>
        /// Server	Result of a subscription request
        /// </summary>
        SubscriptionResponse,
        /// <summary>
        /// Client	Unsubscribe from a subject
        /// </summary>
        UnsubscribeRequest,
        /// <summary>
        /// Server	Result of a Unsubscribe request
        /// </summary>
        UnsubscribeResponse,
        /// <summary>
        /// Client	Publish a message to a subject, with optional reply subject
        /// </summary>
        PubMsg,
        /// <summary>
        /// Server	An acknowledgement that a published message has been processed on the server
        /// </summary>
        PubAck,
        /// <summary>
        /// Server	A message from the NATS Streaming Server to a subscribing client
        /// </summary>
        MsgProto,
        /// <summary>
        /// Client	Acknowledges that a message has been received
        /// </summary>
        Ack,
        /// <summary>
        /// Client	Request sent to close the connection to the NATS Streaming Server
        /// </summary>
        CloseRequest,
        /// <summary>
        /// Server	Result of the close request
        /// </summary>
        CloseResp,
        /// <summary>
        /// Client	Publish more messages
        /// </summary>
        MultiplePubMsg,

    }
}
