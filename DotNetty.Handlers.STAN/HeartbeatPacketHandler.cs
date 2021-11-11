﻿using DotNetty.Codecs.STAN.Packets;
using DotNetty.Transport.Channels;
using System;

namespace DotNetty.Handlers.STAN
{
    public class HeartbeatPacketHandler : SimpleChannelInboundHandler<HeartbeatPacket>
    {
        public override bool IsSharable => true;


        protected override void ChannelRead0(IChannelHandlerContext contex, HeartbeatPacket msg)
        {
            contex.WriteAndFlushAsync(new HeartbeatAckPacket(msg.ReplyTo)).ConfigureAwait(false).GetAwaiter().GetResult();
        }
    }
}
