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
        /// <param name="inboxId"></param>
        /// <param name="clusterID"></param>
        /// <param name="clientID"></param>
        /// <param name="discoverPrefix"></param>
        public ConnectRequestPacket(string inboxId, string clusterID, string clientID, string discoverPrefix = STANConstants.DiscoverPrefix)
        {
            Subject = $"{discoverPrefix}.{clusterID}";
            ReplyTo = $"{STANInboxs.ConnectResponse}{inboxId}.{Guid.NewGuid().ToString("N")}";
            Message = new ConnectRequest() { ClientID = clientID, HeartbeatInbox = Guid.NewGuid().ToString("N") };
        }

        [IgnoreDataMember]
        public override STANPacketType PacketType => STANPacketType.ConnectRequest;

    }
}
