using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;

namespace DotNetty.Codecs.NATS.Packets
{
    [DataContract]
    public class InfoPacket : NATSPacket
    {
        public override NATSPacketType PacketType => NATSPacketType.INFO;

        #region BASE

        /// <summary>
        /// NATS服务器的唯一标识符
        /// </summary>
        [DataMember(Name = "server_id")]
        public string Id { get; set; }
        /// <summary>
        /// NATS服务器名称
        /// </summary>
        [DataMember(Name = "server_name")]
        public string Name { get; set; }
        /// <summary>
        /// NATS服务器的版本
        /// </summary>
        [DataMember(Name = "version")]
        public string Version { get; set; }
        /// <summary>
        /// NATS服务器的版本
        /// </summary>
        [DataMember(Name = "proto")]
        public int Proto { get; set; }
        /// <summary>
        /// 构建NATS服务器的git提交版本
        /// </summary>
        [DataMember(Name = "git_commit")]
        public string GitCommit { get; set; }
        /// <summary>
        /// 构建NATS服务器的golang版本
        /// </summary>
        [DataMember(Name = "go")]
        public string GoVersion { get; set; }
        /// <summary>
        /// NATS服务器主机的IP地址
        /// </summary>
        [DataMember(Name = "host")]
        public string Host { get; set; }
        /// <summary>
        /// NATS服务器配置为侦听的端口号
        /// </summary>
        [DataMember(Name = "port")]
        public int Port { get; set; }
        /// <summary>
        /// 头部
        /// </summary>
        [DataMember(Name = "headers")]
        public bool Headers { get; set; }
        /// <summary>
        /// 认证必须的
        /// </summary>
        [DataMember(Name = "auth_required")]
        public bool AuthRequired { get; set; }
        /// <summary>
        /// 安全协议必须的
        /// </summary>
        [DataMember(Name = "tls_required")]
        public bool TLSRequired { get; set; }
        /// <summary>
        /// 安全协议验证
        /// </summary>
        [DataMember(Name = "tls_verify")]
        public bool TLSVerify { get; set; }
        /// <summary>
        /// 安全协议激活的
        /// </summary>
        [DataMember(Name = "tls_available")]
        public bool TLSAvailable { get; set; }
        /// <summary>
        /// 服务器将从客户端接受的最大有效负载大小。
        /// </summary>
        [DataMember(Name = "max_payload")]
        public long MaxPayload { get; set; }
        /// <summary>
        /// JetStream 流
        /// </summary>
        [DataMember(Name = "jetstream")]
        public bool JetStream { get; set; }
        /// <summary>
        /// IP 地址
        /// </summary>
        [DataMember(Name = "ip")]
        public string IP { get; set; }
        /// <summary>
        /// 客户端编号
        /// </summary>
        [DataMember(Name = "client_id")]
        public string ClientId { get; set; }
        /// <summary>
        /// 客户端编号
        /// </summary>
        [DataMember(Name = "client_ip")]
        public string ClientIP { get; set; }
        /// <summary>
        /// 随机值
        /// </summary>
        [DataMember(Name = "nonce")]
        public string Nonce { get; set; }
        /// <summary>
        /// 随机值
        /// </summary>
        [DataMember(Name = "cluster")]
        public string Cluster { get; set; }
        /// <summary>
        /// 随机值
        /// </summary>
        [DataMember(Name = "cluster_dynamic")]
        public bool Dynamic { get; set; }
        /// <summary>
        /// 客户端可以连接到的服务器URL的可选列表。
        /// </summary>
        [DataMember(Name = "connect_urls")]
        public List<string> ClusterRoutes { get; set; }
        /// <summary>
        /// 客户端可以连接到的服务器URL的可选列表。
        /// </summary>
        [DataMember(Name = "ws_connect_urls")]
        public List<string> ClusterWSRoutes { get; set; }
        /// <summary>
        /// DuckMode 模式。
        /// </summary>
        [DataMember(Name = "ldm")]
        public bool LameDuckMode { get; set; }


        #endregion

        #region Route Specific

        /// <summary>
        /// 
        /// </summary>
        [DataMember(Name = "import")]
        public bool Import { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [DataMember(Name = "export")]
        public bool Export { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [DataMember(Name = "lnoc")]
        public bool LNOC { get; set; }
        /// <summary>
        /// NATS服务器的唯一标识符
        /// </summary>
        [DataMember(Name = "info_on_connect")]
        public bool InfoOnConnect { get; set; }
        /// <summary>
        /// NATS服务器的唯一标识符
        /// </summary>
        [DataMember(Name = "connect_info")]
        public bool ConnectInfo { get; set; }

        #endregion;


        #region Gateways Specific

        /// <summary>
        /// 网关
        /// </summary>
        [DataMember(Name = "gateway")]
        public string Gateway { get; set; }
        /// <summary>
        /// 网关地址集合
        /// </summary>
        [DataMember(Name = "gateway_urls")]
        public List<string> GatewayUrls { get; set; }
        /// <summary>
        /// 网关地址
        /// </summary>
        [DataMember(Name = "gateway_url")]
        public string GatewayUrl { get; set; }
        /// <summary>
        /// 网关命令
        /// </summary>
        [DataMember(Name = "gateway_cmd")]
        public byte GatewayCMD { get; set; }
        /// <summary>
        /// 网关命令承载
        /// </summary>
        [DataMember(Name = "gateway_cmd_payload")]
        public byte[] GatewayCmdPayload { get; set; }
        /// <summary>
        /// 网关
        /// </summary>
        [DataMember(Name = "gateway_nrp")]
        public bool GatewayNRP { get; set; }


        #region LeafNode Specific

        /// <summary>
        /// 网关
        /// </summary>
        [DataMember(Name = "leafnode_urls")]
        public List<string> LeafNodeUrls { get; set; }

        /// <summary>
        /// 网关
        /// </summary>
        [DataMember(Name = "remote_account")]
        public string RemoteAccount { get; set; }

        #endregion;

        #endregion;

        public static InfoPacket CreateFromJson(string json)
        {
            using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(json)))
            {
                var serializer = new DataContractJsonSerializer(typeof(InfoPacket));
                stream.Position = 0;
                return (InfoPacket)serializer.ReadObject(stream);
            }
        }
    }
}
