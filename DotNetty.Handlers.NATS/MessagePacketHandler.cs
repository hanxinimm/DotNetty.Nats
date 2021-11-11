using DotNetty.Codecs.NATS.Packets;
using DotNetty.Transport.Channels;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DotNetty.Handlers.NATS
{
    public abstract class MessagePacketHandler : SimpleChannelInboundHandler<MessagePacket>
    {
        private readonly EventWaitHandle _queueEventWaitHandle;
        public string SubscribeId { get; protected set; }
        protected ConcurrentQueue<MessagePacket> MessageQueues { get; }

        public MessagePacketHandler()
        {
            MessageQueues = new ConcurrentQueue<MessagePacket>();
            _queueEventWaitHandle = new EventWaitHandle(false, EventResetMode.AutoReset);
        }

        protected override void ChannelRead0(IChannelHandlerContext contex, MessagePacket msg)
        {
            MessageHandler(contex, msg);
        }

        private void MessageHandler(IChannelHandlerContext contex, MessagePacket msg)
        {
            if (msg.SubscribeId == SubscribeId)
            {
                MessageQueues.Enqueue(msg);
                _queueEventWaitHandle.Set();
            }
            else
            {
                contex.FireChannelRead(msg);
            }
        }

        protected abstract ValueTask HandleMessageAsync(MessagePacket msg);


        public void MessageProcessingAsync()
        {
            Task.Factory.StartNew(async () =>
            {
                while (true)
                {
                    while (MessageQueues.TryDequeue(out var packet))
                    {
                        await HandleMessageAsync(packet);
                    }

                    _queueEventWaitHandle.WaitOne(TimeSpan.FromMinutes(5));
                }
            }, TaskCreationOptions.LongRunning);
        }
    }
}
