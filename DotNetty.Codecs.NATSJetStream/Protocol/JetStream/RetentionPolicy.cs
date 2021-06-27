using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Text;

namespace DotNetty.Codecs.NATSJetStream.JetStream
{
    [JsonConverter(typeof(StringEnumConverter))]

    public enum RetentionPolicy
    {
        [JsonProperty("limits")]
        Limits,
        [JsonProperty("interest")]
        Interest,
        [JsonProperty("workqueue")]
        WorkQueue
    }
}
