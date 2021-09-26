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
        protected readonly ILogger _logger;
        protected readonly NATSConsumerSubscriptionConfig _subscriptionConfig;
        protected readonly Func<NATSConsumerSubscriptionConfig, MessagePacket, MessageAck, ValueTask> _messageAckCallback;
        private readonly Action<IChannelHandlerContext, MessagePacket> _channelRead;
        public ConsumerMessageHandler(
            ILogger logger,
            NATSConsumerSubscriptionConfig subscriptionConfig,
            Func<NATSConsumerSubscriptionConfig, MessagePacket, MessageAck, ValueTask> messageAckCallback)
        {
            _logger = logger;
            _subscriptionConfig = subscriptionConfig;
            _messageAckCallback = messageAckCallback;
            _channelRead = MessageHandler;
        }

        public override bool IsSharable => true;

        public NATSConsumerSubscriptionConfig SubscriptionConfig => _subscriptionConfig;

        protected abstract void MessageHandler(MessagePacket msg, Func<NATSConsumerSubscriptionConfig, MessagePacket, MessageAck, ValueTask> ackCallback);

        protected override void ChannelRead0(IChannelHandlerContext contex, MessagePacket msg)
        {
            _channelRead(contex, msg);
        }

        private void MessageHandler(IChannelHandlerContext contex, MessagePacket msg)
        {
            if (msg.SubscribeId == _subscriptionConfig.SubscribeId)
            {
                try
                {
                    MessageHandler(msg, _messageAckCallback);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"[SubscriptionMessageHandler]消息处理发生异常 消费者 {_subscriptionConfig.Config.DurableName}");
                }
            }
            else
            {
                contex.FireChannelRead(msg);
            }
        }

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
