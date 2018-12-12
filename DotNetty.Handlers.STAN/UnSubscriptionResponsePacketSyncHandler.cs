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

    public class UnSubscriptionResponsePacketSyncHandler : SimpleChannelInboundHandler<UnSubscriptionResponsePacket>
    {
        private readonly ConcurrentDictionary<string, TaskCompletionSource<UnSubscriptionResponsePacket>> _waitUbSubResponseTaskSchedule;
        public UnSubscriptionResponsePacketSyncHandler(ConcurrentDictionary<string, TaskCompletionSource<UnSubscriptionResponsePacket>> waitUnSubResponseTaskSchedule)
        {
            _waitUbSubResponseTaskSchedule = waitUnSubResponseTaskSchedule;
        }

        protected override void ChannelRead0(IChannelHandlerContext contex, UnSubscriptionResponsePacket msg)
        {
            if (_waitUbSubResponseTaskSchedule.TryRemove(msg.Subject, out var completionSource))
            {
                Console.WriteLine("收到取消订阅响应 主题 {0}  消息确认收件箱 {1}", msg.Subject, msg.ReplyTo);
                completionSource.SetResult(msg);
            }
            else
            {
                contex.FireChannelRead(msg);
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