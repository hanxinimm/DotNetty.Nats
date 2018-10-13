using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace DotNetty.Codecs.STAN.Packets
{
    [DataContract]
    public class SubscribePacket : Packet
    {
        [IgnoreDataMember]
        public override PacketType PacketType => PacketType.SUB;

        public SubscribePacket(string id, string subject,string group)
        {
            Id = id;
            Subject = subject;
            Group = group;
        }

        public SubscribePacket(string id, string subject)
        {
            Id = id;
            Subject = subject;
        }

        /// <summary>
        /// 唯一的字母数字订阅ID
        /// </summary>
        [DataMember(Name = "sid")]
        public string Id { get; set; }

        /// <summary>
        /// 订阅的主题名称
        /// </summary>

        [DataMember(Name = "subject")]
        public string Subject { get; set; }

        /// <summary>
        /// 如果指定，订户将加入此队列组
        /// </summary>

        [DataMember(Name = "queue group")]
        public string Group { get; set; }
    }
}
