using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace DotNetty.Codecs.NATSJetStream.Protocol
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum DeliverPolicy
    {
        [EnumMember(Value = "all")]
        DeliverAll,
        [EnumMember(Value = "last")]
        DeliverLast,
        [EnumMember(Value = "new")]
        DeliverNew,
        [EnumMember(Value = "by_start_sequence")]
        DeliverByStartSequence,
        [EnumMember(Value = "by_start_time")]
        DeliverByStartTime
    }
}
