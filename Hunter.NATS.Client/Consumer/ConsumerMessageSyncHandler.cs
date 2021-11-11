using DotNetty.Codecs.NATS.Packets;
using DotNetty.Codecs.Protocol;
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
        private readonly Action<NATSJetStreamMsgContent> _messageHandler;

        public ConsumerMessageSyncHandler(
            ILogger logger,
            NATSConsumerSubscriptionConfig subscriptionConfig,
            Action<NATSJetStreamMsgContent> messageHandler,
            Func<NATSConsumerSubscriptionConfig, MessagePacket, MessageAck, ValueTask> messageAckCallback)
            : base(logger, subscriptionConfig, messageAckCallback)
        {
            _messageHandler = messageHandler;
        }

        protected override async ValueTask HandleMessageAsync(MessagePacket msg)
        {
            _messageHandler(PackMsgContent(msg));
            await _messageAckCallback(_subscriptionConfig, msg, MessageAck.Ack);
        }
    }
}
