using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Hunter.STAN.Client
{
    public class STANSubscriptionConfig
    {
        public STANSubscriptionConfig() { }

        public STANSubscriptionConfig(string subject, string inbox, string ackInbox)
        {
            Subject = subject;
            Inbox = inbox;
            AckInbox = ackInbox;
        }

        public STANSubscriptionConfig(string subject, string inbox, string ackInbox, int maxMsg)
        {
            Subject = subject;
            Inbox = inbox;
            AckInbox = ackInbox;
            MaxMsg = maxMsg;
        }

        /// <summary>
        /// 主题
        /// </summary>
        public string Subject { get; protected set; }
        /// <summary>
        /// 订阅消息收件箱
        /// </summary>
        public string Inbox { get; protected set; }
        /// <summary>
        /// 消息确认处理收件箱
        /// </summary>
        public string AckInbox { get; protected set; }

        /// <summary>
        /// 等待处理的最大消息数
        /// </summary>
        public int? MaxMsg { get; protected set; }

        /// <summary>
        /// 持久化名称
        /// </summary>
        public string DurableName { get; protected set; }

        /// <summary>
        /// 是否为自动确认
        /// </summary>
        public bool IsAutoAck { get; protected set; }

        ///// <summary>
        ///// 是否为异步回调
        ///// </summary>
        public bool IsAsyncHandler { get; protected set; }

    }
}
