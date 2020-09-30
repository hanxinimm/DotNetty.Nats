using DotNetty.Codecs.STAN.Packets;
using DotNetty.Transport.Channels;
using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace Hunter.STAN.Client
{
    public abstract class SubscriptionMessageHandler : SimpleChannelInboundHandler<MsgProtoPacket>
    {
        private int _messageHandlerCounter = 0;
        protected readonly STANSubscriptionConfig _subscriptionConfig;
        protected readonly Func<STANSubscriptionConfig, MsgProtoPacket, bool, Task> _messageAckCallback;
        private readonly Func<STANSubscriptionConfig, Task> _unSubscriptionCallback;
        private readonly Action<IChannelHandlerContext, MsgProtoPacket> _channelRead;
        public SubscriptionMessageHandler(
            STANSubscriptionConfig subscriptionConfig,
            Func<STANSubscriptionConfig, MsgProtoPacket, bool, Task> messageAckCallback,
            Func<STANSubscriptionConfig, Task> unSubscriptionCallback = null)
        {
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

        protected abstract bool MessageHandler(MsgProtoPacket msg);

        protected override void ChannelRead0(IChannelHandlerContext contex, MsgProtoPacket msg)
        {
            _channelRead(contex, msg);
        }

        private void EndlessMessageHandler(IChannelHandlerContext contex, MsgProtoPacket msg)
        {
            if (msg.Message.Subject == _subscriptionConfig.Subject)
            {
                try
                {
                    var isAck = MessageHandler(msg);
                    _messageAckCallback(_subscriptionConfig, msg, isAck).GetAwaiter().GetResult();
                }
                catch (Exception ex)
                { 
                    //TODO:缺少异常处理机制
                }
            }
            else
            {
                contex.FireChannelRead(msg);
            }
        }

        private void LimitedMessageHandler(IChannelHandlerContext contex, MsgProtoPacket msg)
        {
            if (msg.Subject == _subscriptionConfig.Subject)
            {
                if (_messageHandlerCounter < _subscriptionConfig.MaxMsg)
                {
                    try
                    {
                        var isAck = MessageHandler(msg);
                        _messageAckCallback(_subscriptionConfig, msg, isAck).GetAwaiter().GetResult();
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
