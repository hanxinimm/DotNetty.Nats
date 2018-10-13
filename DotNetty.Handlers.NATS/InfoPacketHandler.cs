// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace DotNetty.Handlers.NATS
{
    using System;
    using DotNetty.Codecs.NATS.Packets;
    using DotNetty.Transport.Channels;

    public class InfoPacketHandler : SimpleChannelInboundHandler<InfoPacket>
    {
        protected override void ChannelRead0(IChannelHandlerContext contex, InfoPacket msg)
        {
            Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(msg));
        }

        public override void ExceptionCaught(IChannelHandlerContext contex, Exception e)
        {
            Console.WriteLine(DateTime.Now.Millisecond);
            Console.WriteLine("{0}", e.StackTrace);
            contex.CloseAsync();
        }
    }
}