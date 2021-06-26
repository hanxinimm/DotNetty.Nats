using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Hunter.NATS.Client.JetStream
{
    public class StreamSource
    {
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("opt_start_seq")]
        public long StartSequence { get; set; }
        [JsonProperty("opt_start_time")]
        public DateTimeOffset StartTime { get; set; }
        [JsonProperty("filter_subject")]
        public string FilterSubject { get; set; }
        [JsonProperty("external")]
        public ExternalStream External { get; set; }
        [JsonProperty("opt_start_seq")]
        public string ObjectName { get; set; }


        public override string ToString()
        {
            return ObjectName + "{" +
                    "sourceName='" + Name + '\'' +
                    ", startSeq=" + StartSequence +
                    ", startTime=" + StartTime +
                    ", filterSubject='" + FilterSubject + '\'' +
                    ", " + External +
                    '}';
        }
    }
}
