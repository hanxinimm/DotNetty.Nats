using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace DotNetty.Codecs.NATSJetStream.Protocol
{
    public class JetStreamMsgMetaData
    {
        [JsonProperty("stream")]
        public string Stream { get; set; }
        [JsonProperty("consumer")]
        public string Consumer { get; set; }
        [JsonProperty("parsed")]
        public bool Parsed { get; set; }
        [JsonProperty("delivered")]
        public int Delivered { get; set; }
        [JsonProperty("stream_seq")]
        public int StreamSequence { get; set; }
        [JsonProperty("consumer_seq")]
        public int ConsumerSequence { get; set; }
        [JsonProperty("timestamp")]
        public DateTime TimeStamp { get; set; }
    }
}
