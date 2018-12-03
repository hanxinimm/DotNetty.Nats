using Hunter.STAN.Client;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

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
            await client.SubscriptionAsync("test3", string.Empty);


            Console.WriteLine("按任意键退出应用");

            Console.ReadLine();
        }
    }
}
