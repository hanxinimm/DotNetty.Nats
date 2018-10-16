using System;
using System.Collections.Generic;
using System.Text;

namespace DotNetty.Codecs.STAN
{
    public class STANSignatures
    {
        internal const string INFO = "INFO";
        internal const string SUB = "SUB";
        internal const string MSG = "MSG";
        internal const string OK = "+OK";
        internal const string PING = "PING";
        internal const string PONG = "PONG";
        internal const string ERR = "-ERR";
    }
}
