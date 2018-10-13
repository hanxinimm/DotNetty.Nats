using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;

namespace DotNetty.Codecs.STAN.Packets
{
    [DataContract]
    public class ConnectRequestPacket : STANPacket
    {
        [IgnoreDataMember]
        public override STANPacketType PacketType => STANPacketType.ConnectRequest;

        /// <summary>
        /// TODO:写成包内可以访问
        /// </summary>
        /// <param name="clientID"></param>
        /// <param name="heartbeatInbox"></param>
        public ConnectRequestPacket(string clientID, string heartbeatInbox)
        {
            ClientID = clientID;
            HeartbeatInbox = heartbeatInbox;
        }

        /// <summary>
        /// 客户端的唯一标识符
        /// </summary>
        [DataMember(Name = "clientID")]
        public string ClientID { get; set; }

        /// <summary>
        /// NATS Streaming Server将为客户端发送心跳的收件箱
        /// </summary>
        [DataMember(Name = "heartbeatInbox")]
        public string HeartbeatInbox { get; set; }
    }
}
