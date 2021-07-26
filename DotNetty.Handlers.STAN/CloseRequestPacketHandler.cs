// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace DotNetty.Handlers.STAN
{
    using DotNetty.Codecs.STAN;
    using DotNetty.Codecs.STAN.Packets;
    using DotNetty.Transport.Channels;
    using System.Threading.Tasks;

    public class CloseRequestPacketHandler : SimpleChannelInboundHandler<CloseResponsePacket>
    {
        private readonly string _replyTo;
        public TaskCompletionSource<CloseResponsePacket> CompletionSource;
        public CloseRequestPacketHandler(string inboxId)
        {
            _replyTo = $"{STANInboxs.CloseResponse}{inboxId}.CloseRequest";
        }

        protected override void ChannelRead0(IChannelHandlerContext contex, CloseResponsePacket msg)
        {
            if (msg.Subject == _replyTo)
            {
                CompletionSource?.TrySetResult(msg);
            }
            else
            {
                contex.FireChannelRead(msg);
            }
        }
    }
}