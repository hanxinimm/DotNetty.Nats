﻿using DotNetty.Codecs.NATS.Packets;
using DotNetty.Transport.Channels;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace Hunter.NATS.Client
{
    public class SubscriptionMessageAsynHandler : SubscriptionMessageHandler
    {
        private readonly Func<NATSMsgContent, ValueTask> _messageHandler;

        public SubscriptionMessageAsynHandler(
            ILogger logger,
            NATSSubscriptionConfig subscriptionConfig,
            Func<NATSMsgContent, ValueTask> messageHandler)
            : base(logger, subscriptionConfig)
        {
            _messageHandler = messageHandler;
        }

        public SubscriptionMessageAsynHandler(
            ILogger logger,
            NATSSubscriptionConfig subscriptionConfig,
            Func<NATSMsgContent, ValueTask> messageHandler,
            Func<NATSSubscriptionConfig, Task> unSubscriptionCallback)
            : base(logger, subscriptionConfig, unSubscriptionCallback)
        {
            _messageHandler = messageHandler;
        }

        protected override void MessageHandler(MessagePacket msg)
        {
            Task.Factory.StartNew(async o =>
            {
                await _messageHandler(PackMsgContent(msg));
            }, 
            msg, 
            default, 
            TaskCreationOptions.DenyChildAttach,
            TaskScheduler.Default);
        }
    }
}
