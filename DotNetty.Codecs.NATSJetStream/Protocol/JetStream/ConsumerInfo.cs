using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace DotNetty.Codecs.NATSJetStream.Protocol
{
    public class ConsumerInfo
    {
		//Stream   string        `json:"stream_name"`
		//Name string         `json:"name"`
		//Config ConsumerConfig `json:"config"`
		//Created time.Time      `json:"created"`
		//Delivered SequencePair   `json:"delivered"`
		//AckFloor SequencePair   `json:"ack_floor"`
		//NumAckPending  int            `json:"num_ack_pending"`
		//NumRedelivered int            `json:"num_redelivered"`
		//NumWaiting     int            `json:"num_waiting"`
		//NumPending uint64         `json:"num_pending"`
		//Cluster* ClusterInfo   `json:"cluster,omitempty"`

		/// <summary>
		/// 流名称
		/// </summary>
		[JsonProperty("stream_name")]
        public string StreamName { get; set; }

		/// <summary>
		/// 名称
		/// </summary>
		[JsonProperty("name")]
		public string Name { get; set; }

		/// <summary>
		/// 流配置
		/// </summary>
		[JsonProperty("config")]
		public ConsumerConfig Config { get; set; }

		/// <summary>
		/// 创建时间
		/// </summary>
		[JsonProperty("created")]
		public DateTime Created { get; set; }

		/// <summary>
		/// 
		/// </summary>
		[JsonProperty("delivered")]
		public SequencePair Delivered { get; set; }

		/// <summary>
		/// 
		/// </summary>
		[JsonProperty("ack_floor")]
		public SequencePair AckFloor { get; set; }

		/// <summary>
		/// 
		/// </summary>
		[JsonProperty("num_ack_pending")]
		public int NumAckPending { get; set; }

		/// <summary>
		/// 
		/// </summary>
		[JsonProperty("num_redelivered")]
		public int NumRedelivered { get; set; }

		/// <summary>
		/// 
		/// </summary>
		[JsonProperty("num_waiting")]
		public int NumWaiting { get; set; }

		/// <summary>
		/// 
		/// </summary>
		[JsonProperty("num_pending")]
		public long NumPending { get; set; }

		/// <summary>
		/// 
		/// </summary>
		[JsonProperty("cluster")]
		public ClusterInfo Cluster { get; set; }
	}
}
