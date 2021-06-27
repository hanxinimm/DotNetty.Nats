using System.Runtime.Serialization;

namespace DotNetty.Codecs.NATSJetStream.Packets
{
    [DataContract]
    public abstract class MessagePacket<TMessage> : MessagePacket
    {
        /// <summary>
        /// 消息
        /// </summary>
        [DataMember(Name = "message")]
        public TMessage Message { get; set; }
    }

    [DataContract]
    public abstract class MessagePacket : NATSJetStreamPacket
    {
        // <summary>
        /// 主题
        /// </summary>
        [DataMember(Name = "subject")]
        public string Subject { get; set; }

        // <summary>
        /// 主题回复标识
        /// </summary>
        [DataMember(Name = "replyTo")]
        public string ReplyTo { get; set; }

        /// <summary>
        /// 以字节为单位的有效载荷大小
        /// </summary>
        [IgnoreDataMember]
        public int PayloadSize { get; set; }

    }
}
