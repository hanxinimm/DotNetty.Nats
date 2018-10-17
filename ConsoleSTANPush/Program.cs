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
        static async Task Main()
        {

            var group = new MultithreadEventLoopGroup(1);
            X509Certificate2 cert = null;
            string targetHost = null;
            //if (ClientSettings.IsSsl)
            //{
            //    cert = new X509Certificate2(Path.Combine(ExampleHelper.ProcessDirectory, "dotnetty.com.pfx"), "password");
            //    targetHost = cert.GetNameInfo(X509NameType.DnsName, false);
            //}
            try
            {


                //ManualResetEvent ConnectRequestCompleted = new ManualResetEvent(false);

                var bootstrap = new Bootstrap();
                bootstrap
                    .Group(group)
                    .Channel<TcpSocketChannel>()
                    .Option(ChannelOption.TcpNodelay, false)
                    .Handler(new ActionChannelInitializer<ISocketChannel>(channel =>
                    {
                        channel.Pipeline.AddFirst(new STANDelimiterBasedFrameDecoder(40960));
                        channel.Pipeline.AddLast(STANEncoder.Instance, new STANDecoder());
                        channel.Pipeline.AddLast(new ErrorPacketHandler());
                        channel.Pipeline.AddLast(new MessagePacketHandler());
                    }));


                IChannel bootstrapChannel = await bootstrap.ConnectAsync(new IPEndPoint(IPAddress.Parse("192.168.0.226"), 4222));

                await bootstrapChannel.WriteAndFlushAsync(new HeartbeatInboxPacket());

                //设置请求响应回复的收件箱
                string InboxId = Guid.NewGuid().ToString("N");

                //侦听连接请求响应消息
                await bootstrapChannel.WriteAndFlushAsync(new InboxPacket(DateTime.Now.Ticks.ToString(), InboxId));

                var spt = await ContentcAsync(bootstrapChannel, InboxId);

                var msgbytes = Encoding.UTF8.GetBytes("这是一条测试数据");

                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start(); //  开始监视代码运行时间

                for (int i = 0; i < 100; i++)
                {
                    await PublishAsync(bootstrapChannel, spt.Message, InboxId, msgbytes);
                }

                stopwatch.Stop(); //  停止监视  

                TimeSpan timespan = stopwatch.Elapsed; //  获取当前实例测量得出的总时间  

                Console.WriteLine("完成发布" + timespan.Milliseconds);

                return;

                for (; ; )
                {
                    Console.WriteLine("请输入任意字符");
                    string line = Console.ReadLine();



                    if (string.IsNullOrEmpty(line))
                    {
                        string json = Newtonsoft.Json.JsonConvert.SerializeObject(new Order()
                        {
                            AccountId = 1000,
                            PeriodNo = 23408,
                            OrderNo = "14586413134678743131347",
                            Amount = 23034.45M,
                            BetCount = 230,
                            MoneyUnit = 123,
                            TotalAmount = 2313,
                            Content = "231313123131313",
                            GameId = 233131313,
                            PlayItemId = 233131313,
                            CreateTime = DateTime.Now
                        });

                        var bytes = System.Text.Encoding.UTF8.GetBytes(json);

                        Stopwatch sw = new Stopwatch();
                        sw.Start();
                        int j = 20;

                        Console.WriteLine("开发发送");

                        for (int i = 0; i < j; i++)
                        {
                            //await bootstrapChannel.WriteAndFlushAsync(string.Format("PUB foo {1}\r\n{0}\r\n", json, bytes.Length));
                            //await bootstrapChannel.WriteAndFlushAsync("hello" + "\r\n");
                            //var packet = new SubscribePacket("test1", "foo." + Guid.NewGuid(), string.Empty);
                            //var packet = new ("foo", Unpooled.WrappedBuffer(bytes));
                            //await bootstrapChannel.WriteAndFlushAsync(packet);
                        }

                        sw.Stop();

                        Console.WriteLine("{0} 条消息已经发送完毕,耗时 {1} 毫秒,请输入消息", j, sw.ElapsedMilliseconds);

                        continue;
                    }
                }

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

        public static async Task<ConnectResponsePacket> ContentcAsync(IChannel bootstrapChannel, string inboxId)
        {

            var Packet = new ConnectRequestPacket(inboxId, "main-cluster", "appname-publisher");

            var ConnectResponseReady = new TaskCompletionSource<ConnectResponsePacket>();

            var Handler = new ReplyPacketHandler<ConnectResponsePacket>(Packet.ReplyTo, ConnectResponseReady);

            bootstrapChannel.Pipeline.AddLast(Handler);

            //发布连接消息
            await bootstrapChannel.WriteAndFlushAsync(Packet);

            var Result = await ConnectResponseReady.Task;

            bootstrapChannel.Pipeline.Remove(Handler);

            return Result;
        }

        public static async Task<SubscriptionResponsePacket> SubscriptionAsync(IChannel bootstrapChannel, ConnectResponse connectResponse,string inboxId)
        {
            var SubscribePacket = new SubscribePacket(DateTime.Now.Ticks.ToString());

            //订阅侦听消息
            await bootstrapChannel.WriteAndFlushAsync(SubscribePacket);

            var Packet = new SubscriptionRequestPacket(inboxId, connectResponse.SubRequests,  "appname-publisher",
                "foo", string.Empty, SubscribePacket.Subject, 1024, 30, null, StartPosition.NewOnly);

            var SubscriptionResponseReady = new TaskCompletionSource<SubscriptionResponsePacket>();

            var Handler = new ReplyPacketHandler<SubscriptionResponsePacket>(Packet.ReplyTo, SubscriptionResponseReady);

            bootstrapChannel.Pipeline.AddLast(Handler);

            //发送订阅请求
            await bootstrapChannel.WriteAndFlushAsync(Packet);

            var Result = await SubscriptionResponseReady.Task;

            bootstrapChannel.Pipeline.Remove(Handler);

            return Result;
        }

        public static async Task<PubAckPacket> PublishAsync(IChannel bootstrapChannel, ConnectResponse connectResponse, string inboxId, byte[] data)
        {

            var Packet = new PubMsgPacket(inboxId, connectResponse.PubPrefix, "appname-publisher", "foo", data);

            var PubAckReady = new TaskCompletionSource<PubAckPacket>();

            var Handler = new ReplyPacketHandler<PubAckPacket>(Packet.ReplyTo, PubAckReady);

            bootstrapChannel.Pipeline.AddLast(Handler);

            //发送订阅请求
            await bootstrapChannel.WriteAndFlushAsync(Packet);

            var Result = await PubAckReady.Task;

            bootstrapChannel.Pipeline.Remove(Handler);

            return Result;

            return null;
        }

    }

    public class STANConnect
    {
        public IChannel Channel;
    }
}
