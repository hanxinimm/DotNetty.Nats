using DotNetty.Transport.Bootstrapping;
using DotNetty.Transport.Channels;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Hunter.NATS.Client.Handlers
{
    public class ReconnectChannelHandler : ChannelHandlerAdapter
    {

        private readonly ILogger _logger;
        public readonly Func<EndPoint,Task> _reconnectHandler;
        public ReconnectChannelHandler(ILogger logger, Func<EndPoint,Task> reconnectHandler)
        {
            _logger = logger;
            _reconnectHandler = reconnectHandler;
        }

        public override void ChannelInactive(IChannelHandlerContext context)
        {
            base.ChannelInactive(context);
            _logger.LogWarning("ChannelInactive connected to {0}", context.Channel.RemoteAddress);
            context.Channel.EventLoop.Schedule(_ => _reconnectHandler((EndPoint)_), context.Channel.RemoteAddress, TimeSpan.FromMilliseconds(1000));
        }
    }
}
