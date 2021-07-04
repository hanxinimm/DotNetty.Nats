using System;
using System.Collections.Generic;
using System.Text;

namespace DotNetty.Codecs.Protocol
{
    public enum MessageAck
    {
        Ack,
        Nak,
        Progress,
        Next,
        Term
    }
}
