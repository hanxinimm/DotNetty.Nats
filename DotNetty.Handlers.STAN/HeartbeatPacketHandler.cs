using DotNetty.Codecs.STAN.Packets;
using DotNetty.Transport.Channels;
using System;

namespace DotNetty.Handlers.STAN
{
    public class HeartbeatPacketHandler : SimpleChannelInboundHandler<MsgProtoPacket>
    {
        private readonly string _heartbeatSubject;
        private readonly Action<MsgProtoPacket> _heartbeatACK;
        public HeartbeatPacketHandler(string heartbeatSubject, Action<MsgProtoPacket> heartbeatACK)
        {
            _heartbeatSubject = heartbeatSubject;
            _heartbeatACK = heartbeatACK;
        }

        protected override void ChannelRead0(IChannelHandlerContext contex, MsgProtoPacket msg)
        {
            if (msg.Subject == _heartbeatSubject)
            {
                _heartbeatACK(msg);
            }
            else
            {
                contex.FireChannelRead(msg);
            }
        }

        public override void ExceptionCaught(IChannelHandlerContext contex, Exception e)
        {
            Console.WriteLine(DateTime.Now.Millisecond);
            Console.WriteLine("{0}", e.StackTrace);
            contex.CloseAsync();
        }
    }
}
