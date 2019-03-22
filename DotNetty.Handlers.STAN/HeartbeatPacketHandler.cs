using DotNetty.Codecs.STAN.Packets;
using DotNetty.Transport.Channels;
using System;

namespace DotNetty.Handlers.STAN
{
    public class HeartbeatPacketHandler : SimpleChannelInboundHandler<HeartbeatPacket>
    {
        protected override void ChannelRead0(IChannelHandlerContext contex, HeartbeatPacket msg)
        {
            Console.WriteLine("Heartbeat " + DateTime.Now.ToString("yyyy MM dd HH:mm:ss"));
            contex.WriteAndFlushAsync(new HeartbeatAckPacket(msg.ReplyTo));
        }
    }
}
