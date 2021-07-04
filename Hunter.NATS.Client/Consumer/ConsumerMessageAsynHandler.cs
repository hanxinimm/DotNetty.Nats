﻿using DotNetty.Codecs.NATS.Packets;
using DotNetty.Transport.Channels;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace Hunter.NATS.Client
{
    public class ConsumerMessageAsynHandler : ConsumerMessageHandler
    {
        private readonly Func<NATSMsgContent, ValueTask> _messageHandler;

        public ConsumerMessageAsynHandler(
            ILogger logger,
            NATSConsumerSubscriptionConfig subscriptionConfig,
            Func<NATSMsgContent, ValueTask> messageHandler)
            : base(logger, subscriptionConfig)
        {
            _messageHandler = messageHandler;
        }

        protected override void MessageHandler(MessagePacket msg)
        {
            Task.Factory.StartNew(async _msg =>
            {
                await _messageHandler(PackMsgContent((MessagePacket)_msg));
            }, 
            msg, 
            default, 
            TaskCreationOptions.DenyChildAttach,
            TaskScheduler.Default);
        }
    }
}