using System;
using System.Collections.Generic;
using System.Text;

namespace DotNetty.Codecs.NATS
{
    public class ProtocolSignatures
    {
        internal const string CRLF = "\r\n";
        internal const string SPACES = " ";
        internal const string COLON = ":";

        internal const string INFO = "INFO";
        internal const string CONNECT = "CONNECT";
        internal const string PUB = "PUB";
        internal const string HPUB = "HPUB";
        internal const string SUB = "SUB";
        internal const string UNSUB = "UNSUB";
        internal const string MSG = "MSG";
        internal const string PING = "PING";
        internal const string PONG = "PONG";
        internal const string OK = "+OK";
        internal const string ERR = "-ERR";

        internal const string HEADER_VERSION = "NATS/1.0";

        // Ack acknowledges a JetStream messages received from a Consumer, indicating the message
        // should not be received again later
        internal const string AckAck = "+ACK";
        // Nak acknowledges a JetStream message received from a Consumer, indicating that the message
        // is not completely processed and should be sent again later
        internal const string AckNak = "-NAK";
        // AckProgress acknowledges a Jetstream message received from a Consumer, indicating that work is
        // ongoing and further processing time is required equal to the configured AckWait of the Consumer
        internal const string AckProgress = "+WPI";
        // AckNext performs an Ack() and request that the next message be sent to subject ib
        internal const string AckNext = "+NXT";
        // AckTerm acknowledges a message received from JetStream indicating the message will not be processed
        // and should not be sent to another consumer
        internal const string AckTerm = "+TERM";
    }
}
