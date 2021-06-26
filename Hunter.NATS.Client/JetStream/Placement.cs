using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Hunter.NATS.Client.JetStream
{
    public class Placement
    {
        [JsonProperty("cluster")]
        public string Cluster { get; set; }
        [JsonProperty("tags")]
        public List<string> Tags { get; set; }
    }
}
