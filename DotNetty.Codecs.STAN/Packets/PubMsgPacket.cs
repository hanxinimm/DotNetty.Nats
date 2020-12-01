using DotNetty.Codecs.STAN.Protocol;
using Google.Protobuf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;

namespace DotNetty.Codecs.STAN.Packets
{
    [DataContract]
    public class PubMsgPacket : MessagePacket<PubMsg>
    {
        /// <summary>
        /// 发布消息到服务器
        /// </summary>
        /// <param name="inboxId"></param>
        /// <param name="pubPrefix"></param>
        /// <param name="clientID"></param>
        /// <param name="subject"></param>
        /// <param name="data"></param>
        public PubMsgPacket(string inboxId, string pubPrefix, string clientID, ByteString connectID, string subject, byte[] data)
        {
            Subject = $"{pubPrefix}.{subject}";
            ReplyTo = $"{STANInboxs.PubAck}{inboxId}.{Guid.NewGuid().ToString("N")}";
            Message = new PubMsg()
            {
                ClientID = clientID,
                Guid = Guid.NewGuid().ToString(),
                Subject = subject,
                Data = ByteString.CopyFrom(data),
                ConnID = connectID,
            };
        }

        /// <summary>
        /// 发布消息到服务器
        /// </summary>
        /// <param name="inboxId"></param>
        /// <param name="pubPrefix"></param>
        /// <param name="clientID"></param>
        /// <param name="subject"></param>
        /// <param name="data"></param>
        public PubMsgPacket(string inboxId, string pubPrefix, string clientID, string subject)
        {
            Subject = $"{pubPrefix}.{subject}";
            ReplyTo = $"{STANInboxs.PubAck}{inboxId}.{Guid.NewGuid().ToString("N")}";
            Message = new PubMsg() { ClientID = clientID, Guid = Guid.NewGuid().ToString(), Subject = subject };
        }

        public override STANPacketType PacketType => STANPacketType.PubMsg;

    }
}
