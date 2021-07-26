// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace DotNetty.Handlers.STAN
{
    using System;
    using System.Net.Sockets;
    using DotNetty.Codecs.STAN.Packets;
    using DotNetty.Transport.Channels;
    using Microsoft.Extensions.Logging;

    public class ErrorPacketHandler : SimpleChannelInboundHandler<UnknownErrorPacket>
    {
        private readonly ILogger _logger;

        public ErrorPacketHandler(ILogger logger)
        {
            _logger = logger;
        }

        public override bool IsSharable => true;


        protected override void ChannelRead0(IChannelHandlerContext contex, UnknownErrorPacket msg)
        {
            _logger.LogError("[ErrorPacketHandler]STAN消息服务发生错误 错误信息:{0}", msg.Message);
        }

        public override void ExceptionCaught(IChannelHandlerContext contex, Exception ex)
        {
            _logger.LogError(ex, "[ErrorPacketHandler]STAN消息服务发生异常");      
        }
    }
}