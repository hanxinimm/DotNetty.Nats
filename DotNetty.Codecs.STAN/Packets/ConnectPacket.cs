using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;

namespace DotNetty.Codecs.STAN.Packets
{
    [DataContract]
    public class ConnectPacket : Packet
    {
        [IgnoreDataMember]
        public override PacketType PacketType => PacketType.CONNECT;

        /// <summary>
        /// TODO:写成包内可以访问
        /// </summary>
        /// <param name="isVerbose"></param>
        /// <param name="pedantic"></param>
        /// <param name="isSSLRequired"></param>
        /// <param name="user"></param>
        /// <param name="password"></param>
        /// <param name="clientName"></param>
        /// <param name="token"></param>
        public ConnectPacket(bool isVerbose, bool pedantic, bool isSSLRequired, string user, string password, string clientName, string token)
        {
            IsVerbose = isVerbose;
            IsPedantic = pedantic;
            IsSSLRequired = isSSLRequired;
            User = user;
            Password = password;
            ClientName = clientName;
            AuthToken = token;
        }

        /// <summary>
        /// 打开+OK协议确认
        /// </summary>
        [DataMember(Name = "verbose")]
        public bool IsVerbose { get; set; }

        /// <summary>
        /// 开启额外的严格格式检查，例如对于正确形成的主题
        /// </summary>
        [DataMember(Name = "pedantic")]
        public bool IsPedantic { get; set; }

        /// <summary>
        /// 指示客户端是否需要SSL连接。
        /// </summary>
        [DataMember(Name = "ssl_required")]
        public bool IsSSLRequired { get; set; }

        /// <summary>
        /// 客户端授权令牌
        /// </summary>
        [DataMember(Name = "auth_token")]
        public string AuthToken { get; set; }
        
        /// <summary>
        /// 连接用户名（如果auth_required已设置）
        /// </summary>
        [DataMember(Name = "user")]
        public string User { get; set; }

        /// <summary>
        /// 连接密码（如果auth_required已设置）
        /// </summary>
        [DataMember(Name = "pass")]
        public string Password { get; set; }

        /// <summary>
        /// 可选的客户名称
        /// </summary>
        [DataMember(Name = "name")]
        public string ClientName { get; set; }

        /// <summary>
        /// 客户端的实现语言。
        /// </summary>
        [DataMember(Name = "lang")]
        public string lang { get; set; }

        /// <summary>
        /// 客户端的版本。
        /// </summary>
        [DataMember(Name = "version")]
        public string Version { get; set; }

        /// <summary>
        /// 可选的int。发送0（或缺省）表示客户端支持原始协议。发送1指示客户端支持动态重新配置集群拓扑更改，通过异步接收INFO已重新连接的已知服务器的消息。
        /// </summary>
        [DataMember(Name = "protocol")]
        public int Protocol { get; set; }

        internal string ToJson()
        {
            if (IsSSLRequired)
                return $"{{\"verbose\":{(IsVerbose ? "true":"false")},\"pedantic\":{(IsPedantic ? "true" : "false")},\"tls_required\":true,\"user\":\"{User}\",\"pass\":\"{Password}\",\"name\":\"{ClientName}\",\"lang\":\".net_core\",\"version\":\"{Version}\",\"protocol\":1}}";
            else
                return $"{{\"verbose\":{(IsVerbose ? "true" : "false")},\"pedantic\":{(IsPedantic ? "true" : "false")},\"tls_required\":false,\"name\":\"{ClientName}\",\"lang\":\".net_core\",\"version\":\"{Version}\",\"protocol\":1}}";
        }
    }
}
