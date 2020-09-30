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
    public class ConnectRequestPacket : MessagePacket<ConnectRequest>
    {

        /// <summary>
        /// 请求连接到NATS Streaming Server
        /// </summary>
        /// <param name="inboxId"></param>
        /// <param name="clusterID"></param>
        /// <param name="clientID"></param>
        /// <param name="discoverPrefix"></param>
        public ConnectRequestPacket(string inboxId, string clusterID, string clientID, string heartbeatInbox, string discoverPrefix = ProtocolConstants.DiscoverPrefix)
        {
            Subject = $"{discoverPrefix}.{clusterID}";
            ReplyTo = $"{STANInboxs.ConnectResponse}{inboxId}.{Guid.NewGuid():N}";
            Message = new ConnectRequest() { ClientID = clientID, HeartbeatInbox = $"{STANInboxs.Heartbeat}{heartbeatInbox}" };
        }

        [IgnoreDataMember]
        public override STANPacketType PacketType => STANPacketType.ConnectRequest;

    }
}
