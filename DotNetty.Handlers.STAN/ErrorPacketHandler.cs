// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace DotNetty.Handlers.STAN
{
    using System;
    using System.Net.Sockets;
    using DotNetty.Codecs.STAN.Packets;
    using DotNetty.Transport.Channels;

    public class ErrorPacketHandler : SimpleChannelInboundHandler<UnknownErrorPacket>
    {
        protected override void ChannelRead0(IChannelHandlerContext contex, UnknownErrorPacket msg)
        {
            
        }
    }
}