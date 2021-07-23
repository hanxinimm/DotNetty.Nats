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
        private readonly string _clientId;
        public PingPacketHandler(ILogger logger, string clientId)
        {
            _logger = logger;
            _clientId = clientId;
        }

        protected override void ChannelRead0(IChannelHandlerContext contex, PingPacket msg)
        {
            _logger.LogDebug($"STAN 服务器心跳 客户端编号 {_clientId} PingPacket => PongPacket");
            contex.WriteAndFlushAsync(new PongPacket()).GetAwaiter();
        }
    }
}