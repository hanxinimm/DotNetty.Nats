using DotNetty.Codecs.NATSJetStream.Protocol;
using System;
using System.Collections.Generic;
using System.Text;

namespace Hunter.NATS.Client
{
    public class NATSConsumerSubscriptionConfig
    {
        public NATSConsumerSubscriptionConfig() { }

        public NATSConsumerSubscriptionConfig(string streamName, ConsumerConfig config, string subscribeId)
        {
            StreamName = streamName;
            Config = config;
            SubscribeId = subscribeId;
        }

        public string StreamName { get; protected set; }

        /// <summary>
        /// 客户端配置
        /// </summary>
        public ConsumerConfig Config { get; protected set; }

        /// <summary>
        /// 订阅编号
        /// </summary>
        public string SubscribeId { get; protected set; }

        ///// <summary>
        ///// 是否为异步回调
        ///// </summary>
        public bool IsAsyncHandler { get; protected set; }

    }
}
