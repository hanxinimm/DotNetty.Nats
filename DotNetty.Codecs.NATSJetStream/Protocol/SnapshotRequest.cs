using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace DotNetty.Codecs.NATSJetStream.Protocol
{
	public class SnapshotRequest
	{
		// Subject to deliver the chunks to for the snapshot.
		[JsonProperty("deliver_subject")]
		public string DeliverSubject { get; set; }
		// Do not include consumers in the snapshot.
		[JsonProperty("no_consumers")]
		public bool? NoConsumers { get; set; }
		// Optional chunk size preference. Otherwise server selects.
		[JsonProperty("chunk_size")]
		public int? ChunkSize { get; set; }
		// Check all message's checksums prior to snapshot.
		[JsonProperty("jsck")]
		public bool? CheckMsgs { get; set; }
	}
}
