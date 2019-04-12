using System;
using System.Collections.Generic;
using System.Text;

namespace Hunter.NATS.Client
{
    public class NATSOptions
    {
        public NATSOptions()
        {
            ClusterNodes = new List<string>();
        }

        /// <summary>
        /// 集群编号
        /// </summary>
        public string ClusterID { get; set; }

        /// <summary>
        /// 客户端编号
        /// </summary>
        public string ClientId { get; set; }

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
