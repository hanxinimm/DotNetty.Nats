// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace DotNetty.Handlers.NATS
{
    using System;
    using System.Threading.Tasks;
    using DotNetty.Codecs.NATS.Packets;
    using DotNetty.Transport.Channels;
    using Newtonsoft.Json;

    public class InfoPacketHandler : SimpleChannelInboundHandler<InfoPacket>
    {
        private readonly TaskCompletionSource<InfoPacket> _infoTaskCompletionSource;
        public InfoPacketHandler(TaskCompletionSource<InfoPacket> infoTaskCompletionSource)
        {
            _infoTaskCompletionSource = infoTaskCompletionSource;
        }
        protected override void ChannelRead0(IChannelHandlerContext contex, InfoPacket msg)
        {
            _infoTaskCompletionSource.SetResult(msg);
            contex.FireChannelReadComplete();
        }
    }
}