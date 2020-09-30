using DotNetty.Codecs.STAN.Packets;
using DotNetty.Handlers.STAN;
using DotNetty.Transport.Channels;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Hunter.STAN.Client
{
    public class SubscriptionResponseHandler : SubscriptionResponsePacketHandler
    {
        private readonly STANSubscriptionConfig _subscriptionConfig;
        private readonly TaskCompletionSource<SubscriptionResponsePacket> _subscriptionResponseReady;
        public SubscriptionResponseHandler(STANSubscriptionConfig subscriptionConfig,
            TaskCompletionSource<SubscriptionResponsePacket> subscriptionResponseReady = null)
        {
            _subscriptionConfig = subscriptionConfig;
            _subscriptionResponseReady = subscriptionResponseReady;

        }

        protected override void ChannelRead0(IChannelHandlerContext ctx, SubscriptionResponsePacket msg)
        {
            if (msg.Subject == _subscriptionConfig.ReplyTo)
            {
                if (string.IsNullOrEmpty(msg.Message.Error))
                {
                    _subscriptionConfig.AckInbox = msg.Message.AckInbox;
                }
                if (_subscriptionResponseReady != null)
                {
                    _subscriptionResponseReady.SetResult(msg);
                }
            }
        }
    }
}
