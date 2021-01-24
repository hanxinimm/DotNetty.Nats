using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Hunter.STAN.Client
{
    public class STANSubscriptionConfig
    {
        public STANSubscriptionConfig() { }

        public STANSubscriptionConfig(string subject, string replyTo, string inbox)
        {
            Subject = subject;
            ReplyTo = replyTo;
            Inbox = inbox;
        }

        public STANSubscriptionConfig(string subject, string replyTo, string inbox, int maxMsg)
        {
            Subject = subject;
            ReplyTo = replyTo;
            Inbox = inbox;
            MaxMsg = maxMsg;
        }

        /// <summary>
        /// 是否取消订阅
        /// </summary>
        public bool IsUnSubscribe { get; set; }

        /// <summary>
        /// 主题
        /// </summary>
        public string Subject { get; protected set; }

        /// <summary>
        /// 持久化名称
        /// </summary>
        public string DurableName { get; protected set; }

        /// <summary>
        /// 订阅消息收件箱
        /// </summary>
        public string Inbox { get; protected set; }

        /// <summary>
        /// 等待处理的最大消息数
        /// </summary>
        public int? MaxMsg { get; protected set; }

        /// <summary>
        /// 订阅响应答复主题
        /// </summary>
        public string ReplyTo { get; protected set; }

        /// <summary>
        /// 消息确认收件箱
        /// </summary>
        public string AckInbox { get; set; }
    }
}
