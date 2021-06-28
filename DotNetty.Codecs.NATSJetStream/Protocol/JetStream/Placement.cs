using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DotNetty.Codecs.NATSJetStream.Protocol
{
    public class Placement
    {
        [JsonProperty("cluster")]
        public string Cluster { get; set; }
        [JsonProperty("tags")]
        public List<string> Tags { get; set; }

        public override string ToString()
        {
            return $@"APIStatistics {{
						cluster='{Cluster}'
						,tags='{string.Join(",", Tags)}'
					}}";
        }
    }
}
