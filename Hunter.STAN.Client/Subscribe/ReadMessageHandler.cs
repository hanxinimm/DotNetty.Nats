using DotNetty.Codecs.STAN.Packets;
using DotNetty.Transport.Channels;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Hunter.STAN.Client
{
    public class ReadMessageHandler : SimpleChannelInboundHandler<MsgProtoPacket>
    {
        private readonly STANSubscriptionConfig _subscriptionConfig;
        private readonly Queue<STANMsgContent> _messageContents;
        private readonly TaskCompletionSource<Queue<STANMsgContent>> _messageTaskReady;
        private readonly Func<STANSubscriptionConfig, Task> _unSubscriptionCallback;
        public ReadMessageHandler(
            STANSubscriptionConfig subscriptionConfig,
            TaskCompletionSource<Queue<STANMsgContent>> messageTaskReady,
            Queue<STANMsgContent> msgContents,
            Func<STANSubscriptionConfig, Task> unSubscriptionCallback = null)
        {
            _subscriptionConfig = subscriptionConfig;
            _messageContents = msgContents;
            _messageTaskReady = messageTaskReady;
            _unSubscriptionCallback = unSubscriptionCallback;
        }
        protected override void ChannelRead0(IChannelHandlerContext contex, MsgProtoPacket msg)
        {
            if (msg.Message.Subject == _subscriptionConfig.Subject)
            {
                if (_messageContents.Count < _subscriptionConfig.MaxMsg)
                {
                    _messageContents.Enqueue(PackMsgContent(msg));
                }

                if (_messageContents.Count >= _subscriptionConfig.MaxMsg)
                {
                    _messageTaskReady.TrySetResult(_messageContents);
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
