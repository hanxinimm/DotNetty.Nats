using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace DotNetty.Codecs.NATSJetStream.Protocol
{
	public class RestoreRequest
	{
		[JsonProperty("config")]
		public JetStreamConfig Config { get; set; }

		[JsonProperty("state")]
		public StreamState State { get; set; }
	}
}
