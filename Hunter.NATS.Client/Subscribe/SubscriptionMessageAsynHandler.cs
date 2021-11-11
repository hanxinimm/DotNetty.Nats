using DotNetty.Codecs.NATS.Packets;
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
            NATSMsgSubscriptionConfig subscriptionConfig,
            Func<NATSMsgContent, ValueTask> messageHandler)
            : base(logger, subscriptionConfig)
        {
            _messageHandler = messageHandler;
        }

        public SubscriptionMessageAsynHandler(
            ILogger logger,
            NATSMsgSubscriptionConfig subscriptionConfig,
            Func<NATSMsgContent, ValueTask> messageHandler,
            Func<NATSMsgSubscriptionConfig, Task> unSubscriptionCallback)
            : base(logger, subscriptionConfig, unSubscriptionCallback)
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
