using Hunter.DDD.EventBus;
using Hunter.DDD.Message;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace TestEventBus
{
    class Program
    {
        static async Task Main(string[] args)
        {

            var services = new ServiceCollection();

            services.AddLogging(options =>
            {
                options.SetMinimumLevel(LogLevel.Debug);
                options.AddConsole();
            });

            //services.AddSTANServer(options =>
            //{
            //    options.ClusterID = "main-cluster";
            //    options.ClientId = $"Security-StatefulManagerService";
            //    //options.Host = "mq.stan.yidujob.com";
            //    //options.Host = "127.0.0.1";
            //    //options.Host = "192.168.4.131";
            //    options.Host = "mq.stan.yd.com";
            //    options.Port = 4222;
            //    //options.ClusterNodes = new List<EndPoint>() { new IPEndPoint(IPAddress.Parse("mq.stan.yidujob.com"), 4222) };
            //});

            services.AddMessageNATSStreamingEventBus(options =>
            {
                options.ClusterID = "main-cluster";
                options.ClientId = $"Security-StatefulManagerService";
                //options.Host = "mq.stan.yidujob.com";
                //options.Host = "127.0.0.1";
                //options.Host = "192.168.4.131";
                options.Host = "mq.stan.yd.com";
                options.Port = 4222;
                //options.ClusterNodes = new List<EndPoint>() { new IPEndPoint(IPAddress.Parse("mq.stan.yidujob.com"), 4222) };
            });

            services.AddDataSerializerAdapter()
                .AddJsonDataSerializer();

            var _serviceProvider = services.BuildServiceProvider();

            Console.WriteLine("Hello World!");


            var _eventBus = _serviceProvider.GetRequiredService<IMessageEventBus>();

            for (var i = 0; i < 100; i++)
            {
                await Task.Factory.StartNew(async () =>
                {
                    await _eventBus.SendAsync(new MSG());
                });
                await Task.Factory.StartNew(async () =>
                {
                    await _eventBus.SendAsync(new MSG());
                });
                await Task.Factory.StartNew(async () =>
                {
                    await _eventBus.SendAsync(new MSG());
                });
            }

            Console.WriteLine("完成");
            Console.ReadLine();

        }
    }

    [Subject("Test")]
    public class MSG
    { 
        
    }
}
