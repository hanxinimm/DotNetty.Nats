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

            var group = new MultithreadEventLoopGroup(12);
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
                var Values = new ConcurrentDictionary<string, MessagePacket>();

                var bootstrap = new Bootstrap();
                bootstrap
                    .Group(group)
                    .Channel<TcpSocketChannel>()
                    .Option(ChannelOption.TcpNodelay, false)
                    .Handler(new ActionChannelInitializer<ISocketChannel>(channel =>
                    {
                        channel.Pipeline.AddFirst(STANEncoder.Instance, new STANDecoder());
                        channel.Pipeline.AddLast(new InfoPacketHandler(),new ErrorPacketHandler(), new ConnectRequestPacketHandler());
                        channel.Pipeline.AddLast(new MessagePacketHandler(Values));
                    }));


                IChannel bootstrapChannel = await bootstrap.ConnectAsync(new IPEndPoint(IPAddress.Parse("192.168.0.226"), 4222));

                await bootstrapChannel.WriteAndFlushAsync(new HeartbeatInboxPacket());

                //设置请求响应回复的收件箱前缀
                string ReplyMessageInbox = $"{STANConstants.InboxPrefix}{Guid.NewGuid().ToString("N")}";

                //侦听连接请求响应消息
                await bootstrapChannel.WriteAndFlushAsync(new SubscribePacket(DateTime.Now.Ticks.ToString(), $"{ReplyMessageInbox}.*"));

                var spt = await ContentcAsync(bootstrapChannel, ReplyMessageInbox);

                var subrt = await SubscriptionAsync(bootstrapChannel, spt.Message, ReplyMessageInbox);

                Console.WriteLine("完成订阅");

                return;



                //IChannel bootstrapChannel = await bootstrap.ConnectAsync(new IPEndPoint(IPAddress.Parse("192.168.0.226"), 4222));

                await bootstrapChannel.WriteAndFlushAsync(new HeartbeatInboxPacket());



                string ReplyTo = $"{STANConstants.ConnectResponseInboxPrefix}{Guid.NewGuid().ToString("N")}";

                Console.WriteLine(ReplyTo);

                //侦听连接请求响应消息
                await bootstrapChannel.WriteAndFlushAsync(new SubscribePacket(DateTime.Now.Ticks.ToString(), $"{ReplyTo}.*"));

                var ConnectRequestReplyTo = $"{ReplyTo}.{DateTime.Now.Ticks}";

                Console.WriteLine(ConnectRequestReplyTo);

                //发布连接消息
                await bootstrapChannel.WriteAndFlushAsync(new ConnectRequestPacket("main-cluster", "appname-publisher", ConnectRequestReplyTo));

                //ConnectRequestCompleted.WaitOne();


                var s = new ConnectResponse();

                s.MergeFrom(Values.Values.First().Payload);

                Values.Clear();

                //ConnectRequestCompleted.Reset();


                //string Inbox = $"{STANConstants.InboxPrefix}{Guid.NewGuid().ToString("N")}";

                //订阅侦听消息
                await bootstrapChannel.WriteAndFlushAsync(new SubscribePacket(DateTime.Now.Ticks.ToString(), ReplyMessageInbox));

                string SubscribeReplyTo = $"{STANConstants.InboxPrefix}{Guid.NewGuid().ToString("N")}";

                Console.WriteLine(SubscribeReplyTo);

                //侦听订阅请求响应消息
                await bootstrapChannel.WriteAndFlushAsync(new SubscribePacket(DateTime.Now.Ticks.ToString(), $"{SubscribeReplyTo}.*"));

                var SubscriptionRequestReplyTo = $"{ReplyTo}.{DateTime.Now.Ticks}";
                Console.WriteLine(SubscriptionRequestReplyTo);

                //发送订阅请求
                await bootstrapChannel.WriteAndFlushAsync(new SubscriptionRequestPacket(s.SubRequests, SubscriptionRequestReplyTo, "appname-publisher",
                    "foo", string.Empty, ReplyMessageInbox, 1024, 30, null, StartPosition.NewOnly));

                //ConnectRequestCompleted.WaitOne();

                if (Values.Count > 0)
                {
                    var sr = new SubscriptionResponse();
                    sr.MergeFrom(Values.Values.First().Payload);
                    var bts = Values.Values.First().Payload;
                    var str = Encoding.UTF8.GetString(bts);
                }


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
                            var packet = new SubscribePacket("test1", "foo." + Guid.NewGuid(), string.Empty);
                            //var packet = new ("foo", Unpooled.WrappedBuffer(bytes));
                            await bootstrapChannel.WriteAndFlushAsync(packet);
                        }

                        sw.Stop();

                        Console.WriteLine("{0} 条消息已经发送完毕,耗时 {1} 毫秒,请输入消息", j, sw.ElapsedMilliseconds);

                        continue;
                    }
                }

                Console.ReadLine();

                await bootstrapChannel.CloseAsync();
            }
            finally
            {
                group.ShutdownGracefullyAsync().Wait(1000);
            }
        }

        public static async Task<ConnectResponsePacket> ContentcAsync(IChannel bootstrapChannel,string replyMessageInbox)
        {

            var ConnectRequestReplyTo = $"{replyMessageInbox}.{DateTime.Now.Ticks}";

            Console.WriteLine(ConnectRequestReplyTo);

            var RequestSubReady = new TaskCompletionSource<ConnectResponsePacket>();

            var Chal = new ConnectResponsePacketHandler(ConnectRequestReplyTo, RequestSubReady);

            bootstrapChannel.Pipeline.AddLast(Chal);

            //发布连接消息
            await bootstrapChannel.WriteAndFlushAsync(new ConnectRequestPacket("main-cluster", "appname-publisher", ConnectRequestReplyTo));

            var Result = await RequestSubReady.Task;

            bootstrapChannel.Pipeline.Remove(Chal);

            return Result;
        }

        public static async Task<SubscriptionResponsePacket> SubscriptionAsync(IChannel bootstrapChannel, ConnectResponse connectResponse,string replyMessageInbox)
        {
            string SubscribeMessageInbox = $"{STANConstants.InboxPrefix}{Guid.NewGuid().ToString("N")}";

            //订阅侦听消息
            await bootstrapChannel.WriteAndFlushAsync(new SubscribePacket(DateTime.Now.Ticks.ToString(), SubscribeMessageInbox));

            var SubscriptionRequestReplyTo = $"{replyMessageInbox}.{DateTime.Now.Ticks}";

            Console.WriteLine(SubscriptionRequestReplyTo);

            var SubscriptionRequestReady = new TaskCompletionSource<SubscriptionResponsePacket>();

            ////侦听订阅请求响应消息
            //await bootstrapChannel.WriteAndFlushAsync(new SubscribePacket(DateTime.Now.Ticks.ToString(), $"{SubscribeReplyTo}.*"));

            var Chal = new SubscriptionResponsePacketHandler(SubscriptionRequestReplyTo, SubscriptionRequestReady);

            bootstrapChannel.Pipeline.AddLast(Chal);

            //发送订阅请求
            await bootstrapChannel.WriteAndFlushAsync(new SubscriptionRequestPacket(connectResponse.SubRequests, SubscriptionRequestReplyTo, "appname-publisher",
                "foo", string.Empty, SubscribeMessageInbox, 1024, 30, null, StartPosition.NewOnly));

            var Result = await SubscriptionRequestReady.Task;

            bootstrapChannel.Pipeline.Remove(Chal);

            return Result;
        }

    }

    public class STANConnect
    {
        public IChannel Channel;
    }
}
