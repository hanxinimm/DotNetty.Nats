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

        /// <summary>
        /// NATS服务器的唯一标识符
        /// </summary>
        [DataMember(Name = "server_id")]
        public string Id { get; set; }

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
        /// NATS服务器的版本
        /// </summary>
        [DataMember(Name = "version")]
        public string Version { get; set; }

        /// <summary>
        /// 构建NATS服务器的golang版本
        /// </summary>
        [DataMember(Name = "go")]
        public string GoVersion { get; set; }


        /// <summary>
        /// 服务器将从客户端接受的最大有效负载大小。
        /// </summary>
        [DataMember(Name = "max_payload")]
        public long MaxPayloadCapacity { get; set; }

        /// <summary>
        /// 最大的内存。
        /// </summary>
        [DataMember(Name = "max_memory")]
        public long MaxMemory { get; set; }

        /// <summary>
        /// 最大的存储。
        /// </summary>
        [DataMember(Name = "max_storage")]
        public long MaxStorage { get; set; }

        /// <summary>
        /// 最大的流数量。
        /// </summary>
        [DataMember(Name = "max_streams")]
        public long MaxStreams { get; set; }

        /// <summary>
        /// 最大的消费者数量。
        /// </summary>
        [DataMember(Name = "max_consumers")]
        public long MaxConsumers { get; set; }


        /// <summary>
        /// 如果已设置，则客户端应尝试在连接时进行身份验证。
        /// </summary>
        [DataMember(Name = "auth_required")]
        public bool IsAuthentication { get; set; }

        /// <summary>
        /// 如果已设置，则客户端必须使用SSL进行身份验证。
        /// </summary>
        [DataMember(Name = "tls_required")]
        public bool IsTLS { get; set; }

        [DataMember(Name = "tls_verify")]
        public bool IsVerifyTLS { get; set; }

        /// <summary>
        /// 客户端可以连接到的服务器URL的可选列表。
        /// </summary>
        [DataMember(Name = "connect_urls")]
        public List<string> ClusterRoutes { get; set; }

        /// <summary>
        /// 头部
        /// </summary>
        [DataMember(Name = "headers")]
        public bool Headers { get; set; }

        /// <summary>
        /// JetStream 流
        /// </summary>
        [DataMember(Name = "jetstream")]
        public bool JetStream { get; set; }

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
