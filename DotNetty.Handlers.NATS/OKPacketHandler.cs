using DotNetty.Codecs.NATS.Packets;
using DotNetty.Transport.Channels;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace DotNetty.Handlers.NATS
{
    public class OKPacketHandler : SimpleChannelInboundHandler<OKPacket>
    {
        private readonly ILogger _logger;
        public OKPacketHandler(ILogger logger)
        {
            _logger = logger;
        }

        protected override void ChannelRead0(IChannelHandlerContext contex, OKPacket msg)
        {
            _logger.LogDebug("NATS 服务器消息发布 OK");
            contex.FireChannelReadComplete();
        }
    }
}
