using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace DotNetty.Codecs.NATSJetStream.Protocol
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum StorageType
    {
        [EnumMember( Value = "file")]
        File,
        [EnumMember(Value = "memory")]
        Memory,
    }
}
