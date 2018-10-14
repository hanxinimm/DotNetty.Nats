using System;
using System.Collections.Generic;
using System.Text;

namespace DotNetty.Codecs.STAN.Packets
{
    public enum STANPacketType
    {
        /// <summary>
        /// Client	Request to connect to the NATS Streaming Server
        /// </summary>
        ConnectRequest = 0,
        /// <summary>
        /// Server	Result of a connection request
        /// </summary>
        ConnectResponse = 1,
        /// <summary>
        /// Client	Request sent to subscribe and retrieve data
        /// </summary>
        SubscriptionRequest = 2,
        /// <summary>
        /// Server	Result of a subscription request
        /// </summary>
        SubscriptionResponse = 3,
        /// <summary>
        /// Client	Unsubscribe from a subject
        /// </summary>
        UnsubscribeRequest = 4,
        /// <summary>
        /// Client	Publish a message to a subject, with optional reply subject
        /// </summary>
        PubMsg = 5,
        /// <summary>
        /// Server	An acknowledgement that a published message has been processed on the server
        /// </summary>
        PubAck = 6,
        /// <summary>
        /// Server	A message from the NATS Streaming Server to a subscribing client
        /// </summary>
        MsgProto = 7,
        /// <summary>
        /// Client	Acknowledges that a message has been received
        /// </summary>
        Ack = 8,
        /// <summary>
        /// Client	Request sent to close the connection to the NATS Streaming Server
        /// </summary>
        CloseRequest = 9,
        /// <summary>
        /// Server	Result of the close request
        /// </summary>
        CloseResp = 10,
        /// <summary>
        /// heartbeat inbox
        /// </summary>
        Heartbeat = 11
    }
}
