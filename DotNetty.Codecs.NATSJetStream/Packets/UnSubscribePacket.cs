using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace DotNetty.Codecs.NATSJetStream.Packets
{
    [DataContract]
    public class UnSubscribePacket : NATSJetStreamPacket
    {
        [IgnoreDataMember]
        public override NATSJetStreamPacketType PacketType => NATSJetStreamPacketType.UNSUB;

        public UnSubscribePacket(string id)
        {
            Id = id;
        }
        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="id">唯一的字母数字订阅ID</param>
        /// <param name="waitMessages">自动取消订阅之前等待的消息数量</param>
        public UnSubscribePacket(string id, int waitMessages)
        {
            Id = id;
            WaitMessages = waitMessages;
        }

        /// <summary>
        /// 唯一的字母数字订阅ID
        /// </summary>
        [DataMember(Name = "sid")]
        public string Id { get; set; }

        /// <summary>
        /// 自动取消订阅之前等待的消息数量
        /// </summary>

        [DataMember(Name = "max_msgs")]
        public int? WaitMessages { get; set; }
    }
}
