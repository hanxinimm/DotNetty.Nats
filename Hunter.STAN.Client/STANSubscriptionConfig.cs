using System;
using System.Collections.Generic;
using System.Text;

namespace Hunter.STAN.Client
{
    public class STANSubscriptionConfig
    {
        public STANSubscriptionConfig(string subject,string inbox,string ackInbox,Action<byte[]> handler)
        {
            Subject = subject;
            Inbox = inbox;
            AckInbox = ackInbox;
            Handler = handler;
        }

        /// <summary>
        /// 主题
        /// </summary>
        public string Subject { get; }
        /// <summary>
        /// 订阅消息收件箱
        /// </summary>
        public string Inbox { get; }
        /// <summary>
        /// 消息确认处理收件箱
        /// </summary>
        public string AckInbox { get; }
        /// <summary>
        /// 处理程序
        /// </summary>
        public Action<byte[]> Handler { get; }

    }
}
