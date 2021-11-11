using DotNetty.Codecs.NATS.Packets;
using DotNetty.Codecs.Protocol;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace Hunter.NATS.Client
{
    public class ConsumerMessageAckAsynHandler : ConsumerMessageHandler
    {
        private readonly Func<NATSJetStreamMsgContent, ValueTask<MessageAck>> _messageHandler;

        public ConsumerMessageAckAsynHandler(
            ILogger logger,
            NATSConsumerSubscriptionConfig subscriptionConfig,
            Func<NATSJetStreamMsgContent, ValueTask<MessageAck>> messageHandler,
            Func<NATSConsumerSubscriptionConfig, MessagePacket, MessageAck, ValueTask> messageAckCallback)
            : base(logger, subscriptionConfig, messageAckCallback)
        {
            _messageHandler = messageHandler;
        }

        protected override async ValueTask HandleMessageAsync(MessagePacket msg)
        {
            var ack_msg = await _messageHandler(PackMsgContent(msg));
            await _messageAckCallback(_subscriptionConfig, msg, ack_msg);
        }
    }
}
