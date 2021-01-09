using Hunter.NATS.Client;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace TestNATSClient
{
    class Program
    {
        private static readonly Regex _clientIdReplacer = new Regex("\\W\\D");
        static async Task Main(string[] args)
        {

           // var SubscriptionMsgContentReady = new TaskCompletionSource<bool>();


           // await Task.Factory.StartNew(async () =>
           //{
           //    var rlt = await SubscriptionMsgContentReady.Task;
           //    if (rlt)
           //    {
           //        Console.WriteLine("1链接成功");
           //    }
           //    else
           //    {
           //        Console.WriteLine("1链接失败");
           //    }
           //},new CancellationTokenSource(TimeSpan.FromSeconds(2)).Token);

           // await Task.Factory.StartNew(async () =>
           // {
           //     var rlt = await SubscriptionMsgContentReady.Task;
           //     if (rlt)
           //     {
           //         Console.WriteLine("2链接成功");
           //     }
           //     else
           //     {
           //         Console.WriteLine("2链接失败");
           //     }
           // });

           // await Task.Factory.StartNew(() =>
           // {
           //     Console.WriteLine("等待开始");
           //     System.Threading.Thread.Sleep(1000 * 5);
           //     Console.WriteLine("等待完成");

           //     SubscriptionMsgContentReady.SetResult(true);

           // });

           // Console.ReadLine();

           // Console.WriteLine("完成");

           // return;

            //var ss = Guid.NewGuid().ToString("N");

            //var options = new NATSOptions();
            //options.ClusterID = "main-cluster";
            //options.ClientId = "TestClientId";
            //options.ClusterNodes.Add(new IPEndPoint(IPAddress.Parse("mq.stan.yd.com"), 4221));
            Environment.SetEnvironmentVariable("NETCOREAPP_ENVIRONMENT", "Production");

            var services = new ServiceCollection();

            services.AddLogging(options => {
                options.SetMinimumLevel(LogLevel.Debug);
                options.AddConsole();
            });

            services.AddNATSServer(options =>
            {
                options.ClusterID = "stan-k8s-cluster";
                options.ClientId = "TestClientId" + Guid.NewGuid().ToString("N");
                options.Host = "192.168.4.131";
                //options.Host = "mq.nats.yd.com";
                //options.Host = "mq.nats.laboroa.cn";
                options.Port = 4221;
                options.IsAuthentication = true;
                options.UserName = "08GF8EJeRlHKvQGTU0m5QA==";
                options.Password = "PXAR6Dj8DDDdMqV1HyZttA==";
                //options.ClusterNodes = new List<EndPoint>() { new IPEndPoint(IPAddress.Parse("mq.stan.yidujob.com"), 4222) };
            });

            var _serviceProvider = services.BuildServiceProvider();


            await using var client = _serviceProvider.GetRequiredService<NATSClient>();


            //await client.ConnectAsync();

            var httpClient = new HttpClient();
            

            var s = await client.SubscribeAsync("ApiGateway.>", "ApiGateway.Test", async (bytes) =>
            {
                Console.WriteLine("开始接受收消息");
                var sss = Encoding.UTF8.GetString(bytes.Data);
                Console.WriteLine("收到消息 {0}", sss);
            });

            //Console.ReadLine();

            //return;


            #region  发布测试

            while (true)
            {
                Console.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start(); //  开始监视代码运行时间



                for (int i = 0; i < 1; i++)
                {

                    var Testbytes = Encoding.UTF8.GetBytes($"序号 {i} 这是一个客户端测试消息-特殊标记" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

                    await client.PublishAsync("ApiGateway.EventTrigger.4343", Testbytes);
                }

                stopwatch.Stop(); //  停止监视  

                TimeSpan timespan = stopwatch.Elapsed; //  获取当前实例测量得出的总时间  
                Console.WriteLine("完成发送" + stopwatch.ElapsedMilliseconds);

                Console.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

                Console.WriteLine("输入任意字符串开始新一轮发送");
                Console.ReadLine();
            }

            #endregion;

            #region 订阅测试

            //var lastValue = 0;
            ////"KeepLast"
            //var s = await client.SubscriptionAsync("OrderPlaced", string.Empty, (bytes) =>
            //{
            //    var sss = Encoding.UTF8.GetString(bytes);
            //    //var nowValue = int.Parse(sss.Split(' ')[0]);
            //    //if (lastValue != 0 && (nowValue - lastValue) != 1)
            //    //{
            //    //    Console.WriteLine("ERROR ==========================================================");
            //    //}
            //    //lastValue = nowValue;
            //    Console.WriteLine(sss);
            //});

            //// 防止此主机进程终止，以使服务保持运行。


            //Console.WriteLine("按任意键取消订阅");

            //Console.ReadLine();

            //await client.UnSubscriptionAsync(s);

            //Console.WriteLine("按任意再次订阅");

            //Console.ReadLine();


            #endregion;

        }
    }
}