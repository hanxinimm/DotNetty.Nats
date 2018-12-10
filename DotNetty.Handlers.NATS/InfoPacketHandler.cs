// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace DotNetty.Handlers.NATS
{
    using System;
    using System.Threading.Tasks;
    using DotNetty.Codecs.NATS.Packets;
    using DotNetty.Transport.Channels;
    using Newtonsoft.Json;

    public class InfoPacketHandler : SimpleChannelInboundHandler<InfoPacket>
    {
        private readonly Action<InfoPacket> _infoCallback;
        public InfoPacketHandler(Action<InfoPacket> infoCallback)
        {
            _infoCallback = infoCallback;
        }
        protected override void ChannelRead0(IChannelHandlerContext contex, InfoPacket msg)
        {
            _infoCallback(msg);
        }

        public override void ExceptionCaught(IChannelHandlerContext contex, Exception e)
        {
            Console.WriteLine(DateTime.Now.Millisecond);
            Console.WriteLine("{0}", e.StackTrace);
            contex.CloseAsync();
        }
    }
}