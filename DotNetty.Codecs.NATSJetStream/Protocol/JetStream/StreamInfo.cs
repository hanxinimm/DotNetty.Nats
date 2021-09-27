using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace DotNetty.Codecs.NATSJetStream.Protocol
{
    public class StreamInfo
    {
        /// <summary>
        /// 流配置
        /// </summary>
        [JsonProperty("config")]
        public JetStreamConfig Config { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        [JsonProperty("created")]
        public DateTime Created { get; set; }

        /// <summary>
        /// 流状态
        /// </summary>
        [JsonProperty("state")]
        public StreamState State { get; set; }

        /// <summary>
        /// 节点信息
        /// </summary>
        [JsonProperty("cluster")]
        public ClusterInfo Cluster { get; set; }

        /// <summary>
        /// 节点信息
        /// </summary>
        [JsonProperty("mirror")]
        public StreamSourceInfo Mirror { get; set; }

        /// <summary>
        /// 节点信息
        /// </summary>
        [JsonProperty("sources")]
        public List<StreamSourceInfo> Sources { get; set; }

        public override string ToString()
        {
            return $@"StreamInfo {{
                        config='{ Config }'
                        , created={ Created }
                        , state={ State }
                        , cluster={ Cluster }
                        , mirror={ Mirror }
                        , sources={ string.Join("|", Sources) }
                    }}";
        }
    }
}
