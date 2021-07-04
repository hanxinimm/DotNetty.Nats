using System;
using System.Collections.Generic;
using System.Text;

namespace Hunter.NATS.Client
{
    public class NATSConsumerSubscriptionConfig
    {
        public NATSConsumerSubscriptionConfig() { }

        public NATSConsumerSubscriptionConfig(string consumerName, string subscribeId)
        {
            ConsumerName = consumerName;
            SubscribeId = subscribeId;
        }

        public NATSConsumerSubscriptionConfig(string consumerName, string subscribeId, string subscribeGroup)
        {
            ConsumerName = consumerName;
            SubscribeId = subscribeId;
            SubscribeGroup = subscribeGroup;
        }

        /// <summary>
        /// 主题
        /// </summary>
        public string ConsumerName { get; protected set; }

        /// <summary>
        /// 订阅编号
        /// </summary>
        public string SubscribeId { get; protected set; }

        /// <summary>
        /// 订阅组
        /// </summary>
        public string SubscribeGroup { get; protected set; }

        ///// <summary>
        ///// 是否为异步回调
        ///// </summary>
        public bool IsAsyncHandler { get; protected set; }

    }
}
