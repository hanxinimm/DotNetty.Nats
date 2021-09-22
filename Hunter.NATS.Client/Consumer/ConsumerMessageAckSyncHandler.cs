using DotNetty.Codecs.NATS.Packets;
using DotNetty.Codecs.Protocol;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace Hunter.NATS.Client
{
    public class ConsumerMessageAckSyncHandler : ConsumerMessageHandler
    {
        private readonly Func<NATSJetStreamMsgContent, MessageAck> _messageHandler;

        public ConsumerMessageAckSyncHandler(
            ILogger logger,
            NATSConsumerSubscriptionConfig subscriptionConfig,
            Func<NATSJetStreamMsgContent, MessageAck> messageHandler,
            Func<NATSConsumerSubscriptionConfig, MessagePacket, MessageAck, ValueTask> messageAckCallback)
            : base(logger, subscriptionConfig, messageAckCallback)
        {
            _messageHandler = messageHandler;
        }

        protected override void MessageHandler(MessagePacket msg, Func<NATSConsumerSubscriptionConfig, MessagePacket, MessageAck, ValueTask> ackCallback)
        {
            Task.Factory.StartNew(async _msg =>
            {
                try
                {
                    var current_msg = _msg as MessagePacket;
                    var ack_msg = _messageHandler(PackMsgContent(current_msg));
                    await ackCallback(_subscriptionConfig, current_msg, ack_msg);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "消息处理发生异常");
                }
            },
            msg,
            default,
            TaskCreationOptions.DenyChildAttach,
            TaskScheduler.Default);
        }
    }
}
