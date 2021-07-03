using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace DotNetty.Codecs.NATSJetStream.Protocol
{
	public class GetMessageRequest
	{
		[JsonProperty("seq")]
		public long? Sequence { get; set; }

		[JsonProperty("last_by_subject")]
		public string LastSubject { get; set; }
	}
}
