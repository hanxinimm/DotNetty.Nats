using DotNetty.Codecs.STAN.Packets;
using DotNetty.Transport.Channels;
using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace Hunter.STAN.Client
{
    public class SubscriptionMessageAckAsynHandler : SubscriptionMessageHandler
    {
        private readonly Func<STANMsgContent, ValueTask<bool>> _messageHandler;

        public SubscriptionMessageAckAsynHandler(
            STANSubscriptionConfig subscriptionConfig,
            Func<STANMsgContent, ValueTask<bool>> messageHandler,
            Func<STANSubscriptionConfig, MsgProtoPacket, bool, Task> messageAckCallback)
            : base(subscriptionConfig, messageAckCallback)
        {
            _messageHandler = messageHandler;
        }

        public SubscriptionMessageAckAsynHandler(
            STANSubscriptionConfig subscriptionConfig,
            Func<STANMsgContent, ValueTask<bool>> messageHandler,
            Func<STANSubscriptionConfig, MsgProtoPacket, bool, Task> messageAckCallback,
            Func<STANSubscriptionConfig, Task> unSubscriptionCallback)
            : base(subscriptionConfig, messageAckCallback, unSubscriptionCallback)
        {
            _messageHandler = messageHandler;
        }


        protected override bool MessageHandler(MsgProtoPacket msg)
        {
            return _messageHandler(PackMsgContent(msg)).GetAwaiter().GetResult();
        }
    }
}
