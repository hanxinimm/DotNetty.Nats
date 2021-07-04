using DotNetty.Codecs.NATS.Packets;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace Hunter.NATS.Client
{
    public class ConsumerMessageAckAsynHandler : ConsumerMessageHandler
    {
        private readonly Func<NATSMsgContent, ValueTask<bool>> _messageHandler;

        public ConsumerMessageAckAsynHandler(
            ILogger logger,
            NATSConsumerSubscriptionConfig subscriptionConfig,
            Func<NATSMsgContent, ValueTask<bool>> messageHandler,
            Func<NATSConsumerSubscriptionConfig, MessagePacket, bool, Task> messageAckCallback)
            : base(logger, subscriptionConfig, messageAckCallback)
        {
            _messageHandler = messageHandler;
        }

        protected override void MessageHandler(MessagePacket msg, Func<NATSConsumerSubscriptionConfig, MessagePacket, bool, Task> ackCallback)
        {
            Task.Factory.StartNew(async _msg =>
            {
                try
                {
                    var current_msg = _msg as MessagePacket;
                    var isAck = await _messageHandler(PackMsgContent(current_msg));
                    await ackCallback(_subscriptionConfig, current_msg, isAck);
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
