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
        private readonly Queue<STANMsgContent> _messageQueue;
        private readonly TaskCompletionSource<Queue<STANMsgContent>> _messageTaskReady;
        private readonly Func<STANSubscriptionConfig, Task> _unSubscriptionCallback;
        private readonly CancellationTokenSource _messageCancellationTokenSource;
        public ReadMessageHandler(
            STANSubscriptionConfig subscriptionConfig,
            TaskCompletionSource<Queue<STANMsgContent>> messageTaskReady,
            Func<STANSubscriptionConfig, Task> unSubscriptionCallback = null)
        {
            _subscriptionConfig = subscriptionConfig;
            _messageQueue = new Queue<STANMsgContent>();
            _messageTaskReady = messageTaskReady;
            _unSubscriptionCallback = unSubscriptionCallback;

            _messageCancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(5));

            //设置任务超时时间-- 5秒钟超时
            _messageCancellationTokenSource.Token.Register(() => _messageTaskReady.TrySetResult(_messageQueue));
        }
        protected override void ChannelRead0(IChannelHandlerContext contex, MsgProtoPacket msg)
        {
            if (msg.Message.Subject == _subscriptionConfig.Subject)
            {
                if (_messageQueue.Count < _subscriptionConfig.MaxMsg)
                {
                    _messageQueue.Enqueue(PackMsgContent(msg));
                }

                if (_messageQueue.Count >= _subscriptionConfig.MaxMsg)
                {
                    _messageTaskReady.SetResult(_messageQueue);
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
