using DotNetty.Codecs.STAN.Protocol;
using Hunter.STAN.Client;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Text;
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
        static async Task Main(string[] args)
        {

            //var sss = new int[2];
            //load(sss);
            //load(new List<int>());

            //var ttt = new TestName<int,string>().GetType();
            //Console.WriteLine(GetPackageType(new TestName<int, string>()));

            //return;

            var options = new STANOptions();
            options.ClusterNodes.Add(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 4222));
            options.ClusterID = "main-cluster";
            options.ClientId = "TestClientIdSender" + Guid.NewGuid().ToString();

            var client = new STANClient(options);
            await client.ContentcAsync();


            //for (int i = 0; i < 20; i++)
            //{
            //    var index = i.ToString();

            var s = await client.SubscribeAsync("Security-App-1",
                "Queue",
                (content) =>
            {

                var data = Encoding.UTF8.GetString(content.Data);
                Console.WriteLine($"订阅 sequence={content.Sequence} data={data}");
                return new ValueTask<bool>(true);
            });

            //}

            //await client.ReadAsync("Security-App-1", 1, 20);
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
