using Google.Protobuf;
using System;
using System.Collections.Generic;
using System.Text;

namespace Hunter.STAN.Client
{
    public class STANConnectionConfig
    {
        public STANConnectionConfig(
            string connectionId,
            string error,
            string pubPrefix,
            string subRequests,
            string unsubRequests,
            string closeRequests,
            string subCloseRequests,
            string pingRequests,
            int pingInterval,
            int pingMaxOut,
            int protocol,
            string publicKey)
        {
            ConnectionId = ByteString.CopyFrom(Encoding.UTF8.GetBytes(connectionId));
            Error = error;
            PubPrefix = pubPrefix;
            SubRequests = subRequests;
            UnsubRequests = unsubRequests;
            CloseRequests = closeRequests;
            SubCloseRequests = subCloseRequests;
            PingRequests = pingRequests;
            PingInterval = pingInterval;
            PingMaxOut = pingMaxOut;
            Protocol = protocol;
            PublicKey = publicKey;
        }

        /// <summary>
        /// 连接编号
        /// </summary>
        public readonly ByteString ConnectionId;

        public readonly string Error;

        /// <summary>
        /// 发布时使用的前缀
        /// </summary>
        public readonly string PubPrefix;
        /// <summary>
        /// 用于订阅请求的主题
        /// </summary>
        public readonly string SubRequests;
        /// <summary>
        /// 用于取消订阅请求的主题
        /// </summary>
        public readonly string UnsubRequests;
        /// <summary>
        /// 关闭连接的主题
        /// </summary>
        public readonly string CloseRequests;
        /// <summary>
        /// 关闭连接的主题
        /// </summary>
        public readonly string SubCloseRequests;

        /// <summary>
        /// 适用于PING请求   
        /// </summary>
        public readonly string PingRequests;

        /// <summary>
        /// 客户端发送PING的时间间隔(以秒为单位)
        /// </summary>
        public readonly int PingInterval;

        /// <summary>
        /// 没有响应的PING的最大数量，之后可以认为连接丢失
        /// </summary>
        public readonly int PingMaxOut;

        /// <summary>
        /// 服务器所在的协议版本
        /// </summary>
        public readonly int Protocol;

        /// <summary>
        /// 保留供将来使用
        /// </summary>
        public readonly string PublicKey;
    }
}
