using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace DotNetty.Codecs.NATSJetStream.Protocol
{
    public class JetStreamResponse
    {
        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("error")]
        public ApiError Error { get; set; }
    }
}
