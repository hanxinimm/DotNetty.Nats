using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace DotNetty.Codecs.NATSJetStream.Protocol
{
    public class StreamState
    {
        /// <summary>
        /// 消息总数
        /// </summary>
        [JsonProperty("messages")]
        public long MessageTotal { get; set; }

        /// <summary>
        /// 字节长度
        /// </summary>
        [JsonProperty("bytes")]
        public long ByteLength { get; set; }

        /// <summary>
        /// 首条消息的序号
        /// </summary>
        [JsonProperty("first_seq")]
        public long FirstSequence { get; set; }

        /// <summary>
        /// 首条消息时间
        /// </summary>
        [JsonProperty("first_ts")]
        public DateTime FirstTime { get; set; }

        /// <summary>
        /// 最后一条消息序号
        /// </summary>
        [JsonProperty("last_seq")]
        public long LastSequence { get; set; }

        /// <summary>
        /// 最后一条消息时间
        /// </summary>
        [JsonProperty("last_ts")]
        public DateTime LastTime { get; set; }

        /// <summary>
        /// 删除的消息数量
        /// </summary>
        [JsonProperty("num_deleted")]
        public int NumDeleted { get; set; }

        /// <summary>
        /// 删除的消息编号
        /// </summary>
        [JsonProperty("deleted")]
        public List<long> Deleted { get; set; }

        /// <summary>
        /// 流状态
        /// </summary>
        [JsonProperty("lost")]
        public LostStreamData Lost { get; set; }

        /// <summary>
        /// 消费者数量
        /// </summary>
        [JsonProperty("consumer_count")]
        public int Consumers { get; set; }


    }
}
