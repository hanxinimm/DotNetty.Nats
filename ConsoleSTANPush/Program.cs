using DotNetty.Buffers;
using DotNetty.Codecs;
using DotNetty.Codecs.STAN;
using DotNetty.Codecs.STAN.Packets;
using DotNetty.Handlers.STAN;
using DotNetty.Transport.Bootstrapping;
using DotNetty.Transport.Channels;
using DotNetty.Transport.Channels.Sockets;
using System;
using System.Diagnostics;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

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

                        channel.Pipeline.AddLast(new DelimiterBasedFrameDecoder(1024, Delimiters.LineDelimiter()));
                        channel.Pipeline.AddLast(STANEncoder.Instance, new STANDecoder());
                        channel.Pipeline.AddLast(new InfoPacketHandler(), new MsgProtoPacketHandler(), new ConnectResponsePacketHandler());
                    }));

                IChannel bootstrapChannel = await bootstrap.ConnectAsync(new IPEndPoint(IPAddress.Parse("192.168.0.226"), 4222));

                await bootstrapChannel.WriteAndFlushAsync(new HeartbeatInboxPacket());

                string ReplyTo = $"{STANConstants.InboxPrefix}{Guid.NewGuid().ToString("N")}";

                await bootstrapChannel.WriteAndFlushAsync(new SubscribePacket(DateTime.Now.Ticks.ToString(), $"{ReplyTo}.*"));
                await bootstrapChannel.WriteAndFlushAsync(new SubscribePacket(DateTime.Now.Ticks.ToString(), $"{ReplyTo}.*"));

                //await bootstrapChannel.WriteAndFlushAsync(new ConnectRequestPacket("main-cluster", "appname-publisher", $"{ReplyTo}.{DateTime.Now.Ticks}"));

                //await bootstrapChannel.WriteAndFlushAsync(new SubscriptionRequestPacket("appname-publisher",
                //    "foo." + Guid.NewGuid(), string.Empty, "_INBOX." + Guid.NewGuid().ToString(), 1024, 30, null, StartPosition.NewOnly));

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
    }
}
