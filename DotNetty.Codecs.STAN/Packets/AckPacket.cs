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
    public class AckPacket : MessagePacket<Ack>
    {
        /// <summary>
        /// 请求连接到NATS Streaming Server
        /// </summary>
        /// <param name="ackInboxId"></param>
        /// <param name="subject"></param>
        /// <param name="sequence"></param>
        public AckPacket(string ackInboxId, string subject, ulong sequence)
        {
            Subject = ackInboxId;
            Message = new Ack()
            {
                Subject = subject,
                Sequence = sequence
            };
        }

        public override STANPacketType PacketType => STANPacketType.Ack;

    }
}
