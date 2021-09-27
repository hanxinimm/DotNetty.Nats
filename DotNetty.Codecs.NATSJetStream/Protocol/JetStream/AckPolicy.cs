using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace DotNetty.Codecs.NATSJetStream.Protocol
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum AckPolicy
    {
        /// <summary>
        /// 要求每个消息都进行手动确认，这也是拉模型下唯一支持的方式
        /// </summary>
        [EnumMember(Value = "explicit")]
        AckExplicit,
        /// <summary>
        /// 不支持任何确认
        /// </summary>
        [EnumMember(Value = "none")]
        AckNone,
        /// <summary>
        /// 这个模式下，如果你确认了第100个消息，那么1-99个消息都会自动确认，适用于批处理任务，以减少确认带来的额外开销
        /// </summary>
        [EnumMember(Value = "all")]
        AckAll,
    }
}
