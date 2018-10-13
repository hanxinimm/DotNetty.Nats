using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;

namespace Hunter.NATS.Client
{
    [DataContract]
    internal class ServerInfo
    {
        //internal string serverId;
        //internal string serverHost;
        //internal int serverPort;
        //internal string serverVersion;
        //internal bool authRequired;
        //internal bool tlsRequired;
        //internal long maxPayload;
        //internal string[] connectURLs;

        [DataMember(Name = "server_id")]
        public string Id { get; set; }

        [DataMember(Name = "host")]
        public string Host { get; set; }

        [DataMember(Name = "port")]
        public int Port { get; set; }

        [DataMember(Name = "version")]
        public string Version { get; set; }

        [DataMember(Name = "go")]
        public string GoVersion { get; set; }

        [DataMember(Name = "max_payload")]
        public long MaxPayloadCapacity { get; set; }

        [DataMember(Name = "auth_required")]
        public bool IsAuthentication { get; set; }

        [DataMember(Name = "tls_required")]
        public bool IsTLS { get; set; }

        [DataMember(Name = "tls_verify")]
        public bool IsVerifyTLS { get; set; }

        [DataMember]
        public string[] ClusterRoutes { get; set; }

        public static ServerInfo CreateFromJson(string json)
        {
            using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(json)))
            {
                var serializer = new DataContractJsonSerializer(typeof(ServerInfo));
                stream.Position = 0;
                return (ServerInfo)serializer.ReadObject(stream);
            }
        }
    }
}
