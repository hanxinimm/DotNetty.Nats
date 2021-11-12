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
        protected readonly ILogger _logger;

        public string SubscribeId { get; protected set; }
        protected ConcurrentQueue<MessagePacket> MessageQueues { get; }

        public MessagePacketHandler(ILogger logger)
        {
            _logger = logger;
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


        public Task MessageProcessingAsync()
        {
            return Task.Factory.StartNew(() =>
            {
                return Task.Factory.StartNew(async () =>
                {
                    while (true)
                    {
                        try
                        {
                            while (MessageQueues.TryDequeue(out var packet))
                            {
                                await HandleMessageAsync(packet);
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, $"[SubscriptionMessageHandler]消息处理发生异常 主题 {SubscribeId}");
                        }

                        _queueEventWaitHandle.WaitOne(TimeSpan.FromMinutes(5));
                    }
                }, TaskCreationOptions.LongRunning);
            });
        }
    }
}
