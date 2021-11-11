using DotNetty.Transport.Bootstrapping;
using DotNetty.Transport.Channels;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Hunter.STAN.Client.Handlers
{
    public class ReconnectChannelHandler : ChannelHandlerAdapter
    {

        private readonly ILogger _logger;
        public readonly Action<EndPoint> _reconnectHandler;
        public ReconnectChannelHandler(ILogger logger, Action<EndPoint> reconnectHandler)
        {
            _logger = logger;
            _reconnectHandler = reconnectHandler;
        }
        public override bool IsSharable => true;

        public override void ChannelInactive(IChannelHandlerContext context)
        {
            base.ChannelInactive(context);
            _logger.LogDebug("ChannelInactive connected to {0}", context.Channel.RemoteAddress);
            context.Channel.EventLoop.ScheduleAsync(_ => _reconnectHandler((EndPoint)_), context.Channel.RemoteAddress, TimeSpan.FromMilliseconds(1000)).ConfigureAwait(false).GetAwaiter().GetResult();
        }
    }
}
