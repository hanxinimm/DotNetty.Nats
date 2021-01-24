using DotNetty.Codecs.STAN.Packets;
using DotNetty.Handlers.STAN;
using DotNetty.Transport.Channels;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace Hunter.STAN.Client
{
    public abstract class SubscriptionMessageHandler : MessagePacketHandler
    {
        private int _messageHandlerCounter = 0;
        protected readonly ILogger _logger;
        protected readonly STANSubscriptionConfig _subscriptionConfig;
        protected readonly Func<STANSubscriptionConfig, MsgProtoPacket, bool, Task> _messageAckCallback;
        private readonly Func<STANSubscriptionConfig, Task> _unSubscriptionCallback;
        private readonly Action<IChannelHandlerContext, MsgProtoPacket> _channelRead;
        public SubscriptionMessageHandler(
            ILogger logger,
            STANSubscriptionConfig subscriptionConfig,
            Func<STANSubscriptionConfig, MsgProtoPacket, bool, Task> messageAckCallback,
            Func<STANSubscriptionConfig, Task> unSubscriptionCallback = null)
        {
            _logger = logger;
            _subscriptionConfig = subscriptionConfig;
            _messageAckCallback = messageAckCallback;
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

        public STANSubscriptionConfig SubscriptionConfig => _subscriptionConfig;

        protected abstract void MessageHandler(MsgProtoPacket msg, Func<STANSubscriptionConfig, MsgProtoPacket, bool, Task> ackCallback);

        protected override void ChannelRead0(IChannelHandlerContext contex, MsgProtoPacket msg)
        {
            _channelRead(contex, msg);
        }

        private void EndlessMessageHandler(IChannelHandlerContext contex, MsgProtoPacket msg)
        {
            if (msg.Subject == _subscriptionConfig.Inbox &&
                msg.Message.Subject == _subscriptionConfig.Subject)
            {
                try
                {
                    MessageHandler(msg, _messageAckCallback);
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

        private void LimitedMessageHandler(IChannelHandlerContext contex, MsgProtoPacket msg)
        {
            if (msg.Subject == _subscriptionConfig.Inbox &&
                msg.Subject == _subscriptionConfig.Subject)
            {
                if (_messageHandlerCounter < _subscriptionConfig.MaxMsg)
                {
                    try
                    {
                        MessageHandler(msg, _messageAckCallback);
                        _messageHandlerCounter++;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"[SubscriptionMessageHandler]消息处理发生异常 主题 {_subscriptionConfig.Subject}");
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

        protected STANMsgContent PackMsgContent(MsgProtoPacket msg)
        {
            return new STANMsgContent()
            {
                Sequence = msg.Message.Sequence,
                Subject = msg.Message.Subject,
                Reply = msg.Message.Reply,
                Data = msg.Message.Data.ToByteArray(),
                Timestamp = msg.Message.Timestamp,
                Redelivered = msg.Message.Redelivered,
                CRC32 = msg.Message.CRC32
            };
        }

    }
}
