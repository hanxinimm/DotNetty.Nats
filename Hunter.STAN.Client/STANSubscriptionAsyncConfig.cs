using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Hunter.STAN.Client
{
    public class STANSubscriptionAsyncConfig
    {
        public STANSubscriptionAsyncConfig(string subject, string inbox, string ackInbox, Func<STANMsgContent, ValueTask<bool>> handler)
        {
            Subject = subject;
            Inbox = inbox;
            AckInbox = ackInbox;
            Handler = handler;
        }

        public STANSubscriptionAsyncConfig(string subject, string inbox, string ackInbox, int maxMsg,string durableName, Func<STANMsgContent, ValueTask<bool>> handler)
        {
            Subject = subject;
            Inbox = inbox;
            AckInbox = ackInbox;
            MaxMsg = maxMsg;
            DurableName = durableName;
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
        /// 处理的最大消息数量
        /// </summary>
        public int? MaxMsg { get; set; }

        /// <summary>
        /// 持久化名称
        /// </summary>
        public string DurableName { get; set; }

        /// <summary>
        /// 处理程序
        /// </summary>
        public Func<STANMsgContent, ValueTask<bool>> Handler { get; }

    }
}
