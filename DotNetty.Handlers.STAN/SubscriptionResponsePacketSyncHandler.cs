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

    public class SubscriptionResponsePacketSyncHandler : SimpleChannelInboundHandler<SubscriptionResponsePacket>
    {
        private readonly ConcurrentDictionary<string, TaskCompletionSource<SubscriptionResponsePacket>> _waitSubResponseTaskSchedule;
        public SubscriptionResponsePacketSyncHandler(ConcurrentDictionary<string, TaskCompletionSource<SubscriptionResponsePacket>> waitSubResponseTaskSchedule)
        {
            _waitSubResponseTaskSchedule = waitSubResponseTaskSchedule;
        }

        protected override void ChannelRead0(IChannelHandlerContext contex, SubscriptionResponsePacket msg)
        {
            if (_waitSubResponseTaskSchedule.TryRemove(msg.Subject, out var completionSource))
            {
                completionSource.SetResult(msg);
            }
            else
            {
                contex.FireChannelRead(msg);
            }
        }
    }
}