using System;
using System.Collections.Generic;
using System.Text;

namespace Hunter.NATS.Client
{
    public class NATSSubscriptionConfig
    {
        public NATSSubscriptionConfig() { }

        public NATSSubscriptionConfig(string subscribeId)
        {
            SubscribeId = subscribeId;
        }

        /// <summary>
        /// 订阅编号
        /// </summary>
        public string SubscribeId { get; protected set; }

    }
}
