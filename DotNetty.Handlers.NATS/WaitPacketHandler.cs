// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace DotNetty.Handlers.NATS
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using DotNetty.Codecs.NATS.Packets;
    using DotNetty.Transport.Channels;

    public class WaitPacketHandler<TPacket> : SimpleChannelInboundHandler<TPacket>
    {
        private readonly TaskCompletionSource<TPacket> _completionSource;
        public WaitPacketHandler(TaskCompletionSource<TPacket> completionSource)
        {
            _completionSource = completionSource;
        }

        protected override void ChannelRead0(IChannelHandlerContext contex, TPacket msg)
        {
            _completionSource.SetResult(msg);
        }
    }
}