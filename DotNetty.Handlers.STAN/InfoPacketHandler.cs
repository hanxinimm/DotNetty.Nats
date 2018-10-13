// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace DotNetty.Handlers.STAN
{
    using System;
    using DotNetty.Codecs.STAN.Packets;
    using DotNetty.Transport.Channels;

    public class ConnectResponsePacketHandler : SimpleChannelInboundHandler<ConnectResponsePacket>
    {
        protected override void ChannelRead0(IChannelHandlerContext contex, ConnectResponsePacket msg)
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