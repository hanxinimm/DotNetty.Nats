using System;
using System.Collections.Generic;
using System.Text;

namespace Hunter.STAN.Client
{
    public class STANMsgPubAck
    {
        public STANMsgPubAck() { }

        public STANMsgPubAck(string guid, string error)
        {
            Guid = guid;
            Error = error;
        }

        public string Guid { get; set; }

        public string Error { get; set; }
    }
}
