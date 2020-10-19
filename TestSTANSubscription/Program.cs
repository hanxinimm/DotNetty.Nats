using DotNetty.Codecs.STAN.Protocol;
using Hunter.STAN.Client;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace TestSTANSubscription
{
    class Program
    {
        public class TextA
        { 
            public string name { get; set; }

            public int? age { get; set; }
        }

        public class TestB : TextA
        { 
            
        }

        public class TestC : TestB
        {

        }

        public static void Test(TextA textA)
        {
            Console.WriteLine("TextA");
        }
        public static void Test(TestB textA)
        {
            TestAp(textA);
        }

        public static void TestAp(dynamic dynamicAp)
        {
            TestHandle(dynamicAp);
        }

        public static void TestHandle(TestC textA)
        {
            Console.WriteLine("TestC");
        }

        public static void TestHandle(dynamic dynamicAp)
        {
            Console.WriteLine("TestHandle");
        }

        public class TestName<T,T2>
        { 
            public T Name { get; set; }

            public T2 Name2 { get; set; }
        }

        protected static string GetPackageType(object package)
        {
            return package?.GetType().Name.Split('`')[0];
        }

        static void load(IEnumerable< int> vals)
        { 
        
        }

        public class SyncingClass : ContextBoundObject
        {
            private EventWaitHandle waitHandle;

            public SyncingClass()
            {
                waitHandle =
                   new EventWaitHandle(false, EventResetMode.ManualReset);
            }

            public void Signal()
            {
                Console.WriteLine("Thread[{0:d4}]: Signalling...", Thread.CurrentThread.GetHashCode());
                waitHandle.Set();
            }

            public void DoWait(bool leaveContext)
            {
                bool signalled;

                waitHandle.Reset();
                Console.WriteLine("Thread[{0:d4}]: Waiting...", Thread.CurrentThread.GetHashCode());
                signalled = waitHandle.WaitOne(3000, leaveContext);
                if (signalled)
                {
                    Console.WriteLine("Thread[{0:d4}]: Wait released!!!", Thread.CurrentThread.GetHashCode());
                }
                else
                {
                    Console.WriteLine("Thread[{0:d4}]: Wait timeout!!!", Thread.CurrentThread.GetHashCode());
                }
            }
        }

        public static void RunWaitKeepContext(object parm)
        {
            ((SyncingClass)parm).DoWait(false);
        }

        public static void RunWaitLeaveContext(object parm)
        {
            ((SyncingClass)parm).DoWait(true);
        }

        public static ValueTask HandleEventQueueMessageAsync(STANMsgContent msgContent)
        {
            var data = Encoding.UTF8.GetString(msgContent.Data);
            Console.WriteLine($"订阅 sequence={msgContent.Sequence} data={data}");
            return new ValueTask();
        }

        private static readonly Regex _clientIdReplacer = new Regex("[^A-Za-z0-9_]");

        static async Task Main(string[] args)
        {

           var ss =  _clientIdReplacer.Replace(nameof(Program), "_");

            //var sss = new int[2];
            //load(sss);
            //load(new List<int>());

            //var ttt = new TestName<int,string>().GetType();
            //Console.WriteLine(GetPackageType(new TestName<int, string>()));

            //return;

            var services = new ServiceCollection();
            services.AddLogging(options =>
            {
                options.AddConsole();
                options.AddDebug();
                options.SetMinimumLevel(LogLevel.Trace);
            });




            var options = new STANOptions();
            options.ClusterNodes.Add(new IPEndPoint(IPAddress.Parse("192.168.4.138"), 4222));
            options.ClusterID = "main-cluster";
            options.ClientId = "TestClientIdSender" + Guid.NewGuid().ToString();

            services.Configure<STANOptions>(options =>
            {
                options.ClusterNodes.Add(new IPEndPoint(IPAddress.Parse("192.168.4.138"), 4222));
                options.ClusterID = "main-cluster";
                options.ClientId = "TestClientIdSender" + Guid.NewGuid().ToString();
            });
            services.AddTransient<STANClient>();

            var spr = services.BuildServiceProvider();

            var client = spr.GetRequiredService<STANClient>();

            var logger = spr.GetRequiredService<ILogger<STANClient>>();


            logger.LogDebug("测试调试信息输出");


            await client.ConnectAsync();

            //var s = await client.SubscribeAsync("Agent-Recruit-Commission",
            //    new STANSubscribeOptions() { Position = StartPosition.SequenceStart, StartSequence = 57 },
            //    (content) =>
            //{

            //    var data = Encoding.UTF8.GetString(content.Data);
            //    Console.WriteLine($"订阅 sequence={content.Sequence} data={data}");
            //    return new ValueTask<bool>(true);
            //});

            var sss = await client.ReadAsync("Agent-Recruit-Commission", 1,200);

            Console.WriteLine(sss.Count);

            //for (int i = 0; i < 20; i++)
            //{
            //    var index = i.ToString();

            //var s = await client.SubscribeAsync("Test-Security-App-1",
            //    "Queue",
            //    (content) =>
            //{

            //    var data = Encoding.UTF8.GetString(content.Data);
            //    Console.WriteLine($"订阅 sequence={content.Sequence} data={data}");
            //    return new ValueTask<bool>(true);
            //});

            //}

            //var ss = await client.SubscribeAsync("Test-Security-App-1", (content) =>
            //{
            //    var data = Encoding.UTF8.GetString(content.Data);
            //    Console.WriteLine($"订阅 sequence={content.Sequence} data={data}");
            //    return new ValueTask<bool>(true);
            //});

            //await client.ReadAsync("Security-App-1", 1, 20);
            //await client.ReadAsync("Security-App-1", 1, 20);
            //await client.ReadAsync("Security-App-1", 1, 20);
            //await client.ReadAsync("Security-App-1", 1, 20);

            //var s = await client.ReadAsync("Security-App-1", 1, 20);

            //foreach (var sss in s)
            //{
            //    var data = Encoding.UTF8.GetString(sss.Data);
            //    Console.WriteLine($"sequence={sss.Sequence} data={data}");
            //}

            //Console.WriteLine("订阅2 完成 " + s.Count);

            //var s2 = await client.ReadAsync("Security-App-1", 1, 20);

            //foreach (var sss in s2)
            //{
            //    var data = Encoding.UTF8.GetString(sss.Data);
            //    Console.WriteLine($"sequence={sss.Sequence} data={data}");
            //}

            //Console.WriteLine("订阅2 完成 " + s2.Count);

            //var content = await client.ReadAsync("Labor-Enterprise-Account", 2);

            //var data = Encoding.UTF8.GetString(content.Data);
            //Console.WriteLine($"sequence={content.Sequence} data={data}");

            //var contents = await client.ReadAsync("Labor-Enterprise-Account", 2, 3);

            Console.WriteLine("订阅成功");

            while (true)
            {

                var str = Console.ReadLine();

                Console.WriteLine(str);
            }

            Thread.Sleep(Timeout.Infinite);


            //// 防止此主机进程终止，以使服务保持运行。
        }
    }
}
