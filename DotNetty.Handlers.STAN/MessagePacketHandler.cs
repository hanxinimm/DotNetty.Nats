using DotNetty.Codecs.STAN.Packets;
using DotNetty.Transport.Channels;
using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace DotNetty.Handlers.STAN
{
    public class MessagePacketHandler : SimpleChannelInboundHandler<MsgProtoPacket>
    {
        private readonly Action<IChannelHandlerContext, MsgProtoPacket> _messageAckCallback;

        public MessagePacketHandler(Action<IChannelHandlerContext, MsgProtoPacket> messageAckCallback)
        {
            _messageAckCallback = messageAckCallback;
        }


        protected override void ChannelRead0(IChannelHandlerContext contex, MsgProtoPacket msg)
        {
            _messageAckCallback(contex, msg);
        }
    }
}
