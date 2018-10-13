// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Runtime.Serialization;

namespace DotNetty.Codecs.NATS.Packets
{
    [DataContract]
    public abstract class Packet
    {
        [IgnoreDataMember]
        public abstract PacketType PacketType { get; }

        public override string ToString()
        {
            return $"{this.GetType().Name}[Type={this.PacketType}]";
        }
    }
}