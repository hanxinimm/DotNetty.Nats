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
        private readonly Action<NATSMsgContent> _messageHandler;

        public ConsumerMessageSyncHandler(
            ILogger logger,
            NATSConsumerSubscriptionConfig subscriptionConfig,
            Action<NATSMsgContent> messageHandler,
            Func<NATSConsumerSubscriptionConfig, MessagePacket, MessageAck, Task> messageAckCallback)
            : base(logger, subscriptionConfig, messageAckCallback)
        {
            _messageHandler = messageHandler;
        }

        protected override void MessageHandler(MessagePacket msg, Func<NATSConsumerSubscriptionConfig, MessagePacket, MessageAck, Task> ackCallback)
        {
            Task.Factory.StartNew(async _msg =>
            {
                var current_msg = _msg as MessagePacket;
                _messageHandler(PackMsgContent(current_msg));
                await ackCallback(_subscriptionConfig, current_msg, MessageAck.Ack);
            },
            msg,
            default,
            TaskCreationOptions.DenyChildAttach,
            TaskScheduler.Default);
        }
    }
}
