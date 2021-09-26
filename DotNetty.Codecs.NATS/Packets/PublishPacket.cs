using System;
using System.Runtime.Serialization;

namespace DotNetty.Codecs.NATS.Packets
{
    [DataContract]
    public class PublishPacket : NATSPacket
    {
        [IgnoreDataMember]
        public override NATSPacketType PacketType => NATSPacketType.PUB;

        public PublishPacket() { }

        public PublishPacket(string subject)
        {
            Subject = subject;
        }

        public PublishPacket(string subject, byte[] payload)
        {
            Subject = subject;
            Payload = payload;
        }

        public PublishPacket(string subject, string replyTo, byte[] payload)
        {
            Subject = subject;
            ReplyTo = replyTo;
            Payload = payload;
        }

        /// <summary>
        /// 要发布到的目标主题
        /// </summary>
        [DataMember(Name = "subject")]
        public string Subject { get; set; }

        /// <summary>
        /// 回复收件箱的主题是订阅者可以用来将回复发送回发布者/请求者
        /// </summary>
        [DataMember(Name = "reply-to")]
        public string ReplyTo { get; set; }

        /// <summary>
        /// 以字节为单位的有效载荷大小
        /// </summary>
        [IgnoreDataMember]
        public int PayloadLength { get { return this.Payload?.Length ?? 0; } }

        /// <summary>
        /// 消息有效载荷数据
        /// </summary>
        [DataMember(Name = "payload")]
        public byte[] Payload { get; set; }
    }
}
