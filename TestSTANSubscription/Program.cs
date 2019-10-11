﻿using DotNetty.Codecs.STAN.Protocol;
using Hunter.STAN.Client;
using System;
using System.Collections.Concurrent;
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

        static async Task Main(string[] args)
        {
            var options = new STANOptions();
            options.ClusterNodes.Add(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 4222));
            options.ClusterID = "main-cluster";
            options.ClientId = "TestClientIdSender" + Guid.NewGuid().ToString();

            var client = new STANClient(options);
            await client.ContentcAsync();


            //var s = await client.ReadAsync("Labor-Enterprise-Account",
            //    2,
            //    (content) =>
            //{
            //    var data = Encoding.UTF8.GetString(content.Data);
            //    Console.WriteLine($"sequence={content.Sequence} data={data}");
            //    return new ValueTask<bool>(true);
            //});

            var s = await client.SubscribeDurableAsync("Security-App-1",
                "T",
                (content) =>
            {
                var data = Encoding.UTF8.GetString(content.Data);
                Console.WriteLine($"sequence={content.Sequence} data={data}");
                return new ValueTask<bool>(true);
            });

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
