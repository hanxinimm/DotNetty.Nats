using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace DotNetty.Codecs.NATSJetStream.JetStream
{
	public class AccountStatistics
	{
		[JsonProperty("memory")]
		public long Memory { get; set; }
		[JsonProperty("storage")]
		public long Storage { get; set; }
		[JsonProperty("streams")]
		public long Streams { get; set; }
		[JsonProperty("consumers")]
		public long Consumers { get; set; }
		[JsonProperty("domain")]
		public string Domain { get; set; }
		[JsonProperty("api")]
		public APIStatistics API { get; set; }
		[JsonProperty("limits")]
		public AccountLimits Limits { get; set; }

		public override string ToString()
		{
			return $@"AccountStatistics {{
						memory={Memory}
						,storage={Storage}
						,streams={Streams}
						,consumers={Consumers}
						,domain='{Domain}'
						,api={API}
						,limits={Limits}
					}}";
		}
	}
}
