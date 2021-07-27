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
        private readonly string _clientId;
        public PongPacketHandler(ILogger logger, string clientId)
        {
            _logger = logger;
            _clientId = clientId;
        }

        public override bool IsSharable => true;


        protected override void ChannelRead0(IChannelHandlerContext contex, PongPacket msg)
        {
            _logger.LogDebug($"STAN 服务器心跳 客户端编号 {_clientId} PongPacket => PingPacket");
        }
    }
}