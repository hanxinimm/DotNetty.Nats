using Hunter.NATS.Client;
using System;
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

            #region 订阅测试

            var lastValue = 0;
            //"KeepLast"
            var s = await client.SubscriptionAsync("OrderPlaced", string.Empty, (bytes) =>
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

            // 防止此主机进程终止，以使服务保持运行。


            Console.WriteLine("按任意键取消订阅");

            Console.ReadLine();

            await client.UnSubscriptionAsync(s);

            Console.WriteLine("按任意再次订阅");

            Console.ReadLine();


            #endregion;

        }
    }
}