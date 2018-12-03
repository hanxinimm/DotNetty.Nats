using System;
using System.Collections.Generic;
using System.Text;

namespace Hunter.STAN.Client
{
    public class STANConnectionConfig
    {
        public STANConnectionConfig(string pubPrefix, string subRequests, string unsubRequests, string closeRequests, string subCloseRequests, string publicKey)
        {
            PubPrefix = pubPrefix;
            SubRequests = subRequests;
            UnsubRequests = unsubRequests;
            CloseRequests = closeRequests;
            SubCloseRequests = subCloseRequests;
            PublicKey = publicKey;
        }

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
        /// 保留供将来使用
        /// </summary>
        public readonly string PublicKey;
    }
}
