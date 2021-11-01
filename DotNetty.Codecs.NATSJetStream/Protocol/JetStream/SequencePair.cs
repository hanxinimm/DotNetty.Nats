using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace DotNetty.Codecs.NATSJetStream.Protocol
{
    public class SequencePair
    {
        /// <summary>
        /// 消费者序号
        /// </summary>
        [JsonProperty("consumer_seq")]
        public long Consumer { get; set; }

        /// <summary>
        /// 流序号
        /// </summary>
        [JsonProperty("stream_seq")]
        public long Stream { get; set; }

        public override string ToString()
        {
            return @$"SequencePair {{
					    consumer_seq={ Consumer }
					    , stream_seq= { Stream }
					}}";
        }
    }
}
