using Hunter.NATS.Client;
using System;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;

namespace TestNATSClient
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var options = new NATSOptions();
            options.ClusterNodes.Add("192.168.0.226");

            var client = new NATSClient(options);
            await client.ContentcAsync("main-cluster", "TestClientId");

            int SValue = 0;



            var s = await client.SubscriptionAsync("OrderPlaced", string.Empty, (bytes) =>
            {
                SValue++;

                if (SValue >= 10000) {
                    var sss = Encoding.UTF8.GetString(bytes);
                    Console.WriteLine(sss);
                    Console.WriteLine(SValue);
                    SValue = 0;
                }
            });


            #region  发布测试

            while (true)
            {
                Console.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start(); //  开始监视代码运行时间

                var Testbytes = Encoding.UTF8.GetBytes("这是一个客户端测试消息-特殊标记" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

                for (int i = 0; i < 10000; i++)
                {
                    //client.Publish("test3", Testbytes);
                    await client.PublishAsync("OrderPlaced", Testbytes);
                }

                stopwatch.Stop(); //  停止监视  

                //TimeSpan timespan = stopwatch.Elapsed; //  获取当前实例测量得出的总时间  
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