using DotNetty.Codecs.NATS.Packets;
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
        private readonly Func<NATSMsgContent, ValueTask> _messageHandler;

        public ConsumerMessageAsynHandler(
            ILogger logger,
            NATSConsumerSubscriptionConfig subscriptionConfig,
            Func<NATSMsgContent, ValueTask> messageHandler,
            Func<NATSConsumerSubscriptionConfig, MessagePacket, bool, Task> messageAckCallback)
            : base(logger, subscriptionConfig, messageAckCallback)
        {
            _messageHandler = messageHandler;
        }

        protected override void MessageHandler(MessagePacket msg, Func<NATSConsumerSubscriptionConfig, MessagePacket, bool, Task> ackCallback)
        {
            Task.Factory.StartNew(async _msg =>
            {
                var current_msg = _msg as MessagePacket;
                await _messageHandler(PackMsgContent(current_msg));
                await ackCallback(_subscriptionConfig, current_msg, true);
            }, 
            msg, 
            default, 
            TaskCreationOptions.DenyChildAttach,
            TaskScheduler.Default);
        }
    }
}
