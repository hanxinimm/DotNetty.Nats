using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Text;

namespace Hunter.NATS.Client.JetStream
{
    [JsonConverter(typeof(StringEnumConverter))]

    public enum DiscardPolicy
    {
        [JsonProperty("old")]
        Old,
        [JsonProperty("new")]
        New
    }
}
