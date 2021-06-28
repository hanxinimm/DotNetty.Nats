using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace DotNetty.Codecs.NATSJetStream.Protocol
{
    public class ListResponse : JetStreamIterableResponse
    {
        [JsonProperty("streams")]
        public List<StreamInfo> Streams { get; set; }
    }
}
