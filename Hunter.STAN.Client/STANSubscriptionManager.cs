using DotNetty.Codecs.STAN.Packets;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Hunter.STAN.Client
{
    public class STANSubscriptionManager
    {
        public STANSubscriptionManager()
        {
            MessageQueues = new ConcurrentQueue<MsgProtoPacket>();
            QueueEventWaitHandle = new ManualResetEvent(false);
        }

        public STANSubscriptionManager(EventResetMode mode)
        {
            MessageQueues = new ConcurrentQueue<MsgProtoPacket>();
            QueueEventWaitHandle = new EventWaitHandle(false, mode);
        }

        /// <summary>
        /// 消息队列
        /// </summary>
        public ConcurrentQueue<MsgProtoPacket> MessageQueues { get; }

        /// <summary>
        /// 队列配置
        /// </summary>
        public STANSubscriptionAsyncConfig SubscriptionConfig { get; set; }

        /// <summary>
        /// 消息通知信号
        /// </summary>
        public EventWaitHandle QueueEventWaitHandle { get; } 
    }
}
