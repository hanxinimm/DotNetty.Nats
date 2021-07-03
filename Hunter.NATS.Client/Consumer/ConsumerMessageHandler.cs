//using DotNetty.Codecs.NATS.Packets;
//using DotNetty.Codecs.NATSJetStream.Protocol;
//using DotNetty.Handlers.NATS;
//using DotNetty.Transport.Channels;
//using Microsoft.Extensions.Logging;
//using System;
//using System.Collections.Concurrent;
//using System.Threading;
//using System.Threading.Tasks;

//namespace Hunter.NATS.Client
//{
//    public abstract class ConsumerMessageHandler : MessagePacketHandler
//    {
//        private int _messageHandlerCounter = 0;
//        private readonly ILogger _logger;
//        protected readonly ConsumerConfig _consumerConfig;
//        private readonly Func<ConsumerConfig, Task> _unConsumerCallback;
//        private readonly Action<IChannelHandlerContext, MessagePacket> _channelRead;
//        public ConsumerMessageHandler(
//            ILogger logger,
//            ConsumerConfig consumerConfig,
//            Func<ConsumerConfig, Task> unSubscriptionCallback = null)
//        {
//            _logger = logger;
//            _consumerConfig = consumerConfig;
//            _unConsumerCallback = unSubscriptionCallback;
//            if (consumerConfig.MaxMsg.HasValue)
//            {
//                _channelRead = LimitedMessageHandler;
//            }
//            else
//            {
//                _channelRead = EndlessMessageHandler;
//            }
//        }

//        public override bool IsSharable => true;

//        public NATSSubscriptionConfig SubscriptionConfig => _consumerConfig;

//        protected abstract void MessageHandler(MessagePacket msg);

//        protected override void ChannelRead0(IChannelHandlerContext contex, MessagePacket msg)
//        {
//            _channelRead(contex, msg);
//        }

//        private void EndlessMessageHandler(IChannelHandlerContext contex, MessagePacket msg)
//        {
//            if (msg.SubscribeId == _consumerConfig.SubscribeId)
//            {
//                try
//                {
//                    MessageHandler(msg);
//                }
//                catch (Exception ex)
//                {
//                    _logger.LogError(ex, $"[SubscriptionMessageHandler]消息处理发生异常 主题 {_consumerConfig.Subject}");
//                }
//            }
//            else
//            {
//                contex.FireChannelRead(msg);
//            }
//        }

//        protected NATSMsgContent PackMsgContent(MessagePacket msg)
//        {
//            return new NATSMsgContent()
//            {
//                SubscribeId = msg.SubscribeId,
//                Subject = msg.Subject,
//                ReplyTo = msg.ReplyTo,
//                Data = msg.Payload
//            };
//        }

//    }
//}
