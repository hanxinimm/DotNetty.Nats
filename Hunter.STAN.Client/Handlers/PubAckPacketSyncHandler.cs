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
    using Microsoft.Extensions.Logging;

    public class PubAckPacketSyncHandler : PubAckPacketHandler
    {
        private readonly ILogger _logger;
        private readonly ConcurrentDictionary<string, TaskCompletionSource<PubAckPacket>> _waitPubAckTaskSchedule;
        public PubAckPacketSyncHandler(ILogger logger, ConcurrentDictionary<string, TaskCompletionSource<PubAckPacket>> waitPubAckTaskSchedule)
        {
            _waitPubAckTaskSchedule = waitPubAckTaskSchedule;
        }

        protected override void ChannelRead0(IChannelHandlerContext contex, PubAckPacket msg)
        {

            if ( _waitPubAckTaskSchedule.Count > 0 && _waitPubAckTaskSchedule.TryRemove(msg.Subject, out var completionSource))
            {
                _logger.LogDebug($"[PubAckPacketSyncHandler]消息标识 {msg.Message.Guid}  发布成功");
                completionSource.SetResult(msg);
            }
            else
            {
                contex.FireChannelRead(msg);
            }
        }
    }
}