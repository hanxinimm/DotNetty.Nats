using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace DotNetty.Codecs.NATSJetStream.Protocol
{
    public class NamesResponse : JetStreamIterableResponse
    {
        [JsonProperty("streams")]
        public List<string> Streams { get; set; }
    }
}
