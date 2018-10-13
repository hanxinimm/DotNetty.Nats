using System;
using System.Collections.Generic;
using System.Text;

namespace Hunter.NATS.Client
{
    public sealed class MessageArgs
    {
        public string subject;
        public string reply;
        public long SubscriptionId { get; set; }
        public int Size { get; set; }
    }
}
