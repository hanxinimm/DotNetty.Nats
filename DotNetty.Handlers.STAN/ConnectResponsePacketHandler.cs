﻿// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace DotNetty.Handlers.STAN
{
    using System;
    using System.Threading.Tasks;
    using DotNetty.Codecs.STAN.Packets;
    using DotNetty.Codecs.STAN.Protocol;
    using DotNetty.Transport.Channels;
    using Google.Protobuf;

    public class ConnectResponsePacketHandler : SimpleChannelInboundHandler<MessagePacket>
    {
        private readonly string _replyTo;
        private readonly TaskCompletionSource<ConnectResponsePacket> _completionSource;
        public ConnectResponsePacketHandler(string replyTo, TaskCompletionSource<ConnectResponsePacket> completionSource)
        {
            _replyTo = replyTo;
            _completionSource = completionSource;
        }

        protected override void ChannelRead0(IChannelHandlerContext contex, MessagePacket msg)
        {
            if (msg.Subject == _replyTo)
            {
                var s = new ConnectResponse();
                s.MergeFrom(msg.Payload);
                _completionSource.SetResult(new ConnectResponsePacket() { Message = s, Subject = msg.Subject, ReplyTo = msg.ReplyTo });
            }
        }

        public override void ExceptionCaught(IChannelHandlerContext contex, Exception e)
        {
            Console.WriteLine(DateTime.Now.Millisecond);
            Console.WriteLine("{0}", e.StackTrace);
            contex.CloseAsync();
        }
    }
}