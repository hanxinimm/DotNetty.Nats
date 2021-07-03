using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace DotNetty.Codecs.NATSJetStream.Protocol
{
	public class PurgeRequest
	{
		[JsonProperty("seq")]
		public long? Sequence { get; set; }
		[JsonProperty("filter")]
		public string Subject { get; set; }
		[JsonProperty("keep")]
		public long? Keep { get; set; }
	}
}
