using System;
using System.Collections.Generic;
using System.Text;

namespace Hunter.NATS.Client
{
    public class NATSMsgContent
    {
        public NATSMsgContent() { }

        public NATSMsgContent(string subscribeId, string subject, string replyTo, byte[] data)
        {
            SubscribeId = subscribeId;
            Subject = subject;
            ReplyTo = replyTo;
            Data = data;
        }

        /// <summary>
        /// 订阅编号
        /// </summary>
        public string SubscribeId { get; set; }

        /// <summary>
        /// 主题
        /// </summary>
        public string Subject { get; set; }

        /// <summary>
        /// 消息回复的收件箱
        /// </summary>
        public string ReplyTo { get; set; }

        /// <summary>
        /// 消息内容
        /// </summary>
        public byte[] Data { get; set; } = Array.Empty<byte>();
    }
}
