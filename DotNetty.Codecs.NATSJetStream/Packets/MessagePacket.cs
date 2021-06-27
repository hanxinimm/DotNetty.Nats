using DotNetty.Codecs.NATS.Packets;
using System.Runtime.Serialization;

namespace DotNetty.Codecs.NATSJetStream.Packets
{
    [DataContract]
    public abstract class MessagePacket<TMessage> : MessagePacket
    {
        /// <summary>
        /// 消息
        /// </summary>
        [DataMember(Name = "message")]
        public TMessage Message { get; set; }
    }
}
