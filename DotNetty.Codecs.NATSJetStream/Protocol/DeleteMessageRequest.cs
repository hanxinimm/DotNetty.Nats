using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace DotNetty.Codecs.NATSJetStream.Protocol
{
	public class DeleteMessageRequest
	{
		[JsonProperty("seq")]
		public long? Sequence { get; set; }

		[JsonProperty("no_erase")]
		public bool? NoErase { get; set; }
	}
}
