using DotNetty.Buffers;
using DotNetty.Common;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace DotNetty.Codecs.NATS.Packets
{
    [DataContract]
    public class MessagePacket : NATSPacket
    {
        [IgnoreDataMember]
        public override NATSPacketType PacketType => NATSPacketType.MSG;
        public MessagePacket() { }

        public MessagePacket(string subject, string subscribeId, string replyTo, int payloadSize, byte[] payload)
        {
            Subject = subject;
            SubscribeId = subscribeId;
            ReplyTo = replyTo;
            Payload = payload;
            PayloadSize = payloadSize;
        }

        /// <summary>
        /// 要发布到的目标主题
        /// </summary>
        [DataMember(Name = "subject")]
        public string Subject { get; set; }

        /// <summary>
        /// 主题的唯一字母数字订阅ID
        /// </summary>
        [DataMember(Name = "sid")]
        public string SubscribeId { get; set; }

        /// <summary>
        /// 发布商正在侦听响应的收件箱主题
        /// </summary>
        [DataMember(Name = "reply-to")]
        public string ReplyTo { get; set; }

        /// <summary>
        /// 以字节为单位的有效载荷大小
        /// </summary>
        [IgnoreDataMember]
        public int PayloadSize { get; set; }

        /// <summary>
        /// 消息有效载荷数据
        /// </summary>
        [DataMember(Name = "payload")]
        public byte[] Payload { get; set; }
    }
}
