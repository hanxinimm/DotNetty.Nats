using DotNetty.Codecs.NATS.Packets;
using DotNetty.Codecs.NATSJetStream.Protocol;
using System;
using System.Collections.Generic;
using System.Text;

namespace DotNetty.Codecs.NATSJetStream
{
    public static class NATSJetStreamExtensions
    {
        public static MessageMetadata GetMetadata(this MessagePacket packet)
        {
            return new MessageMetadata(packet);
        }

    }
}
