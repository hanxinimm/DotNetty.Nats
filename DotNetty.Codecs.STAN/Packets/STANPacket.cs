// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using DotNetty.Buffers;
using Google.Protobuf;
using System.Runtime.Serialization;

namespace DotNetty.Codecs.STAN.Packets
{
    [DataContract]
    public abstract class STANPacket
    {
        [IgnoreDataMember]
        public abstract STANPacketType PacketType { get; }

        public override string ToString()
        {
            return $"{this.GetType().Name}[Type={this.PacketType}]";
        }
    }

    [DataContract]
    public abstract class STANSubscribePacket : STANPacket
    {
        // <summary>
        /// 主题
        /// </summary>
        [DataMember(Name = "subject")]
        public string Subject { get; set; }

        /// <summary>
        /// 唯一的字母数字订阅ID
        /// </summary>
        [DataMember(Name = "sid")]
        public string Id { get; set; }

        /// <summary>
        /// 如果指定，订户将加入此队列组
        /// </summary>

        [DataMember(Name = "queue group")]
        public string Group { get; set; }
    }
}