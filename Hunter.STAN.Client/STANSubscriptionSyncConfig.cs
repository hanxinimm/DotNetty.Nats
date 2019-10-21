using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Hunter.STAN.Client
{
    public class STANSubscriptionSyncConfig : STANSubscriptionConfig
    {
        public STANSubscriptionSyncConfig(string subject,string inbox,string ackInbox, Func<STANMsgContent, bool> handler)
        {
            Subject = subject;
            Inbox = inbox;
            AckInbox = ackInbox;
            Handler = handler;
        }
        public STANSubscriptionSyncConfig(string subject, string inbox, string ackInbox, Action<STANMsgContent> handler)
        {
            IsAutoAck = true;
            Subject = subject;
            Inbox = inbox;
            AckInbox = ackInbox;
            AutoAckHandler = handler;
        }


        public STANSubscriptionSyncConfig(string subject, string inbox, string ackInbox, int maxMsg, Func<STANMsgContent, bool> handler)
        {
            Subject = subject;
            Inbox = inbox;
            AckInbox = ackInbox;
            Handler = handler;
            MaxMsg = maxMsg;
        }
        public STANSubscriptionSyncConfig(string subject, string inbox, string ackInbox, int maxMsg, Action<STANMsgContent> handler)
        {
            IsAutoAck = true;
            Subject = subject;
            Inbox = inbox;
            AckInbox = ackInbox;
            AutoAckHandler = handler;
            MaxMsg = maxMsg;
        }


        public STANSubscriptionSyncConfig(string subject, string inbox, string ackInbox, int maxMsg, string durableName, Func<STANMsgContent, bool> handler)
        {
            Subject = subject;
            Inbox = inbox;
            AckInbox = ackInbox;
            Handler = handler;
            MaxMsg = maxMsg;
            DurableName = durableName;
        }
        public STANSubscriptionSyncConfig(string subject, string inbox, string ackInbox, int maxMsg, string durableName, Action<STANMsgContent> handler)
        {
            IsAutoAck = true;
            Subject = subject;
            Inbox = inbox;
            AckInbox = ackInbox;
            AutoAckHandler = handler;
            MaxMsg = maxMsg;
            DurableName = durableName;
        }

        /// <summary>
        /// 自动确认消息处理程序
        /// </summary>
        public Action<STANMsgContent> AutoAckHandler { get; }

        /// <summary>
        /// 处理程序
        /// </summary>
        public Func<STANMsgContent, bool> Handler { get; }

    }
}
