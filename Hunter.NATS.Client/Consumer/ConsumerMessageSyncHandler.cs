using DotNetty.Codecs.NATS.Packets;
using DotNetty.Transport.Channels;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace Hunter.NATS.Client
{
    public class ConsumerMessageSyncHandler : SubscriptionMessageHandler
    {
        private readonly Action<NATSMsgContent> _messageHandler;

        public ConsumerMessageSyncHandler(
            ILogger logger,
            NATSSubscriptionConfig subscriptionConfig,
            Action<NATSMsgContent> messageHandler)
            : base(logger, subscriptionConfig)
        {
            _messageHandler = messageHandler;
        }

        public ConsumerMessageSyncHandler(
            ILogger logger,
            NATSSubscriptionConfig subscriptionConfig,
            Action<NATSMsgContent> messageHandler,
            Func<NATSSubscriptionConfig, Task> unSubscriptionCallback)
            : base(logger, subscriptionConfig, unSubscriptionCallback)
        {
            _messageHandler = messageHandler;
        }


        protected override void MessageHandler(MessagePacket msg)
        {
            _messageHandler(PackMsgContent(msg));
        }
    }
}
