using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace DotNetty.Codecs.NATSJetStream.JetStream
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

        public override string ToString()
        {
            return $@"StreamSource {{
                        sourceName= '{Name}' 
                        , startSeq={StartSequence}
                        , startTime={StartTime}
                        , filterSubject='{FilterSubject}'
                        , external={External}
                    }}";
        }
    }
}
