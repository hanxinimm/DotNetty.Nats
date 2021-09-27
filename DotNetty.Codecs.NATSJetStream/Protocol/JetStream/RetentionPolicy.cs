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
        /// <summary>
        /// 对消息的数量、存储容量和年龄进行限制
        /// </summary>
        [EnumMember(Value = "limits")]
        Limits,
        /// <summary>
        /// 只要有消费者处于活跃状态，消息就会被保存下来
        /// </summary>
        [EnumMember(Value = "interest")]
        Interest,
        /// <summary>
        /// 直到被一个观察者消耗之前，消息都会保存
        /// </summary>
        [EnumMember(Value = "workqueue")]
        WorkQueue
    }
}
