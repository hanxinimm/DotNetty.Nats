using System;
using System.Collections.Generic;
using System.Text;

namespace DotNetty.Codecs.NATSJetStream.Protocol
{
    public class MessageMetadata
    {
        public string Stream { get; set; }
        public string Consumer { get; set; }
        public long Delivered { get; set; }
        public long StreamSequence { get; set; }
        public long ConsumerSequence { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
