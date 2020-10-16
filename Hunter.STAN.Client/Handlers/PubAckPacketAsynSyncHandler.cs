﻿// Copyright (c) Microsoft. All rights reserved.
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

    public class PubAckPacketAsynSyncHandler : PubAckPacketHandler
    {

        private readonly ILogger _logger;
        public PubAckPacketAsynSyncHandler(ILogger logger)
        {
            _logger = logger;
        }

        protected override void ChannelRead0(IChannelHandlerContext contex, PubAckPacket msg)
        {
            if (!string.IsNullOrEmpty(msg.Message.Error))
            {
                _logger.LogError($"[PubAckPacketAsynSyncHandler]消息标识 {msg.Message.Guid} 错误信息 {msg.Message.Error}");
            }
            else
            {
                _logger.LogDebug($"[PubAckPacketAsynSyncHandler]消息标识 {msg.Message.Guid}  发布成功");
            }
        }
    }
}