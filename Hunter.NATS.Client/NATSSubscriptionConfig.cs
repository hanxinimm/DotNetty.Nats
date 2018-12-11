using System;
using System.Collections.Generic;
using System.Text;

namespace Hunter.NATS.Client
{
    public class NATSSubscriptionConfig
    {
        public NATSSubscriptionConfig(string subject, string subscribeId, Action<byte[]> handler)
        {
            Subject = subject;
            SubscribeId = subscribeId;
            Handler = handler;
        }

        /// <summary>
        /// 主题
        /// </summary>
        public string Subject { get; }
        /// <summary>
        /// 订阅编号
        /// </summary>
        public string SubscribeId { get; }

        /// <summary>
        /// 等待处理的最大消息数
        /// </summary>
        public int? MaxProcessed { get; set; }
        /// <summary>
        /// 处理程序
        /// </summary>
        public Action<byte[]> Handler { get; }

    }
}
