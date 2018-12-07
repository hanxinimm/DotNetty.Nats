// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace DotNetty.Handlers.STAN
{
    using System;
    using System.Collections.Concurrent;
    using System.Threading;
    using System.Threading.Tasks;
    using DotNetty.Codecs.STAN.Packets;
    using DotNetty.Codecs.STAN.Protocol;
    using DotNetty.Transport.Channels;
    using Google.Protobuf;

    public class PubAckPacketAsynHandler : SimpleChannelInboundHandler<PubAckPacket>
    {
        private readonly Action<PubAckPacket> _pubAckCallback;
        public PubAckPacketAsynHandler(Action<PubAckPacket> pubAckCallback)
        {
            _pubAckCallback = pubAckCallback;
        }

        protected override void ChannelRead0(IChannelHandlerContext contex, PubAckPacket msg)
        {
            _pubAckCallback(msg);
        }

        public override void ExceptionCaught(IChannelHandlerContext contex, Exception e)
        {
            Console.WriteLine(DateTime.Now.Millisecond);
            Console.WriteLine("{0}", e.StackTrace);
            contex.CloseAsync();
        }
    }
}