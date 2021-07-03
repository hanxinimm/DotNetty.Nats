using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace DotNetty.Codecs.NATSJetStream.Protocol
{
    public class ConsumerNamesResponse : JetStreamIterableResponse
    {
        [JsonProperty("consumers")]
        public List<string> Consumers { get; set; }
    }
}
