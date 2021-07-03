using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace DotNetty.Codecs.NATSJetStream.Protocol
{
	public class PurgeRequest
	{
		// Purge up to but not including sequence.
		[JsonProperty("seq")]
		public long? Sequence { get; set; }

		// Subject to match against messages for the purge command.
		[JsonProperty("filter")]
		public string Subject { get; set; }

		// Number of messages to keep.
		[JsonProperty("keep")]
		public long? Keep { get; set; }
	}
}
