// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace DotNetty.Handlers.STAN
{
    using DotNetty.Codecs.STAN;
    using DotNetty.Codecs.STAN.Packets;
    using DotNetty.Transport.Channels;
    using System.Threading.Tasks;

    public class ConnectRequestPacketHandler : SimpleChannelInboundHandler<ConnectResponsePacket>
    {
        private readonly string _replyTo;
        public TaskCompletionSource<ConnectResponsePacket> CompletionSource;
        public ConnectRequestPacketHandler(string inboxId)
        {
            _replyTo = $"{STANInboxs.ConnectResponse}{inboxId}.ConnectRequest";
        }

        protected override void ChannelRead0(IChannelHandlerContext contex, ConnectResponsePacket msg)
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