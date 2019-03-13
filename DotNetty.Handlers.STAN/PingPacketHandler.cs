﻿// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace DotNetty.Handlers.STAN
{
    using System;
    using DotNetty.Codecs.STAN.Packets;
    using DotNetty.Transport.Channels;

    public class PingPacketHandler : SimpleChannelInboundHandler<PingPacket>
    {
        protected override void ChannelRead0(IChannelHandlerContext contex, PingPacket msg)
        {
            Console.WriteLine("PingPacket => PongPacket");
            contex.WriteAndFlushAsync(new PongPacket());
        }
    }
}