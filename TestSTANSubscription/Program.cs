using Hunter.STAN.Client;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TestSTANSubscription
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var options = new STANOptions();
            options.ClusterNodes.Add("192.168.0.226");
            options.ClusterID = "main-cluster";
            options.ClientId = "TestClientIdSender";

            var client = new STANClient(options);
            await client.ContentcAsync();

            //"KeepLast"
            var s = client.Subscribe("Security-App-1", "TestSubscription", "keep", (bytes) =>
            {
                var sss = Encoding.UTF8.GetString(bytes);
                //var nowValue = int.Parse(sss.Split(' ')[0]);
                //if (lastValue != 0 && (nowValue - lastValue) != 1)
                //{
                //    Console.WriteLine("ERROR ==========================================================");
                //}
                //lastValue = nowValue;
                Console.WriteLine(sss);
            });


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
