using DotNetty.Codecs.STAN.Packets;
using DotNetty.Transport.Channels;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace Hunter.STAN.Client
{
    public class SubscriptionMessageAsynHandler : SubscriptionMessageHandler
    {

        private readonly Func<STANMsgContent, ValueTask> _messageHandler;

        public SubscriptionMessageAsynHandler(
            ILogger logger,
            STANSubscriptionConfig subscriptionConfig,
            Func<STANMsgContent, ValueTask> messageHandler,
            Func<STANSubscriptionConfig, MsgProtoPacket, bool, Task> messageAckCallback)
            : base(logger, subscriptionConfig, messageAckCallback)
        {
            _messageHandler = messageHandler;
        }

        public SubscriptionMessageAsynHandler(
            ILogger logger,
            STANSubscriptionConfig subscriptionConfig,
            Func<STANMsgContent, ValueTask> messageHandler,
            Func<STANSubscriptionConfig, MsgProtoPacket, bool, Task> messageAckCallback,
            Func<STANSubscriptionConfig, Task> unSubscriptionCallback)
            : base(logger, subscriptionConfig, messageAckCallback, unSubscriptionCallback)
        {
            _messageHandler = messageHandler;
        }

        protected override void MessageHandler(MsgProtoPacket msg, Func<STANSubscriptionConfig, MsgProtoPacket, bool, Task> ackCallback)
        {
            Task.Factory.StartNew(async _msg =>
            {
                try
                {
                    var current_msg = _msg as MsgProtoPacket;
                    await _messageHandler(PackMsgContent(current_msg));
                    await ackCallback(_subscriptionConfig, current_msg, true);
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
