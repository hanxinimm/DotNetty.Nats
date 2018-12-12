using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace DotNetty.Codecs.NATS.Packets
{
    public class UnknownErrorPacket : ErrorPacket
    {
        public UnknownErrorPacket(string message)
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
