using DotNetty.Codecs.STAN.Packets;
using DotNetty.Transport.Channels;
using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace DotNetty.Handlers.STAN
{
    public abstract class MessagePacketHandler : SimpleChannelInboundHandler<MsgProtoPacket>
    {

    }
}
