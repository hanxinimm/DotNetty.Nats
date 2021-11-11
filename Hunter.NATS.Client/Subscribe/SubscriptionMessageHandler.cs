using DotNetty.Codecs.NATS.Packets;
using DotNetty.Handlers.NATS;
using DotNetty.Transport.Channels;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace Hunter.NATS.Client
{
    public abstract class SubscriptionMessageHandler : MessagePacketHandler
    {
        private int _messageHandlerCounter = 0;
        protected readonly NATSMsgSubscriptionConfig _subscriptionConfig;
        private readonly Func<NATSMsgSubscriptionConfig, Task> _unSubscriptionCallback;
        private readonly Func<NATSMsgContent,ValueTask> _messageHandler;
        public SubscriptionMessageHandler(
            ILogger logger,
            NATSMsgSubscriptionConfig subscriptionConfig,
            Func<NATSMsgSubscriptionConfig, Task> unSubscriptionCallback = null)
            : base(logger)
        {
            _subscriptionConfig = subscriptionConfig;
            _unSubscriptionCallback = unSubscriptionCallback;
            if (subscriptionConfig.MaxMsg.HasValue)
            {
                _messageHandler = LimitedMessageHandler;
            }
            else
            {
                _messageHandler = EndlessMessageHandler;
            }
        }

        public override bool IsSharable => true;

        public NATSMsgSubscriptionConfig SubscriptionConfig => _subscriptionConfig;

        protected override async ValueTask HandleMessageAsync(MessagePacket msg)
        {
            await _messageHandler(PackMsgContent(msg));
        }

        protected abstract ValueTask MessageHandler(NATSMsgContent msg);

        private async ValueTask EndlessMessageHandler(NATSMsgContent msg)
        {
            await MessageHandler(msg);
        }

        private async ValueTask LimitedMessageHandler(NATSMsgContent msg)
        {
            if (_messageHandlerCounter < _subscriptionConfig.MaxMsg)
            {
                await MessageHandler(msg);
                _messageHandlerCounter++;
            }
            else
            {
                await _unSubscriptionCallback(_subscriptionConfig);
            }
        }

        protected NATSMsgContent PackMsgContent(MessagePacket msg)
        {
            return new NATSMsgContent()
            {
                SubscribeId = msg.SubscribeId,
                Subject = msg.Subject,
                ReplyTo = msg.ReplyTo,
                Data = msg.Payload
            };
        }

    }
}
