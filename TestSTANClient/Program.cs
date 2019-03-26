using Hunter.STAN.Client;
using System;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using System.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Hunter.Message.SMS.Abstractions;

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

            var services = new ServiceCollection();

            services.AddTwilioSmsSender(testoptions =>
            {
                testoptions.AccountSid = "AC7424c95c7be55450b36307514359d410";
                testoptions.AuthToken = "OhPCBk1SG3ioVIIsAkBY41HUorXUeo9A";
                testoptions.FromPhoneNumber = "12028462874";
            });

            var serviceSP = services.BuildServiceProvider();
            var smsSender = serviceSP.GetRequiredService<ISmsSender>();

            await smsSender.SendAsync("8618166772526", "上琪开15你仲了吗? 迦我薇信qq同号：28739347 就有晚上的一肖③馬，另外时时菜PK⑩计划");

            return;

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

            var options = new STANOptions();
            options.ClusterNodes.Add("192.168.0.226");

            var client = new STANClient(options);
            await client.ContentcAsync("main-cluster", "TestClientIdSender");

            //for (int i = 0; i < 100; i++)
            //{
            //    var client = new STANClient(options);
            //    await client.ContentcAsync("main-cluster", "TestClientId" + i);
            //}

            //Console.WriteLine("完成创建");
            //Console.ReadLine();

            //return;

            #region 订阅测试

            //var lastValue = 0;
            ////"KeepLast"
            //var s = client.Subscribe("OrderPlaced", string.Empty, string.Empty, (bytes) =>
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

            #region  发布测试

            int i = 0;

            while (true)
            {
                Console.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start(); //  开始监视代码运行时间


                var Testbytes = Encoding.UTF8.GetBytes($"序号 {i} 这是一个客户端测试消息-特殊标记" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                //client.Publish("test3", Testbytes);
                var rlt = await client.PublishWaitAckAsync("Security-App-1", Testbytes);
                //if (Rlt == null) Console.WriteLine("发送失败");
                

                stopwatch.Stop(); //停止监视  

                //TimeSpan timespan = stopwatch.Elapsed; //  获取当前实例测量得出的总时间  
                Console.WriteLine("完成发送" + stopwatch.ElapsedMilliseconds);

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
