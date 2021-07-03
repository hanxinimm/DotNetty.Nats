using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace DotNetty.Codecs.NATSJetStream.Protocol
{
    public class RestoreResponse : JetStreamResponse
    {
        // Subject to deliver the chunks to for the snapshot restore.
        [JsonProperty("deliver_subject")]
        public string DeliverSubject { get; set; }
    }
}
