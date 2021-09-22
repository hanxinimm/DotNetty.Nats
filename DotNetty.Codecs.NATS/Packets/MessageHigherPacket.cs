using System.Collections.Generic;
using System.Runtime.Serialization;

namespace DotNetty.Codecs.NATS.Packets
{
    public class MessageHigherPacket : MessagePacket
    {
        [IgnoreDataMember]
        public override NATSPacketType PacketType => NATSPacketType.HMSG;

        public MessageHigherPacket() { }

        public MessageHigherPacket(
            string subject, 
            string subscribeId, 
            string replyTo, 
            int payloadSize,
            byte[] payload, 
            string version,
            IDictionary<string, string> headers)
            : base(subject, subscribeId, replyTo, payloadSize, payload)
        {
            Subject = subject;
            SubscribeId = subscribeId;
            ReplyTo = replyTo;
            Payload = payload;
            PayloadSize = payloadSize;
            Version = version;
            Headers = headers;
        }

        /// <summary>
        /// 版本
        /// </summary>
        [DataMember(Name = "version")]
        public string Version { get; set; }

        /// <summary>
        /// 消息有效头部数据
        /// </summary>
        [DataMember(Name = "headers")]
        public IDictionary<string, string> Headers { get; set; }
    }
}
