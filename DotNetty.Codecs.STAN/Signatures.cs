using System;
using System.Collections.Generic;
using System.Text;

namespace DotNetty.Codecs.STAN
{
    public class Signatures
    {
        internal const string CRLF = "\r\n";
        internal const string SPACES = " ";

        internal const string INFO = "INFO";
        internal const string CONNECT = "CONNECT";
        internal const string PUB = "PUB";
        internal const string SUB = "SUB";
        internal const string UNSUB = "UNSUB";
        internal const string MSG = "MSG";
        internal const string PING = "PING";
        internal const string PONG = "PONG";
        internal const string OK = "+OK";
        internal const string ERR = "-ERR";
    }
}
