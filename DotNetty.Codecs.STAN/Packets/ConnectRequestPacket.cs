using DotNetty.Codecs.STAN.Protocol;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;

namespace DotNetty.Codecs.STAN.Packets
{
    [DataContract]
    public class ConnectRequestPacket : STANPacket<ConnectRequest>
    {

        /// <summary>
        /// 请求连接到NATS Streaming Server
        /// </summary>
        /// <param name="clusterID"></param>
        /// <param name="clientID"></param>
        /// <param name="discoverPrefix"></param>
        public ConnectRequestPacket(string clusterID, string clientID, string replyTo, string discoverPrefix = STANConstants.DiscoverPrefix)
        {
            Subject = $"{discoverPrefix}.{clusterID}";
            ReplyTo = replyTo;
            Message = new ConnectRequest() { ClientID = clientID, HeartbeatInbox = Guid.NewGuid().ToString() };
        }

        [IgnoreDataMember]
        public override STANPacketType PacketType => STANPacketType.ConnectRequest;

    }
}
