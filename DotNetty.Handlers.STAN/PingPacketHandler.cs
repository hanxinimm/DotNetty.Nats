// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace DotNetty.Handlers.STAN
{
    using System;
    using DotNetty.Codecs.STAN.Packets;
    using DotNetty.Transport.Channels;
    using Microsoft.Extensions.Logging;

    public class PingPacketHandler : SimpleChannelInboundHandler<PingPacket>
    {
        private readonly ILogger _logger;
        public PingPacketHandler(ILogger logger)
        {
            _logger = logger;
        }

        protected override void ChannelRead0(IChannelHandlerContext contex, PingPacket msg)
        {
            _logger.LogDebug("STAN 服务器心跳 PingPacket => PongPacket");
            contex.WriteAndFlushAsync(new PongPacket());
        }
    }
}