using DotNetty.Codecs.NATS.Packets;
using DotNetty.Transport.Channels;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace DotNetty.Handlers.NATS
{
    public class MessagePacketHandler : SimpleChannelInboundHandler<MessagePacket>
    {
        private readonly Action<MessagePacket> _messageCallback;
        public MessagePacketHandler(Action<MessagePacket> messageCallback)
        {
            _messageCallback = messageCallback;
        }

        protected override void ChannelRead0(IChannelHandlerContext contex, MessagePacket msg)
        {
            _messageCallback(msg);
        }

        public override void ExceptionCaught(IChannelHandlerContext contex, Exception e)
        {
            Console.WriteLine(DateTime.Now.Millisecond);
            Console.WriteLine("{0}", e.StackTrace);
            contex.CloseAsync();
        }
    }
}
