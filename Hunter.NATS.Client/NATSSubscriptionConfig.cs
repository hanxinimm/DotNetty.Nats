using System;
using System.Collections.Generic;
using System.Text;

namespace Hunter.NATS.Client
{
    public class NATSSubscriptionConfig
    {
        public NATSSubscriptionConfig() { }

        public NATSSubscriptionConfig(string subject, string subscribeId)
        {
            Subject = subject;
            SubscribeId = subscribeId;
        }

        /// <summary>
        /// 主题
        /// </summary>
        public string Subject { get; protected set; }
        /// <summary>
        /// 订阅编号
        /// </summary>
        public string SubscribeId { get; protected set; }

        /// <summary>
        /// 等待处理的最大消息数
        /// </summary>
        public int? MaxMsg { get; set; }

        ///// <summary>
        ///// 是否为异步回调
        ///// </summary>
        public bool IsAsyncHandler { get; protected set; }

    }
}
