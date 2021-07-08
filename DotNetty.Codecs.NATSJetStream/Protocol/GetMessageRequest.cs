using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace DotNetty.Codecs.NATSJetStream.Protocol
{
	public class GetMessageRequest
	{
		public GetMessageRequest() { }
		public GetMessageRequest(long sequence)
		{
			Sequence = sequence;
		}
		public GetMessageRequest(string subject)
		{
			LastSubject = subject;
		}

		[JsonProperty("seq")]
		public long? Sequence { get; set; }

		[JsonProperty("last_by_subject")]
		public string LastSubject { get; set; }
	}
}
