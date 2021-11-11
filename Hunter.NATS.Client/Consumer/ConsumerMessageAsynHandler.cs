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
    public class ConsumerMessageAsynHandler : ConsumerMessageHandler
    {
        private readonly Func<NATSJetStreamMsgContent, ValueTask> _messageHandler;

        public ConsumerMessageAsynHandler(
            ILogger logger,
            NATSConsumerSubscriptionConfig subscriptionConfig,
            Func<NATSJetStreamMsgContent, ValueTask> messageHandler,
            Func<NATSConsumerSubscriptionConfig, MessagePacket, MessageAck, ValueTask> messageAckCallback)
            : base(logger, subscriptionConfig, messageAckCallback)
        {
            _messageHandler = messageHandler;
        }

        protected override async ValueTask HandleMessageAsync(MessagePacket msg)
        {
            await _messageHandler(PackMsgContent(msg));
            await _messageAckCallback(_subscriptionConfig, msg, MessageAck.Ack);
        }
    }
}
