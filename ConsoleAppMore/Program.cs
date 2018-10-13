

using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DotNetty.Buffers;
using DotNetty.Codecs;
using DotNetty.Codecs.STAN;
using DotNetty.Codecs.STAN.Packets;
using DotNetty.Handlers.NATS;
//using DotNetty.Handlers.Tls;
using DotNetty.Transport.Bootstrapping;
using DotNetty.Transport.Channels;
using DotNetty.Transport.Channels.Sockets;

namespace ConsoleAppMore
{
    class Program
    {
        private static readonly Regex _subjectRegex = new Regex(@"^[a-zA-Z\d]+(\.(\*|\>|[a-zA-Z\d]+))*$", RegexOptions.Compiled);

        static async Task RunClientAsync()
        {
            Console.WriteLine("订阅程序");

            //ExampleHelper.SetConsoleLogger();

            //Console.WriteLine("foo.> {0}", _subjectRegex.IsMatch("foo.>"));
            //Console.WriteLine("BAR {0}", _subjectRegex.IsMatch("BAR"));
            //Console.WriteLine("foo.bar {0}", _subjectRegex.IsMatch("foo.bar"));
            //Console.WriteLine("foo.BAR {0}", _subjectRegex.IsMatch("foo.BAR"));
            //Console.WriteLine("FOO.BAR {0}", _subjectRegex.IsMatch("FOO.BAR"));



            //Console.WriteLine("foo.*.baz {0}", _subjectRegex.IsMatch("foo.*.baz"));
            //Console.WriteLine("foo*.bar {0}", _subjectRegex.IsMatch("foo*.bar"));
            //Console.WriteLine("f*o.b*r {0}", _subjectRegex.IsMatch("f*o.b*r"));
            //Console.WriteLine("foo> {0}", _subjectRegex.IsMatch("foo>"));

            //return;

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

                        channel.Pipeline.AddLast(new DelimiterBasedFrameDecoder(1048576, Delimiters.LineDelimiter()));
                        channel.Pipeline.AddLast(NATSEncoder.Instance, new NATSDecoder(true, 20480));
                        channel.Pipeline.AddLast(new PingPacketHandler(), new PongPacketHandler(), new OKPacketHandler(), new MessagePacketHandler(),new InfoPacketHandler(), new ErrorPacketHandler());
                    }));

                IChannel bootstrapChannel = await bootstrap.ConnectAsync(new IPEndPoint(IPAddress.Parse("192.168.0.12"), 4222));

                //await bootstrapChannel.WriteAndFlushAsync(new ConnectPacket(false, false, false, null, null, "test-client", null));

                await bootstrapChannel.WriteAndFlushAsync(new PingPacket());

                for (; ; )
                {
                    string line = Console.ReadLine();
                    if (string.IsNullOrEmpty(line))
                    {
                        Stopwatch sw = new Stopwatch();
                        sw.Start();
                        int j = 1;
                        for (int i = 0; i < j; i++)
                        {
                            //await bootstrapChannel.WriteAndFlushAsync(string.Format("PUB foo {1}\r\n{0}\r\n", json, bytes.Length));
                            //await bootstrapChannel.WriteAndFlushAsync("hello" + "\r\n");
                            var packet = new SubscribePacket("test", "foo", string.Empty);
                            //var packet = new PublishPacket("foo", Unpooled.WrappedBuffer(bytes));
                            await bootstrapChannel.WriteAndFlushAsync(packet);
                        }

                        sw.Stop();

                        Console.WriteLine("{0} 条消息已经发送完毕,耗时 {1} 毫秒,请输入消息", j, sw.ElapsedMilliseconds);

                        continue;
                    }

                    try
                    {
                        await bootstrapChannel.WriteAndFlushAsync(new UnSubscribePacket("test1"));

                        Console.WriteLine("取消订阅");
                    }
                    catch
                    {
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

        static void Main() => RunClientAsync().Wait();
    }
}
