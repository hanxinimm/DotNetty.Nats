using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Hunter.STAN.Client
{
    public class STANSubscriptionAsyncConfig : STANSubscriptionConfig
    {
        public STANSubscriptionAsyncConfig(string subject, string inbox, string ackInbox, Func<STANMsgContent, ValueTask<bool>> handler)
        {
            IsAsyncHandler = true;
            Subject = subject;
            Inbox = inbox;
            AckInbox = ackInbox;
            AsyncHandler = handler;
        }
        public STANSubscriptionAsyncConfig(string subject, string inbox, string ackInbox, Func<STANMsgContent, Task> handler)
        {
            IsAsyncHandler = true;
            IsAutoAck = true;
            Subject = subject;
            Inbox = inbox;
            AckInbox = ackInbox;
            AutoAckAsyncHandler = handler;
        }


        public STANSubscriptionAsyncConfig(string subject, string inbox, string ackInbox, int maxMsg, Func<STANMsgContent, ValueTask<bool>> handler)
        {
            IsAsyncHandler = true;
            Subject = subject;
            Inbox = inbox;
            AckInbox = ackInbox;
            AsyncHandler = handler;
            MaxMsg = maxMsg;
        }
        public STANSubscriptionAsyncConfig(string subject, string inbox, string ackInbox, int maxMsg, Func<STANMsgContent, Task> handler)
        {
            IsAsyncHandler = true;
            IsAutoAck = true;
            Subject = subject;
            Inbox = inbox;
            AckInbox = ackInbox;
            AutoAckAsyncHandler = handler;
            MaxMsg = maxMsg;
        }


        public STANSubscriptionAsyncConfig(string subject, string inbox, string ackInbox, int maxMsg, string durableName, Func<STANMsgContent, ValueTask<bool>> handler)
        {
            IsAsyncHandler = true;
            Subject = subject;
            Inbox = inbox;
            AckInbox = ackInbox;
            AsyncHandler = handler;
            MaxMsg = maxMsg;
            DurableName = durableName;
        }
        public STANSubscriptionAsyncConfig(string subject, string inbox, string ackInbox, int maxMsg, string durableName, Func<STANMsgContent, Task> handler)
        {
            IsAsyncHandler = true;
            IsAutoAck = true;
            Subject = subject;
            Inbox = inbox;
            AckInbox = ackInbox;
            AutoAckAsyncHandler = handler;
            MaxMsg = maxMsg;
            DurableName = durableName;
        }

        /// <summary>
        /// 自动确认消息处理程序
        /// </summary>
        public Func<STANMsgContent,Task> AutoAckAsyncHandler { get; }

        /// <summary>
        /// 处理程序
        /// </summary>
        public Func<STANMsgContent, ValueTask<bool>> AsyncHandler { get; }

    }
}
