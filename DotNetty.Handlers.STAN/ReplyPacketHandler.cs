// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace DotNetty.Handlers.STAN
{
    using DotNetty.Codecs.STAN;
    using DotNetty.Codecs.STAN.Packets;
    using DotNetty.Transport.Channels;
    using System.Threading.Tasks;

    public class ReplyPacketHandler<TPacket> : SimpleChannelInboundHandler<TPacket>
        where TPacket : MessagePacket
    {
        private readonly string _replyTo;
        private readonly TaskCompletionSource<TPacket> _completionSource;
        public ReplyPacketHandler(string inboxId, TaskCompletionSource<TPacket> completionSource)
        {
            _replyTo = $"{STANInboxs.ConnectResponse}{inboxId}";
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