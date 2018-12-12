using DotNetty.Buffers;
using DotNetty.Common;
using Google.Protobuf;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace DotNetty.Codecs.STAN.Packets
{
    [DataContract]
    public abstract class MessagePacket<TMessage> : MessagePacket
        where TMessage : IMessage
    {
        /// <summary>
        /// 消息
        /// </summary>
        [DataMember(Name = "message")]
        public TMessage Message { get; set; }
    }

    [DataContract]
    public abstract class MessagePacket : STANPacket
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

        [IgnoreDataMember]
        public override STANPacketType PacketType => STANPacketType.MSG;

        /// <summary>
        /// 以字节为单位的有效载荷大小
        /// </summary>
        [IgnoreDataMember]
        public int PayloadSize { get; set; }

    }
}
