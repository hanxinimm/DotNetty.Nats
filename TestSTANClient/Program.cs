using Hunter.STAN.Client;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Text;
using System.Diagnostics;
using Hunter.NATS.Client;

namespace TestSTANClient
{
    class Program
    {

        static async Task Main(string[] args)
        {
            var options = new NATSOptions();
            options.ClusterNodes.Add("192.168.0.226");

            var client = new NATSClient(options);
            await client.ContentcAsync("main-cluster", "TestClientId");

            //for (int j = 0; j < 8; j++)
            //{
            //    Stopwatch stopwatch = new Stopwatch();
            //    stopwatch.Start(); //  开始监视代码运行时间

            //    await client.PublishAsync("test33", Encoding.UTF8.GetBytes("测试消息"));

            //    stopwatch.Stop(); //  停止监视  

            //    //TimeSpan timespan = stopwatch.Elapsed; //  获取当前实例测量得出的总时间  
            //    Console.WriteLine("完成发送" + stopwatch.ElapsedMilliseconds);

            //    Console.ReadLine();

            //}

            //await client.PongAsync();

            //Console.WriteLine("发送一万条");
            var SubscribeId = await client.SubscriptionAsync("foo-test", string.Empty, (bytes) =>
            {
                var sss = Encoding.UTF8.GetString(bytes);
                Console.WriteLine("收到消息: " + sss);
            });

            //await Task.Factory.StartNew(async () =>
            //{
            //    await Task.Delay(TimeSpan.FromSeconds(5));

            //    Console.WriteLine("取消订阅");

            //    await client.UnSubscriptionAsync(SubscribeId);
            //});

            await Task.Factory.StartNew(async () =>
            {
                await Task.Delay(TimeSpan.FromSeconds(5));

                Console.WriteLine("收到5条消息取消订阅");

                await client.AutoUnSubscriptionAsync(SubscribeId, 5);
            });



            Console.ReadLine();
        }

        //static async Task Main(string[] args)
        //{

        //    var options = new STANOptions();
        //    options.ClusterNodes.Add("192.168.0.226");

        //    var client = new STANClient(options);
        //    await client.ContentcAsync("main-cluster", "TestClientId");

        //    #region 订阅测试

        //    //var lastValue = 0;
        //    //var s = client.Subscription("test33", string.Empty, (bytes) =>
        //    //{
        //    //    var sss = Encoding.UTF8.GetString(bytes);
        //    //    var nowValue = int.Parse(sss.Split(' ')[0]);
        //    //    if (lastValue != 0 && (nowValue - lastValue) != 1)
        //    //    {
        //    //        Console.WriteLine("ERROR ==========================================================");
        //    //    }
        //    //    lastValue = nowValue;
        //    //    Console.WriteLine(nowValue);
        //    //});
        //    //Console.WriteLine($"本地返回的订阅编号{s.ReplyTo}");

        //    #endregion

        //    #region  发布测试

        //    //while (true)
        //    //{
        //    //    Console.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

        //    //    Stopwatch stopwatch = new Stopwatch();
        //    //    stopwatch.Start(); //  开始监视代码运行时间

        //    //    var Testbytes = Encoding.UTF8.GetBytes("这是一个客户端测试消息-特殊标记" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

        //    //    for (int i = 0; i < 5; i++)
        //    //    {
        //    //        //client.Publish("test3", Testbytes);
        //    //        await client.PublishAsync("test", Testbytes);
        //    //    }

        //    //    stopwatch.Stop(); //  停止监视  

        //    //    //TimeSpan timespan = stopwatch.Elapsed; //  获取当前实例测量得出的总时间  
        //    //    Console.WriteLine("完成发送" + stopwatch.ElapsedMilliseconds);

        //    //    Console.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

        //    //    Console.WriteLine("输入任意字符串开始新一轮发送");
        //    //    Console.ReadLine();
        //    //}

        //    #endregion;

        //    Console.WriteLine("按任意键退出应用");

        //    Console.ReadLine();
        //}
    }
}
