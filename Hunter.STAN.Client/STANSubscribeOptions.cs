using DotNetty.Codecs.STAN.Protocol;
using System;
using System.Collections.Generic;
using System.Text;

namespace Hunter.STAN.Client
{
    public class STANSubscribeOptions
    {
        /// <summary>
        /// 允许未确认的最大飞行中的消息
        /// </summary>
        public int? MaxInFlight { get; set; }
        /// <summary>
        /// 从客户端收到确认的超时
        /// </summary>
        public int? AckWaitInSecs { get; set; }
        /// <summary>
        /// 枚举类型，指定历史记录中开始重播数据的点
        /// </summary>
        public StartPosition Position { get; set; }
        /// <summary>
        /// 从StartSequence字段中的顺序开始发送消息
        /// </summary>
        public ulong? StartSequence { get; set; }
        /// <summary>
        /// 从StartTimeDelta字段中指定的持续时间发送消息。
        /// </summary>
        public long? StartTimeDelta { get; set; }
    }
}
