using DotNetty.Codecs.NATS.Packets;
using DotNetty.Codecs.NATSJetStream;
using DotNetty.Codecs.NATSJetStream.Protocol;
using DotNetty.Codecs.Protocol;
using DotNetty.Handlers.NATS;
using DotNetty.Transport.Channels;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace Hunter.NATS.Client
{
    public abstract class ConsumerMessageHandler : MessagePacketHandler
    {
        protected readonly NATSConsumerSubscriptionConfig _subscriptionConfig;
        protected readonly Func<NATSConsumerSubscriptionConfig, MessagePacket, MessageAck, ValueTask> _messageAckCallback;
        public ConsumerMessageHandler(
            ILogger logger,
            NATSConsumerSubscriptionConfig subscriptionConfig,
            Func<NATSConsumerSubscriptionConfig, MessagePacket, MessageAck, ValueTask> messageAckCallback)
            : base(logger)
        {
            _subscriptionConfig = subscriptionConfig;
            _messageAckCallback = messageAckCallback;
            SubscribeId = subscriptionConfig.SubscribeId;
        }

        public override bool IsSharable => true;

        public NATSConsumerSubscriptionConfig SubscriptionConfig => _subscriptionConfig;

        protected NATSJetStreamMsgContent PackMsgContent(MessagePacket msg)
        {
            var metadata = msg.GetMetadata();

            return new NATSJetStreamMsgContent()
            {
                SubscribeId = msg.SubscribeId,
                Subject = msg.Subject,
                ReplyTo = msg.ReplyTo,
                Data = msg.Payload,
                Metadata = metadata
            };
        }

    }
}
