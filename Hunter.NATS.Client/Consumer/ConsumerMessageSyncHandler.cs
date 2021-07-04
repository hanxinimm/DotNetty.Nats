using DotNetty.Codecs.NATS.Packets;
using DotNetty.Transport.Channels;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace Hunter.NATS.Client
{
    public class ConsumerMessageSyncHandler : ConsumerMessageHandler
    {
        private readonly Action<NATSMsgContent> _messageHandler;

        public ConsumerMessageSyncHandler(
            ILogger logger,
            NATSConsumerSubscriptionConfig subscriptionConfig,
            Action<NATSMsgContent> messageHandler)
            : base(logger, subscriptionConfig)
        {
            _messageHandler = messageHandler;
        }

        protected override void MessageHandler(MessagePacket msg)
        {
            _messageHandler(PackMsgContent(msg));
        }
    }
}
