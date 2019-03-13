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
    public class PubMultipleMsgPacket : STANPacket
    {
        /// <summary>
        /// 发布消息到服务器
        /// </summary>
        /// <param name="msgPackets"></param>
        public PubMultipleMsgPacket(string inboxId)
        {
            ReplyTo = $"{STANInboxs.PubAck}{inboxId}.{Guid.NewGuid().ToString("N")}";
            MessagePackets = new List<PubMsgPacket>();
        }

        /// <summary>
        /// 发布消息到服务器
        /// </summary>
        /// <param name="msgPackets"></param>
        public PubMultipleMsgPacket(string inboxId, IList<PubMsgPacket> msgPackets)
        {
            ReplyTo = $"{STANInboxs.PubAck}{inboxId}.{Guid.NewGuid().ToString("N")}";
            MessagePackets = msgPackets;
        }

        // <summary>
        /// 主题回复标识
        /// </summary>
        [DataMember(Name = "replyTo")]
        public string ReplyTo { get; set; }

        public IList<PubMsgPacket> MessagePackets { get; set; }

        public override STANPacketType PacketType => STANPacketType.MultiplePubMsg;

    }
}
