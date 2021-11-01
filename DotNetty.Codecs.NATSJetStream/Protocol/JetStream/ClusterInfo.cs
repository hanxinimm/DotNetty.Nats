using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DotNetty.Codecs.NATSJetStream.Protocol
{
    public class ClusterInfo
    {
        /// <summary>
        /// 名称
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; set; }

        /// <summary>
        /// 节点领袖
        /// </summary>
        [JsonProperty("leader")]
        public string Leader { get; set; }

        /// <summary>
        /// 节点分片
        /// </summary>
        [JsonProperty("replicas")]
        public List<PeerInfo> Replicas { get; set; } = new List<PeerInfo>();

        public override string ToString()
        {
            return @$"SequencePair {{
					    name='{ Name }' 
					    , leader='{ Leader }'
					    , replicas= { string.Join("|", Replicas.Select(v => v.ToString())) }
					}}";
        }
    }
}
