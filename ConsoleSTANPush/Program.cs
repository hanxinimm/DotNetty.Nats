using DotNetty.Buffers;
using DotNetty.Codecs;
using DotNetty.Codecs.STAN;
using DotNetty.Codecs.STAN.Packets;
using DotNetty.Codecs.STAN.Protocol;
using DotNetty.Handlers.STAN;
using DotNetty.Transport.Bootstrapping;
using DotNetty.Transport.Channels;
using DotNetty.Transport.Channels.Sockets;
using System;
using System.Diagnostics;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Threading;
using System.Linq;
using Google.Protobuf;
using System.Text;

namespace ConsoleSTANPush
{
    class Program
    {
        static int MessageCount = 0;
        static string AckInbox = string.Empty;
        static string Subject = string.Empty;

        static async Task Main()
        {
            var group = new MultithreadEventLoopGroup(1);
            X509Certificate2 cert = null;
            string targetHost = null;

            try
            {
                var bootstrap = new Bootstrap();
                bootstrap
                    .Group(group)
                    .Channel<TcpSocketChannel>()
                    .Option(ChannelOption.TcpNodelay, false)
                    .Handler(new ActionChannelInitializer<ISocketChannel>(channel =>
                    {
                        channel.Pipeline.AddFirst(new STANDelimiterBasedFrameDecoder(4096));
                        channel.Pipeline.AddLast(STANEncoder.Instance, new STANDecoder());
                        channel.Pipeline.AddLast(new ErrorPacketHandler());
                        //channel.Pipeline.AddLast(new MessagePacketHandler(AckAsync));
                    }));


                IChannel bootstrapChannel = await bootstrap.ConnectAsync(new IPEndPoint(IPAddress.Parse("192.168.0.226"), 4222));

                //设置请求响应回复的收件箱
                string InboxId = Guid.NewGuid().ToString("N");

                await bootstrapChannel.WriteAndFlushAsync(new HeartbeatInboxPacket(InboxId));

                string ClientId = "TestClientId";

                //侦听连接请求响应消息
                await bootstrapChannel.WriteAndFlushAsync(new InboxPacket(DateTime.Now.Ticks.ToString(), InboxId));

                var spt = await ContentcAsync(bootstrapChannel, ClientId, InboxId);


                //Stopwatch stopwatch = new Stopwatch();
                //stopwatch.Start(); //  开始监视代码运行时间


                //Console.WriteLine("请输入要运行的模式");
                //string Code = Console.ReadLine();

                Console.WriteLine("请输入一条消息主题");
                Subject = Console.ReadLine();

                //if (Code == "1")
                //{
                //    var rps = await SubscriptionAsync(bootstrapChannel, ClientId, spt.Message, InboxId);

                //    AckInbox = rps.Message.AckInbox;

                //    //Console.WriteLine("收到消息确认 主题 {0}  第 {1} 条", rps.Subject, Interlocked.Increment(ref MessageCount));
                //}
                //else
                //{
                //var pps = await PublishAsync(bootstrapChannel, spt.Message, ClientId, InboxId);
                //}



                //stopwatch.Stop(); //  停止监视  

                ////TimeSpan timespan = stopwatch.Elapsed; //  获取当前实例测量得出的总时间  

                //Console.WriteLine("完成订阅" + stopwatch.ElapsedMilliseconds);

                //Console.ReadLine();


                await CloseRequestAsync(bootstrapChannel, spt.Message, ClientId, InboxId);

                Console.WriteLine("关闭中...");
                Console.ReadLine();

                await bootstrapChannel.CloseAsync();
            }
            catch (Exception ex)
            {

            }
            finally
            {
                group.ShutdownGracefullyAsync().Wait(1000);
            }
        }

        public static async Task<ConnectResponsePacket> ContentcAsync(IChannel bootstrapChannel, string clientId, string inboxId)
        {

            var Packet = new ConnectRequestPacket(inboxId, "main-cluster", clientId);

            var ConnectResponseReady = new TaskCompletionSource<ConnectResponsePacket>();

            var Handler = new ReplyPacketHandler<ConnectResponsePacket>(Packet.ReplyTo, ConnectResponseReady);

            bootstrapChannel.Pipeline.AddLast(Handler);

            //发布连接消息
            await bootstrapChannel.WriteAndFlushAsync(Packet);

            var Result = await ConnectResponseReady.Task;

            bootstrapChannel.Pipeline.Remove(Handler);

            return Result;
        }

        public static async Task<SubscriptionResponsePacket> SubscriptionAsync(IChannel bootstrapChannel, string clientId, ConnectResponse connectResponse,string inboxId)
        {
            var SubscribePacket = new SubscribePacket(DateTime.Now.Ticks.ToString());

            //订阅侦听消息
            await bootstrapChannel.WriteAndFlushAsync(SubscribePacket);

            var Packet = new SubscriptionRequestPacket(inboxId, connectResponse.SubRequests, clientId, "foo", string.Empty, SubscribePacket.Subject, 1024, 3, "KeepLast", StartPosition.LastReceived);

            var SubscriptionResponseReady = new TaskCompletionSource<SubscriptionResponsePacket>();

            var Handler = new ReplyPacketHandler<SubscriptionResponsePacket>(Packet.ReplyTo, SubscriptionResponseReady);

            bootstrapChannel.Pipeline.AddLast(Handler);

            //发送订阅请求
            await bootstrapChannel.WriteAndFlushAsync(Packet);

            var Result = await SubscriptionResponseReady.Task;

            bootstrapChannel.Pipeline.Remove(Handler);

            return Result;
        }

        public static async Task<PubAckPacket> PublishAsync(IChannel bootstrapChannel, ConnectResponse connectResponse, string clientId, string inboxId)
        {

            var PubAckReady = new TaskCompletionSource<PubAckPacket>();

            var Handler = new ReplyPacketHandler<PubAckPacket>(string.Empty, PubAckReady);

            bootstrapChannel.Pipeline.AddLast(Handler);


            for (int i = 0; i < 200; i++)
            {
                Console.WriteLine("这是一条测试数据 编号 " + i);

                var msgbytes = Encoding.UTF8.GetBytes("这是一条测试数据 编号 " + i);

                var Packet = new PubMsgPacket(inboxId, connectResponse.PubPrefix, clientId, Subject, msgbytes);

                //发送订阅请求
                await bootstrapChannel.WriteAndFlushAsync(Packet);

                Console.ReadLine();

            }
            var Result = await PubAckReady.Task;

            bootstrapChannel.Pipeline.Remove(Handler);

            return Result;

            return null;
        }

        public static Task AckAsync(IChannel bootstrapChannel, string subject, ulong sequence)
        {
            var Packet = new AckPacket(AckInbox, subject, sequence);

            //发送消息成功处理
            return bootstrapChannel.WriteAndFlushAsync(Packet);
        }

        public static async Task<CloseResponsePacket> CloseRequestAsync(IChannel bootstrapChannel, ConnectResponse connectResponse, string clientId, string inboxId)
        {

            var Packet = new CloseRequestPacket(inboxId, connectResponse.CloseRequests, clientId);

            var CloseRequestReady = new TaskCompletionSource<CloseResponsePacket>();

            var Handler = new ReplyPacketHandler<CloseResponsePacket>(Packet.ReplyTo, CloseRequestReady);

            bootstrapChannel.Pipeline.AddLast(Handler);

            //发送关闭
            await bootstrapChannel.WriteAndFlushAsync(Packet);

            var Result = await CloseRequestReady.Task;

            bootstrapChannel.Pipeline.Remove(Handler);

            return Result;
        }

    }

    public class STANConnect
    {
        public IChannel Channel;
    }
}
