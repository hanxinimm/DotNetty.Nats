using DotNetty.Buffers;
using DotNetty.Common;
using Google.Protobuf;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace DotNetty.Codecs.STAN.Packets
{
    [DataContract]
    public abstract class MessagePacket<TMessage> : STANPacket<TMessage>
        where TMessage : IMessage
    {
        [IgnoreDataMember]
        public override STANPacketType PacketType => STANPacketType.MSG;

        /// <summary>
        /// 以字节为单位的有效载荷大小
        /// </summary>
        [IgnoreDataMember]
        public int PayloadSize { get; set; }

    }
}
