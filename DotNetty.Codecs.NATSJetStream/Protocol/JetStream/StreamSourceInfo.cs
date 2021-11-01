using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace DotNetty.Codecs.NATSJetStream.Protocol
{
    public class StreamSourceInfo
    {
        /// <summary>
        /// 名称
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; set; }

        /// <summary>
        /// 名称
        /// </summary>
        [JsonProperty("external")]
        public ExternalStream External { get; set; }

        /// <summary>
        /// 名称
        /// </summary>
        [JsonProperty("active")]
        public TimeSpan Active { get; set; }

        /// <summary>
        /// 名称
        /// </summary>
        [JsonProperty("lag")]
        public long Lag { get; set; }

        /// <summary>
        /// 名称
        /// </summary>
        [JsonProperty("error")]
        public ApiError Error { get; set; }

        public override string ToString()
        {
            return $@"StreamSourceInfo {{
                        name='{ Name }'
                        , external={ External }
                        , active={ Active }
                        , lag={ Lag }
                        , error={ Error }
                    }}";
        }

    }
}
