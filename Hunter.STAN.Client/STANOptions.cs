using System;
using System.Collections.Generic;
using System.Text;

namespace Hunter.STAN.Client
{
    public class STANOptions
    {
        public STANOptions()
        {
            ClusterNodes = new List<string>();
        }

        /// <summary>
        /// 链接超时时间
        /// </summary>
        public TimeSpan ConnectTimeout { get; set; }

        /// <summary>
        /// 消息确认超时时间
        /// </summary>
        public TimeSpan AckTimeout { get; set; }

        /// <summary>
        /// 发布消息确认的超时时间
        /// </summary>
        public TimeSpan PubAckTimeout { get; set; }

        /// <summary>
        /// 允许的最大未确认的发布消息数量
        /// </summary>
        public int MaxPubAckInFlight { get; set; }

        /// <summary>
        /// 集群节点地址
        /// </summary>
        public List<string> ClusterNodes { get; set; }
    }
}
