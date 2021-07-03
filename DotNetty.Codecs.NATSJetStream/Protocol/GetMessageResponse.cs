using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace DotNetty.Codecs.NATSJetStream.Protocol
{
    public class GetMessageResponse : JetStreamResponse
    {
        [JsonProperty("success")]
        public StoredMessage Message { get; set; }
    }
}
