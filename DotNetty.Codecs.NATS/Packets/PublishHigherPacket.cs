﻿using DotNetty.Buffers;
using DotNetty.Common;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace DotNetty.Codecs.NATS.Packets
{
    [DataContract]
    public class PublishHigherPacket : NATSPacket
    {
        [IgnoreDataMember]
        public override NATSPacketType PacketType => NATSPacketType.HPUB;

        public PublishHigherPacket() { }

        public PublishHigherPacket(string subject)
        {
            Subject = subject;
        }

        public PublishHigherPacket(string subject, byte[] payload)
        {
            Subject = subject;
            Payload = payload;
        }

        public PublishHigherPacket(string subject, byte[] payload, Dictionary<string, string> headers)
        {
            Subject = subject;
            Payload = payload;
            Headers = headers;
        }

        public PublishHigherPacket(string subject, string replyTo, byte[] payload)
        {
            Subject = subject;
            ReplyTo = replyTo;
            Payload = payload;
        }

        public PublishHigherPacket(string subject, string replyTo, byte[] payload, Dictionary<string, string> headers)
        {
            Subject = subject;
            ReplyTo = replyTo;
            Payload = payload;
            Headers = headers;
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

        /// <summary>
        /// 头部
        /// </summary>
        [DataMember(Name = "headers")]
        public Dictionary<string,string> Headers { get; set; }
    }
}
