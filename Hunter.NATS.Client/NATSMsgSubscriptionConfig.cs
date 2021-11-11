using System;
using System.Collections.Generic;
using System.Text;

namespace Hunter.NATS.Client
{
    public class NATSMsgSubscriptionConfig : NATSSubscriptionConfig
    {
        public NATSMsgSubscriptionConfig() { }

        public NATSMsgSubscriptionConfig(string subject, string subscribeId)
        {
            Subject = subject;
            SubscribeId = subscribeId;
        }

        public NATSMsgSubscriptionConfig(string subject, string subscribeId, int? maxMsg)
        {
            Subject = subject;
            SubscribeId = subscribeId;
            MaxMsg = maxMsg;
        }

        public NATSMsgSubscriptionConfig(string subject, string subscribeId, string subscribeGroup)
        {
            Subject = subject;
            SubscribeId = subscribeId;
            SubscribeGroup = subscribeGroup;
        }

        public NATSMsgSubscriptionConfig(string subject, string subscribeId, string subscribeGroup, int? maxMsg)
        {
            Subject = subject;
            SubscribeId = subscribeId;
            SubscribeGroup = subscribeGroup;
            MaxMsg = maxMsg;
        }

        /// <summary>
        /// 主题
        /// </summary>
        public string Subject { get; protected set; }

        /// <summary>
        /// 订阅组
        /// </summary>
        public string SubscribeGroup { get; protected set; }

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
