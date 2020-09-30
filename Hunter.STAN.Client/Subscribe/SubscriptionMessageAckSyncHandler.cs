using DotNetty.Codecs.STAN.Packets;
using DotNetty.Transport.Channels;
using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace Hunter.STAN.Client
{
    public class SubscriptionMessageAckSyncHandler : SubscriptionMessageHandler
    {
        private readonly Func<STANMsgContent, bool> _messageHandler;

        public SubscriptionMessageAckSyncHandler(
            STANSubscriptionConfig subscriptionConfig,
            Func<STANMsgContent, bool> messageHandler,
            Func<STANSubscriptionConfig, MsgProtoPacket, bool, Task> messageAckCallback)
            : base(subscriptionConfig, messageAckCallback)
        {
            _messageHandler = messageHandler;
        }

        public SubscriptionMessageAckSyncHandler(
            STANSubscriptionConfig subscriptionConfig,
            Func<STANMsgContent, bool> messageHandler,
            Func<STANSubscriptionConfig, MsgProtoPacket, bool, Task> messageAckCallback,
            Func<STANSubscriptionConfig, Task> unSubscriptionCallback)
            : base(subscriptionConfig, messageAckCallback, unSubscriptionCallback)
        {
            _messageHandler = messageHandler;
        }


        protected override bool MessageHandler(MsgProtoPacket msg)
        {
            return _messageHandler(PackMsgContent(msg));
        }
    }
}
