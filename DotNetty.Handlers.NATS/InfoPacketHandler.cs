// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace DotNetty.Handlers.NATS
{
    using System;
    using System.Threading.Tasks;
    using DotNetty.Codecs.NATS.Packets;
    using DotNetty.Transport.Channels;
    using Microsoft.Extensions.Logging;
    using Newtonsoft.Json;

    public class InfoPacketHandler : SimpleChannelInboundHandler<InfoPacket>
    {
        private readonly ILogger _logger;
        private readonly TaskCompletionSource<InfoPacket> _infoTaskCompletionSource;
        public InfoPacketHandler(ILogger logger, TaskCompletionSource<InfoPacket> infoTaskCompletionSource)
        {
            _logger = logger;
            _infoTaskCompletionSource = infoTaskCompletionSource;
        }
        protected override void ChannelRead0(IChannelHandlerContext contex, InfoPacket msg)
        {
            _infoTaskCompletionSource.SetResult(msg);
        }

        public override void ExceptionCaught(IChannelHandlerContext contex, Exception ex)
        {
            _logger.LogError(ex, "[InfoPacketHandler]NATS消息服务信息异常");
            contex.CloseAsync();
        }
    }
}