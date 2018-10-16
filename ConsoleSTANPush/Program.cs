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


                ManualResetEvent ConnectRequestCompleted = new ManualResetEvent(false);
                var Values = new ConcurrentDictionary<string, MessagePacket>();

                var bootstrap = new Bootstrap();
                bootstrap
                    .Group(group)
                    .Channel<TcpSocketChannel>()
                    .Option(ChannelOption.TcpNodelay, false)
                    .Handler(new ActionChannelInitializer<ISocketChannel>(channel =>
                    {
                        //IChannelPipeline pipeline = channel.Pipeline;

                        //if (cert != null)
                        //{
                        //    pipeline.AddLast(new TlsHandler(stream => new SslStream(stream, true, (sender, certificate, chain, errors) => true), new ClientTlsSettings(targetHost)));
                        //}
                        //channel.Pipeline.AddLast(new STANDelimiterBasedFrameDecoder(1024));
                        channel.Pipeline.AddLast(STANEncoder.Instance, new STANDecoder());
                        channel.Pipeline.AddLast(new InfoPacketHandler(),new ErrorPacketHandler(), new ConnectRequestPacketHandler(), new MessagePacketHandler(Values, ConnectRequestCompleted), new ConnectResponsePacketHandler());
                    }));

                IChannel bootstrapChannel = await bootstrap.ConnectAsync(new IPEndPoint(IPAddress.Parse("192.168.0.226"), 4222));

                await bootstrapChannel.WriteAndFlushAsync(new HeartbeatInboxPacket());



                string ReplyTo = $"{STANConstants.ConnectResponseInboxPrefix}{Guid.NewGuid().ToString("N")}";

                Console.WriteLine(ReplyTo);

                //侦听连接请求响应消息
                await bootstrapChannel.WriteAndFlushAsync(new SubscribePacket(DateTime.Now.Ticks.ToString(), $"{ReplyTo}.*"));

                var ConnectRequestReplyTo = $"{ReplyTo}.{DateTime.Now.Ticks}";

                Console.WriteLine(ConnectRequestReplyTo);

                //发布连接消息
                await bootstrapChannel.WriteAndFlushAsync(new ConnectRequestPacket("main-cluster", "appname-publisher", ConnectRequestReplyTo));

                ConnectRequestCompleted.WaitOne();


                var s = new ConnectResponse();

                s.MergeFrom(Values.Values.First().Payload);

                Values.Clear();

                ConnectRequestCompleted.Reset();


                string Inbox = $"{STANConstants.InboxPrefix}{Guid.NewGuid().ToString("N")}";

                //订阅侦听消息
                await bootstrapChannel.WriteAndFlushAsync(new SubscribePacket(DateTime.Now.Ticks.ToString(), Inbox));

                string SubscribeReplyTo = $"{STANConstants.InboxPrefix}{Guid.NewGuid().ToString("N")}";

                Console.WriteLine(SubscribeReplyTo);

                //侦听订阅请求响应消息
                await bootstrapChannel.WriteAndFlushAsync(new SubscribePacket(DateTime.Now.Ticks.ToString(), $"{SubscribeReplyTo}.*"));

                var SubscriptionRequestReplyTo = $"{ReplyTo}.{DateTime.Now.Ticks}";
                Console.WriteLine(SubscriptionRequestReplyTo);

                //发送订阅请求
                await bootstrapChannel.WriteAndFlushAsync(new SubscriptionRequestPacket(s.SubRequests, SubscriptionRequestReplyTo, "appname-publisher",
                    "foo", string.Empty, Inbox, 1024, 30, null, StartPosition.NewOnly));

                ConnectRequestCompleted.WaitOne();

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

        public async Task<IChannel> ContentcAsync(Bootstrap bootstrap)
        {

            IChannel bootstrapChannel = await bootstrap.ConnectAsync(new IPEndPoint(IPAddress.Parse("192.168.0.226"), 4222));

            await bootstrapChannel.WriteAndFlushAsync(new HeartbeatInboxPacket());



            string ReplyTo = $"{STANConstants.ConnectResponseInboxPrefix}{Guid.NewGuid().ToString("N")}";

            Console.WriteLine(ReplyTo);

            //侦听连接请求响应消息
            await bootstrapChannel.WriteAndFlushAsync(new SubscribePacket(DateTime.Now.Ticks.ToString(), $"{ReplyTo}.*"));

            var ConnectRequestReplyTo = $"{ReplyTo}.{DateTime.Now.Ticks}";

            Console.WriteLine(ConnectRequestReplyTo);

            //发布连接消息
            await bootstrapChannel.WriteAndFlushAsync(new ConnectRequestPacket("main-cluster", "appname-publisher", ConnectRequestReplyTo));

            ConnectRequestCompleted.WaitOne();

            return bootstrapChannel;
        }
    }
}
