using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace DotNetty.Codecs.NATSJetStream.Protocol
{
    public class PubAckResponse : JetStreamResponse
    {
        [JsonProperty("stream")]
        public string Stream { get; set; }

        [JsonProperty("seq")]
        public long Sequence { get; set; }

        [JsonProperty("duplicate")]
        public bool Duplicate { get; set; }
    }
}
