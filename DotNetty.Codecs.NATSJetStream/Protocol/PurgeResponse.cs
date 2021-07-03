using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace DotNetty.Codecs.NATSJetStream.Protocol
{
    public class PurgeResponse : JetStreamResponse
    {
        [JsonProperty("success")]
        public bool Success { get; set; }

        [JsonProperty("purged")]
        public long Purged { get; set; }
    }
}
