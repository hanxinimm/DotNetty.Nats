using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace DotNetty.Codecs.NATSJetStream.JetStream
{
    public class APIStatistics
    {
        [JsonProperty("total")]
        public long Total { get; set; }

        [JsonProperty("errors")]
        public long Errors { get; set; }

		public override string ToString()
		{
			return $@"APIStatistics {{
						total={Total}
						,errors={Errors}
					}}";
		}
	}
}
