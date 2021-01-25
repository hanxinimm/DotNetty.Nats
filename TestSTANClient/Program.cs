using Hunter.STAN.Client;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace TestSTANClient
{
    class Program
    {
        private static void pingTimerCallback(object state)
        {
            Console.WriteLine("我已执行发送Ping");
        }

        static async Task Main(string[] args)
        {

            var ss =  Guid.NewGuid().ToString("n");

            ////http://192.168.0.226:8221/subz

            //HttpClient hpclient = new HttpClient();
            //hpclient.BaseAddress = new Uri("http://192.168.0.226:8221");

            //var result = await hpclient.GetStringAsync("/subsz");

            //Console.WriteLine(result);

            //return;

            //Regex _subjectRegex = new Regex(@"^[a-zA-Z\d]+(\.[a-zA-Z\d]+)*$", RegexOptions.Compiled);

            //var mh = _subjectRegex.Match("foo.1");

            //if (mh.Success)
            //{
            //    Console.WriteLine(mh.Value);
            //}

            //return;

            //Timer PingTimer = new Timer(pingTimerCallback, null, 120000, Timeout.Infinite);

            //Console.ReadLine();

            //return;

            //IPHostEntry hostInfo = Dns.GetHostEntry("www.contoso.com");

            var services = new ServiceCollection();

            services.AddLogging(options => {
                options.SetMinimumLevel(LogLevel.Debug);
                options.AddConsole();
            });

            services.AddSTANServer(options =>
            {
                options.ClusterID = "main-cluster";
                options.ClientId = $"Security-StatefulManagerService";
                //options.Host = "mq.stan.yidujob.com";
                options.Host = "127.0.0.1";
                //options.Host = "192.168.4.131";
                //options.Host = "mq.stan.yd.com";
                options.Port = 4222;
                //options.ClusterNodes = new List<EndPoint>() { new IPEndPoint(IPAddress.Parse("mq.stan.yidujob.com"), 4222) };
            });


            var _serviceProvider = services.BuildServiceProvider();


            await using var client = _serviceProvider.GetRequiredService<STANClient>();

            client.ConnectAsync();

            //await client.DisposeAsync();

            //await client.DisposeAsync();
            //await client.DisposeAsync();
            //client.TryConnectAsync();


            //await client.ConnectAsync();


                //client.ConnectAsync();

                #region 无用代码

                for (int x = 0; x < 2; x++)//x是压测多少秒
            {
                for (int j = 0; j < 4; j++)//j是并发数
                {
                    Task.Run(async () =>
                    {
                        //for (int i = 0; ; i++)
                        for (int i = 0; i < 10; i++)//i是每个并发下执行多少次如果压测行为是起task的每秒钟的并发就是i*j
                        {
                            {
                                DateTime tt1 = DateTime.Now;
                                Task.Run(() =>
                                {
                                    
                                    //压测行为
                                    client.CheckConnect();

                                    //client.DisposeAsync();

                                });
                                double time = (DateTime.Now - tt1).TotalMilliseconds;
                                if (time < 1000)
                                {
                                    await Task.Delay((int)(1000 - time));
                                }
                            }
                        }
                    });
                }
                System.Threading.Thread.Sleep(1000);
                break;
            }

            #endregion;




            //client.TryConnectAsync();

            //client.TryConnectAsync();

            //await client.DisposeAsync();

            //await client.DisposeAsync();

            //await client.DisposeAsync();

            Console.WriteLine("成功执行");

            Console.ReadLine();

            client.CheckConnect();

            //await client.DisposeAsync();

            //await client.DisposeAsync();

            //await client.ConnectAsync();

            Console.WriteLine("连接成功");



            //for (int i = 0; i < 100; i++)
            //{
            //    var client = new STANClient(options);
            //    await client.ConnectAsync("main-cluster", "TestClientId" + i);
            //}

            //Console.WriteLine("完成创建");
            //Console.ReadLine();

            //return;

            #region 订阅测试

            //var lastValue = 0;
            ////"KeepLast"
            var s = client.SubscribeAsync("OrderPlaced", (bytes) =>
            {
                var sss = Encoding.UTF8.GetString(bytes.Data);
                //var nowValue = int.Parse(sss.Split(' ')[0]);
                //if (lastValue != 0 && (nowValue - lastValue) != 1)
                //{
                //    Console.WriteLine("ERROR ==========================================================");
                //}
                //lastValue = nowValue;
                Console.WriteLine(sss);
            });

            //// 防止此主机进程终止，以使服务保持运行。


            //Console.WriteLine("按任意键取消订阅");

            //Console.ReadLine();

            //client.UnSubscribe(s);

            //Console.WriteLine("按任意再次订阅");

            //Console.ReadLine();

            //var s1 = client.Subscribe("Sales@DESKTOP-EMDRLGS", string.Empty, string.Empty, (bytes) =>
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


            #endregion



            //client.Subscribe("Security-App-1", "Test-Vue", "Vue", new STANSubscribeOptions(), (data) =>
            //{
            //    var TestString = Encoding.UTF8.GetString(data);
            //    Console.WriteLine(TestString);
            //});

            //Console.WriteLine("订阅成功");

            //Console.ReadLine();

            //return;

            #region  发布测试

            int i = 0;

            var s0 = await client.ReadAsync("OrderPlaced", 1,10);

            var s1 = await client.ReadAsync("OrderPlaced", 1, 10);

            var s2 = await client.ReadAsync("OrderPlaced", 1, 10);

            Console.ReadLine();


            while (true)
            {
                Console.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start(); //  开始监视代码运行时间


                var Testbytes = Encoding.UTF8.GetBytes($"序号 {i} 这是一个客户端测试消息-特殊标记" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                //client.Publish("test3", Testbytes);

                for (int j = 0; j < 3; j++)
                {

                    await Task.Factory.StartNew(async () =>
                    {
                        await client.PublishAsync("OrderPlaced", Testbytes);
                    });

                }
                //if (Rlt == null) Console.WriteLine("发送失败");
                

                stopwatch.Stop(); //停止监视  

                //TimeSpan timespan = stopwatch.Elapsed; //  获取当前实例测量得出的总时间  
                Console.WriteLine("完成发送 " + stopwatch.ElapsedMilliseconds);

                Console.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

                Console.WriteLine("输入任意字符串开始新一轮发送");
                Console.ReadLine();

                i++;
            }



            #endregion;

            Console.WriteLine("按任意键退出应用");

            Console.ReadLine();
        }
    }
}
