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

    public class ReplyPacketHandler<TPacket> : SimpleChannelInboundHandler<TPacket>
        where TPacket : STANPacket
    {
        static int MessageCount = 0;

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
                _completionSource.SetResult(msg);
            }
            else
            {

                Console.WriteLine("收到消息确认 主题 {0}  第 {1} 条", msg.Subject, Interlocked.Increment(ref MessageCount));
                if (MessageCount == 100)
                {

                    _completionSource.SetResult(msg);
                }
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