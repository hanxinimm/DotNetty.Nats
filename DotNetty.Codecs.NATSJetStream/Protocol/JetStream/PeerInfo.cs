using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace DotNetty.Codecs.NATSJetStream.Protocol
{
    public class PeerInfo  
    {
        /// <summary>
        /// 名称
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; set; }

        /// <summary>
        /// 是否未当前节点
        /// </summary>
        [JsonProperty("current")]
        public bool Current { get; set; }

        /// <summary>
        /// 是否离线
        /// </summary>
        [JsonProperty("offline")]
        public bool IsOffline { get; set; }

        /// <summary>
        /// 激活持续时间
        /// </summary>
        [JsonProperty("active")]
        public TimeSpan Active { get; set; }

        /// <summary>
        /// 名称
        /// </summary>
        [JsonProperty("lag")]
        public long Lag { get; set; }

    }
}
