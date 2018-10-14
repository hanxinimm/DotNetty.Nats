using System;
using System.Collections.Generic;
using System.Text;

namespace DotNetty.Codecs.STAN
{
    public class STANSignatures
    {
        internal const string ConnectRequest = "ConnectRequest";
        internal const string ConnectResponse = "ConnectResponse";
        internal const string SubscriptionRequest = "SubscriptionRequest";
        internal const string SubscriptionResponse = "SubscriptionResponse";
        internal const string UnsubscribeRequest = "UnsubscribeRequest";
        internal const string PubMsg = "PubMsg";
        internal const string PubAck = "PubAck";
        internal const string MsgProto = "MsgProto";
        internal const string Ack = "Ack";
        internal const string CloseRequest = "CloseRequest";
        internal const string CloseResp = "CloseResp";
    }
}
