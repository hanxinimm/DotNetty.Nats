using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace DotNetty.Codecs.NATSJetStream.Protocol
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum RetentionPolicy
    {
        [EnumMember(Value = "limits")]
        Limits,
        [EnumMember(Value = "interest")]
        Interest,
        [EnumMember(Value = "workqueue")]
        WorkQueue
    }
}
