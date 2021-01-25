// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace DotNetty.Handlers.STAN
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using DotNetty.Codecs.STAN.Packets;
    using DotNetty.Codecs.STAN.Protocol;
    using DotNetty.Transport.Channels;
    using Google.Protobuf;
    using Microsoft.Extensions.Logging;

    public class ReplyPacketHandler<TPacket> : SimpleChannelInboundHandler<TPacket>
        where TPacket : MessagePacket
    {
        private readonly string _replyTo;
        private readonly TaskCompletionSource<TPacket> _completionSource;
        public ReplyPacketHandler(string replyTo, TaskCompletionSource<TPacket> completionSource)
        {
            _replyTo = replyTo;
            _completionSource = completionSource;
        }

        protected override void ChannelRead0(IChannelHandlerContext contex, TPacket msg)
        {
            if (msg.Subject == _replyTo)
            {
                _completionSource.TrySetResult(msg);
            }
            else
            {
                contex.FireChannelRead(msg);
            }
        }
    }
}