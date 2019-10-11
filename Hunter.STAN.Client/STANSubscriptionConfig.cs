﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Hunter.STAN.Client
{
    public class STANSubscriptionConfig
    {
        public STANSubscriptionConfig(string subject,string inbox,string ackInbox, Func<STANMsgContent, bool> handler)
        {
            Subject = subject;
            Inbox = inbox;
            AckInbox = ackInbox;
            Handler = handler;
        }

        public STANSubscriptionConfig(string subject, string inbox, string ackInbox, Action<STANMsgContent> handler)
        {
            IsAutoAck = true;
            Subject = subject;
            Inbox = inbox;
            AckInbox = ackInbox;
            AutoAckHandler = handler;
        }

        public STANSubscriptionConfig(string subject, string inbox, string ackInbox, Func<STANMsgContent, ValueTask<bool>> handler)
        {
            IsAsyncCallback = true;
            Subject = subject;
            Inbox = inbox;
            AckInbox = ackInbox;
            AsyncHandler = handler;
        }

        public STANSubscriptionConfig(string subject, string inbox, string ackInbox, Func<STANMsgContent, Task> handler)
        {
            IsAsyncCallback = true;
            IsAutoAck = true;
            Subject = subject;
            Inbox = inbox;
            AckInbox = ackInbox;
            AutoAckAsyncHandler = handler;
        }


        public STANSubscriptionConfig(string subject, string inbox, string ackInbox, int maxMsg, string durableName, Func<STANMsgContent, ValueTask<bool>> handler)
        {
            IsAsyncCallback = true;
            Subject = subject;
            Inbox = inbox;
            AckInbox = ackInbox;
            AsyncHandler = handler;
            MaxMsg = maxMsg;
            DurableName = durableName;
        }

        public STANSubscriptionConfig(string subject, string inbox, string ackInbox, int maxMsg, string durableName, Func<STANMsgContent, Task> handler)
        {
            IsAsyncCallback = true;
            IsAutoAck = true;
            Subject = subject;
            Inbox = inbox;
            AckInbox = ackInbox;
            AutoAckAsyncHandler = handler;
            MaxMsg = maxMsg;
            DurableName = durableName;
        }

        public STANSubscriptionConfig(string subject, string inbox, string ackInbox, int maxMsg, string durableName, Func<STANMsgContent, bool> handler)
        {
            Subject = subject;
            Inbox = inbox;
            AckInbox = ackInbox;
            Handler = handler;
            MaxMsg = maxMsg;
            DurableName = durableName;
        }

        public STANSubscriptionConfig(string subject, string inbox, string ackInbox, int maxMsg, string durableName, Action<STANMsgContent> handler)
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
        /// 等待处理的最大消息数
        /// </summary>
        public int? MaxMsg { get; set; }

        /// <summary>
        /// 持久化名称
        /// </summary>
        public string DurableName { get; set; }

        /// <summary>
        /// 是否为自动确认
        /// </summary>
        public bool IsAutoAck { get; set; }

        /// <summary>
        /// 是否为异步回调
        /// </summary>
        public bool IsAsyncCallback { get; set; }

        /// <summary>
        /// 自动确认消息处理程序
        /// </summary>
        public Action<STANMsgContent> AutoAckHandler { get; }

        /// <summary>
        /// 处理程序
        /// </summary>
        public Func<STANMsgContent, bool> Handler { get; }

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
