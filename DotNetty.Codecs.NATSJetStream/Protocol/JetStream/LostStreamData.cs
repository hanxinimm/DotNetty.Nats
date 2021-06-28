using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace DotNetty.Codecs.NATSJetStream.Protocol
{
    public class LostStreamData
    {
        /// <summary>
        /// 消息集合
        /// </summary>
        [JsonProperty("msgs")]
        public List<long> Messages { get; set; }

        /// <summary>
        /// 消息字节长度
        /// </summary>
        [JsonProperty("bytes")]
        public long Bytes { get; set; }
    }
}
