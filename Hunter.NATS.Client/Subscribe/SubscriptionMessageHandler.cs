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
        private readonly ILogger _logger;
        protected readonly NATSSubscriptionConfig _subscriptionConfig;
        private readonly Func<NATSSubscriptionConfig, Task> _unSubscriptionCallback;
        private readonly Action<IChannelHandlerContext, MessagePacket> _channelRead;
        public SubscriptionMessageHandler(
            ILogger logger,
            NATSSubscriptionConfig subscriptionConfig,
            Func<NATSSubscriptionConfig, Task> unSubscriptionCallback = null)
        {
            _logger = logger;
            _subscriptionConfig = subscriptionConfig;
            _unSubscriptionCallback = unSubscriptionCallback;
            if (subscriptionConfig.MaxMsg.HasValue)
            {
                _channelRead = LimitedMessageHandler;
            }
            else
            {
                _channelRead = EndlessMessageHandler;
            }
        }

        public override bool IsSharable => true;

        public NATSSubscriptionConfig SubscriptionConfig => _subscriptionConfig;

        protected abstract void MessageHandler(MessagePacket msg);

        protected override void ChannelRead0(IChannelHandlerContext contex, MessagePacket msg)
        {
            _channelRead(contex, msg);
        }

        private void EndlessMessageHandler(IChannelHandlerContext contex, MessagePacket msg)
        {
            if (msg.SubscribeId == _subscriptionConfig.SubscribeId)
            {
                try
                {
                    MessageHandler(msg);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"[SubscriptionMessageHandler]消息处理发生异常 主题 {_subscriptionConfig.Subject}");
                }
            }
            else
            {
                contex.FireChannelRead(msg);
            }
        }

        private void LimitedMessageHandler(IChannelHandlerContext contex, MessagePacket msg)
        {
            if (msg.SubscribeId == _subscriptionConfig.SubscribeId)
            {
                if (_messageHandlerCounter < _subscriptionConfig.MaxMsg)
                {
                    try
                    {
                        MessageHandler(msg);
                        _messageHandlerCounter++;
                    }
                    catch (Exception ex)
                    {

                    }
                }
                else
                {
                    _unSubscriptionCallback(_subscriptionConfig);
                }
            }
            else
            {
                contex.FireChannelRead(msg);
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
