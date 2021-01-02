using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace Hunter.STAN.Client
{
    public class STANOptions
    {
        public STANOptions()
        {
            ClusterNodes = new List<EndPoint>();
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
        /// 集群节点地址
        /// </summary>
        public List<EndPoint> ClusterNodes { get; set; }

        /// <summary>
        /// 集群主机地址
        /// </summary>
        public string Host { get; set; }

        /// <summary>
        /// 通讯端口
        /// </summary>
        public int Port { get; set; }

        /// <summary>
        /// HTTP端口
        /// </summary>
        public int HttpPort { get; set; }

        /// <summary>
        /// 节点端口
        /// </summary>
        public int ClusterPort { get; set; }

        /// <summary>
        /// 是否需要认证
        /// </summary>
        public bool IsAuthentication { get; set; }

        /// <summary>
        /// 用户名
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// 密码
        /// </summary>
        public string Password { get; set; }

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
        /// 消息发布确认回调
        /// </summary>
        public Action<STANMsgPubAck> PubAckCallback { get; set; }

        /// <summary>
        /// 日记记录
        /// </summary>
        public ILogger Logger { get; set; }

    }
}
