using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace DotNetty.Codecs.NATSJetStream.Protocol
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum DiscardPolicy
    {
        /// <summary>
        /// (默认)会删除旧的消息
        /// </summary>
        [EnumMember(Value = "old")]
        Old,
        /// <summary>
        /// 策略会拒绝新的消息
        /// </summary>
        [EnumMember(Value = "new")]
        New
    }
}
