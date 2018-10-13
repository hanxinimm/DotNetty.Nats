using System;
using System.Collections.Generic;
using System.Text;

namespace Hunter.NATS.Client
{
    internal class NATSConstants
    {
        internal const string _CRLF_ = "\r\n";
        internal const string _EMPTY_ = "";
        internal const string _SPC_ = " ";
        internal const string _PUB_P_ = "PUB ";

        internal const string _OK_OP_ = "+OK";
        internal const string _ERR_OP_ = "-ERR";
        internal const string _MSG_OP_ = "MSG";
        internal const string _PING_OP_ = "PING";
        internal const string _PONG_OP_ = "PONG";
        internal const string _INFO_OP_ = "INFO";

        internal const string inboxPrefix = "_INBOX.";

        internal const string conProto = "CONNECT {0}" + _CRLF_;
        internal const string pingProto = "PING" + _CRLF_;
        internal const string pongProto = "PONG" + _CRLF_;
        internal const string pubProto = "PUB {0} {1} {2}" + _CRLF_;
        internal const string subProto = "SUB {0} {1} {2}" + _CRLF_;
        internal const string unsubProto = "UNSUB {0} {1}" + _CRLF_;

        internal const string pongProtoNoCRLF = "PONG";
        internal const string okProtoNoCRLF = "+OK";

        internal const string STALE_CONNECTION = "stale connection";
        internal const string AUTH_TIMEOUT = "authorization timeout";
    }
}
