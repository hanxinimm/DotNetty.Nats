using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace DotNetty.Codecs.STAN.Packets
{
    [DataContract]
    public class ErrorPacket : Packet
    {
        [IgnoreDataMember]
        public override PacketType PacketType => PacketType.MINUS_ERR;

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
