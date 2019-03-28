using DotNetty.Transport.Bootstrapping;
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
        public readonly Func<EndPoint,Task> _reconnectHandler;
        public ReconnectChannelHandler(Func<EndPoint,Task> reconnectHandler)
        {
            _reconnectHandler = reconnectHandler;
        }

        public override void ChannelInactive(IChannelHandlerContext context)
        {
            base.ChannelInactive(context);
            Console.WriteLine("ChannelInactive connected to {0}", context.Channel.RemoteAddress);
            context.Channel.EventLoop.Schedule(_ => _reconnectHandler((EndPoint)_), context.Channel.RemoteAddress, TimeSpan.FromMilliseconds(1000));
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
