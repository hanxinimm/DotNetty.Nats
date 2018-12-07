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
        static int MessageCount = 0;
        private Func<IChannel, string, ulong,Task> _callback;

        public MessagePacketHandler(Func<IChannel, string, ulong, Task> callback)
        {
            _callback = callback;
        }


        protected override void ChannelRead0(IChannelHandlerContext contex, MsgProtoPacket msg)
        {
            Console.WriteLine($"订阅编号{msg.SubscribeId}");
            //Console.WriteLine("收到消息 主题 {0} Timestamp {1}  Redelivered {2} Sequence {3} 第 {4} 条 事件  {5}",
            //    msg.Subject, msg.Message.Timestamp, msg.Message.Redelivered, msg.Message.Sequence, msg.Message.Data?.ToStringUtf8(), Interlocked.Increment(ref MessageCount));

            //_callback(contex.Channel, msg.Message.Subject, msg.Message.Sequence);
        }

        public override void ExceptionCaught(IChannelHandlerContext contex, Exception e)
        {
            Console.WriteLine(DateTime.Now.Millisecond);
            Console.WriteLine("{0}", e.StackTrace);
            contex.CloseAsync();
        }
    }
}
