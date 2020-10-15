// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace DotNetty.Handlers.STAN
{
    using System;
    using DotNetty.Codecs.STAN.Packets;
    using DotNetty.Transport.Channels;
    using Microsoft.Extensions.Logging;

    public class PongPacketHandler : SimpleChannelInboundHandler<PongPacket>
    {
        private readonly ILogger _logger;
        public PongPacketHandler(ILogger logger)
        {
            _logger = logger;
        }

        protected override void ChannelRead0(IChannelHandlerContext contex, PongPacket msg)
        {
            _logger.LogDebug("PongPacket => PingPacket");

            contex.WriteAndFlushAsync(new PingPacket());
        }
    }
}