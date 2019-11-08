using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Hunter.NATS.Client
{
    public class NATSSubscriptionAsyncConfig : NATSSubscriptionConfig
    {
        public NATSSubscriptionAsyncConfig(string subject, string subscribeId, Func<NATSMsgContent, ValueTask> handler)
        {
            IsAsyncHandler = true;
            Subject = subject;
            SubscribeId = subscribeId;
            AsyncHandler = handler;
        }

        public NATSSubscriptionAsyncConfig(string subject, string subscribeId, int maxMsg, Func<NATSMsgContent, ValueTask> handler)
        {
            IsAsyncHandler = true;
            Subject = subject;
            SubscribeId = subscribeId;
            MaxMsg = maxMsg;
            AsyncHandler = handler;
        }

        public NATSSubscriptionAsyncConfig(string subject, string subscribeId, Action<NATSMsgContent> handler)
        {
            IsAsyncHandler = false;
            Subject = subject;
            SubscribeId = subscribeId;
            Handler = handler;
        }

        public NATSSubscriptionAsyncConfig(string subject, string subscribeId, int maxMsg, Action<NATSMsgContent> handler)
        {
            IsAsyncHandler = false;
            Subject = subject;
            SubscribeId = subscribeId;
            MaxMsg = maxMsg;
            Handler = handler;
        }

        /// <summary>
        /// 处理程序
        /// </summary>
        public Func<NATSMsgContent, ValueTask> AsyncHandler { get; }

        /// <summary>
        /// 处理程序
        /// </summary>
        public Action<NATSMsgContent> Handler { get; }

    }
}
