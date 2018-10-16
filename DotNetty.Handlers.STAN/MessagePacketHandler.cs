using DotNetty.Codecs.STAN.Packets;
using DotNetty.Transport.Channels;
using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace DotNetty.Handlers.STAN
{
    public class MessagePacketHandler : SimpleChannelInboundHandler<MessagePacket>
    {
        ManualResetEvent ConnectRequestCompleted;
        readonly ConcurrentDictionary<string, MessagePacket> MessageBoxs;

        public MessagePacketHandler(ConcurrentDictionary<string, MessagePacket> messageBoxs, ManualResetEvent connectRequestCompleted)
        {
            MessageBoxs = messageBoxs;
            ConnectRequestCompleted = connectRequestCompleted;
        }

        protected override void ChannelRead0(IChannelHandlerContext contex, MessagePacket msg)
        {
            MessageBoxs.AddOrUpdate(msg.Subject, msg, (k, v) => msg);
            ConnectRequestCompleted.Set();
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
