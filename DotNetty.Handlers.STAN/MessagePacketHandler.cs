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
        protected override void ChannelRead0(IChannelHandlerContext contex, MsgProtoPacket msg)
        {
            contex.FireChannelRead(msg);
            //Console.WriteLine("收到消息 主题 {0}  订阅唯一编号{1} 第 {2} 条", msg.Subject, msg.Message.Subject, Interlocked.Increment( ref MessageCount));
        }

        public override Task WriteAsync(IChannelHandlerContext context, object message)
        {
            if (message is ConnectRequestPacket)
            {
                
            }
            return base.WriteAsync(context, message);
        }

        public override void ExceptionCaught(IChannelHandlerContext contex, Exception e)
        {
            Console.WriteLine(DateTime.Now.Millisecond);
            Console.WriteLine("{0}", e.StackTrace);
            contex.CloseAsync();
        }
    }
}
