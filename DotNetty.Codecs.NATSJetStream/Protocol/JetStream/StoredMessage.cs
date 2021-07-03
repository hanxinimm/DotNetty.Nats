using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace DotNetty.Codecs.NATSJetStream.Protocol
{
    public class StoredMessage
    {
        [JsonProperty("subject")]
        public string Subject { get; set; }

        [JsonProperty("seq")]
        public long Sequence { get; set; }

        [JsonProperty("hdrs")]
        public byte[] Header { get; set; }

        [JsonProperty("data")]
        public byte[] Data { get; set; }

        [JsonProperty("time")]
        public DateTime Time { get; set; }
    }
}
