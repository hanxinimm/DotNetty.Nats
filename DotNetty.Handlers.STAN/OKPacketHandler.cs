using DotNetty.Codecs.STAN.Packets;
using DotNetty.Transport.Channels;
using System;
using System.Collections.Generic;
using System.Text;

namespace DotNetty.Handlers.STAN
{
    public class OKPacketHandler : SimpleChannelInboundHandler<OKPacket>
    {
        protected override void ChannelRead0(IChannelHandlerContext contex, OKPacket msg)
        {
            //Console.WriteLine("OK");
        }

        public override void ExceptionCaught(IChannelHandlerContext contex, Exception e)
        {
            Console.WriteLine(DateTime.Now.Millisecond);
            Console.WriteLine("{0}", e.StackTrace);
            contex.CloseAsync();
        }
    }
}
