using DotNetty.Buffers;
using DotNetty.Codecs.NATS.Packets;
using DotNetty.Common;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace DotNetty.Codecs.NATSJetStream.Packets
{
    [DataContract]
    public abstract class DirectivePacket : PublishPacket
    {
        public DirectivePacket() { }

        public DirectivePacket(string subject, string replyTo)
        {
            Subject = subject;
            ReplyTo = replyTo;
        }
    }
}
