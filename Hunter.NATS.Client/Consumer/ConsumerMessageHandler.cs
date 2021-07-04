﻿using DotNetty.Codecs.NATS.Packets;
using DotNetty.Codecs.NATSJetStream.Protocol;
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
        private readonly ILogger _logger;
        protected readonly NATSConsumerSubscriptionConfig _subscriptionConfig;
        private readonly Action<IChannelHandlerContext, MessagePacket> _channelRead;
        public ConsumerMessageHandler(
            ILogger logger,
            NATSConsumerSubscriptionConfig subscriptionConfig)
        {
            _logger = logger;
            _subscriptionConfig = subscriptionConfig;
            _channelRead = MessageHandler;

        }

        public override bool IsSharable => true;

        protected abstract void MessageHandler(MessagePacket msg);

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
                    MessageHandler(msg);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"[SubscriptionMessageHandler]消息处理发生异常 消费者 {_subscriptionConfig.ConsumerName}");
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
