// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

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
}