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
    public class CloseRequestPacket : MessagePacket<CloseRequest>
    {
        /// <summary>
        /// 发送关闭服务器连接
        /// </summary>
        /// <param name="inboxId"></param>
        /// <param name="subRequests"></param>
        /// <param name="clientID"></param>
        public CloseRequestPacket(string inboxId, string subRequests, string clientID)
        {
            Subject = subRequests;
            ReplyTo = $"{STANInboxs.CloseResponse}{inboxId}.{Guid.NewGuid().ToString("N")}";
            Message = new CloseRequest()
            {
                ClientID = clientID
            };
        }


        public override STANPacketType PacketType => STANPacketType.CloseRequest;

    }
}
