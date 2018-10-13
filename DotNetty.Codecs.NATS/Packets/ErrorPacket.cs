using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace DotNetty.Codecs.NATS.Packets
{
    [DataContract]
    public class ErrorPacket : NATSPacket
    {
        [IgnoreDataMember]
        public override NATSPacketType PacketType => NATSPacketType.MINUS_ERR;

        public ErrorPacket(string message)
        {
            Message = message;
        }

        /// <summary>
        /// 错误的消息主题
        /// </summary>
        [DataMember]
        public string Message { get; set; }
    }
}
