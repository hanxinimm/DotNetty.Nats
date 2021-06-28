using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace DotNetty.Codecs.NATSJetStream.Protocol
{
    public class ConsumerCreateRequest
    {
        public ConsumerCreateRequest() { }

        public ConsumerCreateRequest(string streamName, ConsumerConfig config)
        {
            Stream = streamName;
            Config = config;
        }

        /// <summary>
        /// 流名称
        /// </summary>
        [JsonProperty("stream_name")]
        public string Stream { get; set; }

        /// <summary>
        /// 消费者配置
        /// </summary>
        [JsonProperty("config")]
        public ConsumerConfig Config { get; set; }
    }
}
