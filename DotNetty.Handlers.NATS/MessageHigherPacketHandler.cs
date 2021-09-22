using DotNetty.Codecs.NATS.Packets;
using DotNetty.Transport.Channels;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace DotNetty.Handlers.NATS
{
    public abstract class MessageHigherPacketHandler : SimpleChannelInboundHandler<MessageHigherPacket>
    {

    }
}
