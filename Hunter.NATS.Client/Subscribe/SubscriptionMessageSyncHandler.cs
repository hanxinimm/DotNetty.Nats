using DotNetty.Codecs.NATS.Packets;
using DotNetty.Transport.Channels;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace Hunter.NATS.Client
{
    public class SubscriptionMessageSyncHandler : SubscriptionMessageHandler
    {
        private readonly Action<NATSMsgContent> _messageHandler;
#if NETSTANDARD2_1
        private readonly ValueTask completedValueTask = new ValueTask();
#endif

        public SubscriptionMessageSyncHandler(
            ILogger logger,
            NATSMsgSubscriptionConfig subscriptionConfig,
            Action<NATSMsgContent> messageHandler)
            : base(logger, subscriptionConfig)
        {
            _messageHandler = messageHandler;
        }

        public SubscriptionMessageSyncHandler(
            ILogger logger,
            NATSMsgSubscriptionConfig subscriptionConfig,
            Action<NATSMsgContent> messageHandler,
            Func<NATSMsgSubscriptionConfig, Task> unSubscriptionCallback)
            : base(logger, subscriptionConfig, unSubscriptionCallback)
        {
            _messageHandler = messageHandler;
        }

        protected override ValueTask MessageHandler(NATSMsgContent msg)
        {
            _messageHandler(msg);

#if NETSTANDARD2_1
            return completedValueTask;
#else
            return ValueTask.CompletedTask;
#endif
        }
    }
}
