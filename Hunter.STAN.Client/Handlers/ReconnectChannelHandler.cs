using DotNetty.Transport.Channels;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Hunter.STAN.Client.Handlers
{
    public class ReconnectChannelHandler : ChannelHandlerAdapter
    {
        public override void ChannelInactive(IChannelHandlerContext context)
        {
            Console.WriteLine("ReconnectChannelHandler");
            Task.Run(() => context.ConnectAsync(new IPEndPoint(IPAddress.Parse("192.168.0.226"), 4222)));
            //base.ChannelInactive(context);
        }

        public override void ExceptionCaught(IChannelHandlerContext contex, Exception e)
        {
            if (e is System.Net.Sockets.SocketException ex)
            {

            }
            Console.WriteLine(DateTime.Now.Millisecond);
            Console.WriteLine("{0}", e.StackTrace);
        }
    }
}
