using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;

namespace DotNetty.Codecs.NATS.Packets
{
    [DataContract]
    public class ConnectPacket : NATSPacket
    {
        [IgnoreDataMember]
        public override NATSPacketType PacketType => NATSPacketType.CONNECT;

        /// <summary>
        /// 需要验证的
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
        /// 无需验证的
        /// </summary>
        /// <param name="isVerbose"></param>
        /// <param name="pedantic"></param>
        /// <param name="isSSLRequired"></param>
        /// <param name="user"></param>
        /// <param name="password"></param>
        /// <param name="clientName"></param>
        /// <param name="token"></param>
        public ConnectPacket(bool isVerbose, bool pedantic, bool isSSLRequired, string clientName)
        {
            IsVerbose = isVerbose;
            IsPedantic = pedantic;
            IsSSLRequired = isSSLRequired;
            ClientName = clientName;
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
        public string Lang { get; set; } = ".NET";

        /// <summary>
        /// 客户端的版本。
        /// </summary>
        [DataMember(Name = "version")]
        public string Version { get; set; }

        /// <summary>
        /// 可选的int。发送0（或缺省）表示客户端支持原始协议。发送1指示客户端支持动态重新配置集群拓扑更改，通过异步接收INFO已重新连接的已知服务器的消息。
        /// </summary>
        [DataMember(Name = "protocol")]
        public int Protocol { get; set; } = 1;

        /// <summary>
        /// 识别用户权限和帐户的 JWT
        /// </summary>
        [DataMember(Name = "jwt")]
        public string JwtToken { get; set; }

        /// <summary>
        /// NKeys 是基于Ed25519的全新、高度安全的公钥签名系统
        /// </summary>
        /// <remarks>
        /// https://docs.nats.io/nats-server/configuration/securing_nats/auth_intro/nkey_auth
        /// </remarks>
        [DataMember(Name = "nkey")]
        public string NKey { get; set; }


        /// <summary>
        /// 如果服务器在 INFO 上响应 nonce，则 NATS 客户端必须使用 nonce 签名来回复
        /// </summary>
        [DataMember(Name = "sig")]
        public string Sign { get; set; }

        /// <summary>
        /// 可选的布尔。
        /// </summary>
        /// <remarks>
        /// 如果设置为真实，服务器（版本 1.2.0+）将不会将来自此连接的源消息发送到自己的订阅。客户端应仅针对支持此功能的服务器将此设置为真实，当INFO协议中的原型设置为至少1时
        /// </remarks>
        [DataMember(Name = "echo")]
        public bool ECHO { get; set; }

        /// <summary>
        /// 是否启用消息头
        /// </summary>
        [DataMember(Name = "headers")]
        public bool Headers { get; set; } = true;

        /// <summary>
        /// 是否禁用响应
        /// </summary>
        [DataMember(Name = "no_responders")]
        public bool NoResponders { get; set; }

        internal string Content
        {
            get
            {
                return $"{{{IgnoreNullValue("verbose", IsVerbose, true)}{IgnoreNullValue("pedantic", IsPedantic)}{IgnoreNullValue("tls_required", IsSSLRequired)}{IgnoreNullValue("user", User)}{IgnoreNullValue("pass", Password)}{IgnoreNullValue("name", ClientName)}{IgnoreNullValue("lang", Lang)}{IgnoreNullValue("version", Version)}{IgnoreNullValue("protocol", Protocol)}{IgnoreNullValue("jwt", JwtToken)}{IgnoreNullValue("nkey", NKey)}{IgnoreNullValue("sig", Sign)}{IgnoreNullValue("echo", ECHO)}{IgnoreNullValue("headers", Headers)}{IgnoreNullValue("no_responders", NoResponders)}}}";
            }
        }

        static string IgnoreNullValue(string key, bool value, bool isFrist = false)
        {
            return isFrist ? $"\"{key}\":{(value ? "true" : "false")}" : $",\"{key}\":{(value ? "true" : "false")}";
        }

        static string IgnoreNullValue(string key, string value, bool isFrist = false)
        {
            if (string.IsNullOrEmpty(value)) return string.Empty;
            return isFrist ? $"\"{key}\":\"{value}\"" : $",\"{key}\":\"{value}\"";
        }

        static string IgnoreNullValue(string key, int value, bool isFrist = false)
        {
            return isFrist ? $"\"{key}\":{value}" : $",\"{key}\":{value}";
        }
    }
}
