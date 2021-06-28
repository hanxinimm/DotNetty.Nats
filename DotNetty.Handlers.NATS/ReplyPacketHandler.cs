// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace DotNetty.Handlers.NATS
{
    using DotNetty.Codecs.NATS.Packets;
    using DotNetty.Transport.Channels;
    using System.Threading.Tasks;

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