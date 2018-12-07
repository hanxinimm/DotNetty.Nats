using Hunter.STAN.Client;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Text;
using System.Diagnostics;

namespace TestSTANClient
{
    class Program
    {
        static async Task Main(string[] args)
        {

            var options = new STANOptions();
            options.ClusterNodes.Add("192.168.0.226");



            var client = new STANClient(options);
            await client.ContentcAsync("main-cluster", "TestClientId");
            var s = client.Subscription("test3", string.Empty);
            Console.WriteLine($"本地返回的订阅编号{s.SubscribeId}");
            //while (true)
            //{
            //    Console.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

            //    Stopwatch stopwatch = new Stopwatch();
            //    stopwatch.Start(); //  开始监视代码运行时间

            //    var Testbytes = Encoding.UTF8.GetBytes("这是一个客户端测试消息-特殊标记" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

            //    for (int i = 0; i < 1; i++)
            //    {
            //        client.Publish("test", Testbytes);
            //        //await client.PublishAsync("test", Testbytes);
            //    }

            //    stopwatch.Stop(); //  停止监视  

            //    //TimeSpan timespan = stopwatch.Elapsed; //  获取当前实例测量得出的总时间  
            //    Console.WriteLine("完成发送" + stopwatch.ElapsedMilliseconds);

            //    Console.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

            //    Console.WriteLine("输入任意字符串开始新一轮发送");
            //    Console.ReadLine();
            //}

            Console.WriteLine("按任意键退出应用");

            Console.ReadLine();
        }
    }
}
