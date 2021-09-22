using System;
using System.Collections.Generic;
using System.Text;

namespace DotNetty.Codecs.NATS
{
    public class NATSSignatures
    {
        public const string CRLF = "\r\n";
        public const string SPACES = " ";

        public const string INFO = "INFO";
        public const string CONNECT = "CONNECT";
        public const string PUB = "PUB";
        public const string HPUB = "HPUB";
        public const string SUB = "SUB";
        public const string UNSUB = "UNSUB";
        public const string MSG = "MSG";
        public const string HMSG = "HMSG";

        public const string PING = "PING";
        public const string PONG = "PONG";
        public const string OK = "+OK";
        public const string ERR = "-ERR";

        public const string HEADER_VERSION = "NATS/1.0";

        // Ack acknowledges a JetStream messages received from a Consumer, indicating the message
        // should not be received again later
        public const string AckAck = "+ACK";
        // Nak acknowledges a JetStream message received from a Consumer, indicating that the message
        // is not completely processed and should be sent again later
        public const string AckNak = "-NAK";
        // AckProgress acknowledges a Jetstream message received from a Consumer, indicating that work is
        // ongoing and further processing time is required equal to the configured AckWait of the Consumer
        public const string AckProgress = "+WPI";
        // AckNext performs an Ack() and request that the next message be sent to subject ib
        public const string AckNext = "+NXT";
        // AckTerm acknowledges a message received from JetStream indicating the message will not be processed
        // and should not be sent to another consumer
        public const string AckTerm = "+TERM";
    }
}
