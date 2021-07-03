using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace DotNetty.Codecs.NATSJetStream.Protocol
{
    public class ConsumerListResponse : JetStreamIterableResponse
    {
        [JsonProperty("consumers")]
        public List<ConsumerInfo> Consumers { get; set; }
    }
}
