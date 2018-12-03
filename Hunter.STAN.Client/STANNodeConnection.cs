using DotNetty.Transport.Channels;
using System;
using System.Collections.Generic;
using System.Text;

namespace Hunter.STAN.Client
{
    public class STANNodeConnection
    {
        public STANNodeConnection(string clusterID, string clientID, string heartbeatInbox, string replyInbox, STANConnectionConfig config, IChannel channel)
        {
            ClusterID = clusterID;
            ClientID = clientID;
            HeartbeatInbox = heartbeatInbox;
            ReplyInbox = replyInbox;
            Config = config;
            Channel = channel;
        }

        /// <summary>
        /// 集群编号
        /// </summary>
        public readonly string ClusterID;

        /// <summary>
        /// 客户端编号
        /// </summary>
        public readonly string ClientID;

        /// <summary>
        /// 心跳收件箱
        /// </summary>
        public readonly string HeartbeatInbox;

        /// <summary>
        /// 消息应答收件箱
        /// </summary>
        public readonly string ReplyInbox;

        /// <summary>
        /// 连接配置信息
        /// </summary>
        public readonly STANConnectionConfig Config;

        /// <summary>
        /// 通讯通道
        /// </summary>
        protected readonly IChannel Channel;
    }
}
