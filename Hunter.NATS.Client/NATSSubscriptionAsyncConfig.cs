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
        }

        public NATSSubscriptionAsyncConfig(string subject, string subscribeId, int maxMsg, Func<NATSMsgContent, ValueTask> handler)
        {
            IsAsyncHandler = true;
            Subject = subject;
            SubscribeId = subscribeId;
            MaxMsg = maxMsg;
        }

        /// <summary>
        /// 处理程序
        /// </summary>
        public Func<NATSMsgContent, ValueTask> AsyncHandler { get; }

    }
}
