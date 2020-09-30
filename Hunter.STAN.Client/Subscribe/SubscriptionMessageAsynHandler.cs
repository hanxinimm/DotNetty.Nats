using DotNetty.Codecs.STAN.Packets;
using DotNetty.Transport.Channels;
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
            STANSubscriptionConfig subscriptionConfig,
            Func<STANMsgContent, ValueTask> messageHandler,
            Func<STANSubscriptionConfig, MsgProtoPacket, bool, Task> messageAckCallback)
            : base(subscriptionConfig, messageAckCallback)
        {
            _messageHandler = messageHandler;
        }

        public SubscriptionMessageAsynHandler(
            STANSubscriptionConfig subscriptionConfig,
            Func<STANMsgContent, ValueTask> messageHandler,
            Func<STANSubscriptionConfig, MsgProtoPacket, bool, Task> messageAckCallback,
            Func<STANSubscriptionConfig, Task> unSubscriptionCallback)
            : base(subscriptionConfig, messageAckCallback, unSubscriptionCallback)
        {
            _messageHandler = messageHandler;
        }

        protected override bool MessageHandler(MsgProtoPacket msg)
        {
            _messageHandler(PackMsgContent(msg)).GetAwaiter().GetResult();

            return true;
        }
    }
}
