using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace DotNetty.Codecs.NATSJetStream.JetStream
{
	public class AccountLimits
	{
		[JsonProperty("max_memory")]
		public long MaxMemory { get; set; }

		[JsonProperty("max_storage")]
		public long MaxStorage { get; set; }

		[JsonProperty("max_streams")]
		public int MaxStreams { get; set; }

		[JsonProperty("max_consumers")]
		public int MaxConsumers { get; set; }

		public override string ToString()
		{
			return $@"AccountLimit {{
					memory={MaxMemory}
					, storage={MaxStorage}
					, streams={MaxStreams}
					, consumers={MaxConsumers}
					'}}";
		}
	}
}
