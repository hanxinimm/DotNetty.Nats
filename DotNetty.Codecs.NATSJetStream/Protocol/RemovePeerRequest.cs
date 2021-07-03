using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace DotNetty.Codecs.NATSJetStream.Protocol
{
    public class RemovePeerRequest : JetStreamResponse
    {
        [JsonProperty("peer")]
        public string Peer { get; set; }
    }
}
