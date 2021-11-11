using DotNetty.Codecs.NATSJetStream.Protocol;
using DotNetty.Codecs.Protocol;
using Hunter.NATS.Client;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NATS.Client;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
            var bin = Encoding.UTF8.GetBytes("\n");
            Console.WriteLine(string.Format("{0:x}", bin[0]));
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
            //Environment.SetEnvironmentVariable("NETCOREAPP_ENVIRONMENT", "Production");

            var services = new ServiceCollection();

            services.AddLogging(options =>
            {
                options.SetMinimumLevel(LogLevel.Debug);
                options.AddConsole();
            });

            services.AddNATSServer(options =>
            {
                //options.ClusterID = "stan-k8s-cluster";
                options.ClientId = "TestClientId" + Guid.NewGuid().ToString("N");
                //options.Host = "127.0.0.1";
                //options.Host = "192.168.4.131";
                options.Host = "mq.nats.yd.com";
                //options.Host = "mq.nats.laboroa.cn";
                options.Port = 4221;
                //options.IsAuthentication = true;
                //options.UserName = "08GF8EJeRlHKvQGTU0m5QA==";
                //options.Password = "PXAR6Dj8DDDdMqV1HyZttA==";
                //options.ClusterNodes = new List<EndPoint>() { new IPEndPoint(IPAddress.Parse("mq.stan.yidujob.com"), 4222) };
            });

            var _serviceProvider = services.BuildServiceProvider();


            await using var client = _serviceProvider.GetRequiredService<NATSClient>();

            //var s = await client.SubscribeAsync("ApiGateway.>", string.Empty, (bytes) =>
            //{
            //    var sss = Encoding.UTF8.GetString(bytes.Data);
            //    //var nowValue = int.Parse(sss.Split(' ')[0]);
            //    //if (lastValue != 0 && (nowValue - lastValue) != 1)
            //    //{
            //    //    Console.WriteLine("ERROR ==========================================================");
            //    //}
            //    //lastValue = nowValue;
            //    Console.WriteLine(sss);
            //});

            //Console.ReadLine();

            //return;

            //await Task.Factory.StartNew(async () => await client.ConnectAsync(),
            //       new CancellationToken(),
            //       TaskCreationOptions.DenyChildAttach,
            //       TaskScheduler.Default);


            //var streamList = await client.StreamListAsync();

            //foreach (var StreamItem in streamList.Streams)
            //{
            //    Console.WriteLine($"StreamName = {StreamItem.Config.Name} " +
            //        $"StreamSubjects = {string.Join(',', StreamItem.Config.Subjects)}" +
            //        $"Stream = {StreamItem}");
            //    //var rlt = await client.StreamDeleteAsync(StreamItem.Config.Name);
            //}

            //var streamInfo = await client.StreamInfoAsync("Labor-Work-Tenant");
            //Console.WriteLine(streamInfo);

            //Console.WriteLine("按任意键继续");

            //Console.ReadLine();

            //var streamNames = await client.StreamNamesAsync();

            //var streamName = streamNames.Streams.FirstOrDefault();

            var streamName = "Test-All";

            var streamCreate = await client.StreamCreateOrGetAsync(JetStreamConfig.Builder()
                .SetName(streamName)
                .SetSubjects($"{streamName}.>")
                .SetMaxAge(TimeSpan.FromMinutes(5)).Build());



            Console.WriteLine($"StreamName = {streamCreate}");

            //var streamInfo = await client.StreamInfoAsync(streamName);

            //Console.WriteLine("streamInfo = {0}", streamInfo);


            //var streamUpdate = await client.StreamUpdateAsync(JetStreamConfig.Builder(streamInfo.Config)
            //    .SetSubjects("TestAll-Work.>")
            //    .SetMaxAge(TimeSpan.FromDays(5)).Build());


            //var streamName = "Labor-Work-Tenant";
            //var streamInfo = await client.StreamInfoAsync(streamName);

            //Console.WriteLine("按任意键删除流");
            //Console.ReadLine();

            //var streamDelete = await client.StreamDeleteAsync(streamName);

            //Console.WriteLine("streamDelete = {0}", streamDelete);





            ///var consumerInfo = await client.ConsumerInfoAsync(streamName,"Te");

            //var consumerNames = await client.ConsumerNamesAsync("Finance-Borrow");

            //foreach (var consumerName in consumerNames.Consumers)
            //{
            //    Console.WriteLine($"consumerName = {consumerName}");
            //    //var rlt = await client.ConsumerDeleteAsync(streamName, consumerName);
            //}

            //var consumerList = await client.ConsumerListAsync("Recruit-Job");

            //foreach (var consumer in consumerList.Consumers)
            //{
            //    if (consumer.Name == "Finance_Borrow_CreateEvent_Labor_Finance_StatelessManagerService")
            //    {
            //        Console.WriteLine($"consumerName = {consumer.Name} {consumer.Config}");
            //    }

            //    //var rlt = await client.ConsumerDeleteAsync(streamName, consumerName);
            //}

            //var rlt = await client.ConsumerDeleteAsync("Recruit-Job", "Recruit_Job_PublishJobCommand_Labor_Recruit_StatelessManagerService");
            ////var streamMessage = await client.StreamReadMessageAsync("TestAll", 2);


            Console.WriteLine("按任意键继续");

            Console.ReadLine();

            var httpclient = new HttpClient();
            httpclient.Timeout = TimeSpan.FromSeconds(20);

            //ThreadPool.QueueUserWorkItem(new WaitCallback(MessageProcessingChannelAsyncConfigAsync), Packet.Message.Inbox);

            //[事件]开始处理 主题 Finance-Borrow.CreateEvent.5128660 序号 2

            var consumerCreate = await client.ConsumerCreateOrAdaptiveAsync(streamName,
                ConsumerConfig.Builder()
                .SetFilterSubject($"{streamName}.Apply.>")
                .SetDeliverPolicy(DeliverPolicy.DeliverLast)
                 .SetAckPolicy(AckPolicy.AckAll)
                 .SetMaxDeliver(3)
                 .SetDurable("T19_2")
                 //.SetDeliverGroup("T")
                 .SetAckWait(TimeSpan.FromSeconds(20)),
                 async (bytes) =>
                {

                    Console.WriteLine("[消费者]请求数据S" + DateTime.Now);


                    await httpclient.GetAsync("https://api.docs.gateway.yidujob.com/apiGateway/docs/Enterprise");

                    Console.WriteLine("[消费者]请求数据E" + DateTime.Now);



                    Console.WriteLine("[消费者]开始接受收消息");
                    var sss = Encoding.UTF8.GetString(bytes.Data);
                    Console.WriteLine("收到消息 {0}  标识 {1}", sss, bytes.Metadata);
                    return MessageAck.Ack;
                });

            Console.WriteLine($"consumerConfig = {consumerCreate}");

            //=Labor-Work-Tenant.MultipleInterviewPassedMessage
            //完成发布消息 Labor-Work-Tenant.MultipleInterviewPassedMessage 消息标识95260000-5992-6c2b-39e9-08d9809856ca
            //var s2 = await client.SubscribeAsync($"{streamName}.>", "T", (bytes) =>
            //{
            //    Console.WriteLine("开始接受收消息");
            //    var sss = Encoding.UTF8.GetString(bytes.Data);
            //    Console.WriteLine("收到消息 {0}", sss);
            //});

            Console.WriteLine("按任意键开始发布消息");

            Console.ReadLine();

            //return;


            #region  发布测试

            int msg_sq = 0;

            var header = new Dictionary<string, string>();

            header.Add("Content-Type", "Json");
            header.Add("Safe", "true");
            header.Add("Token", "auth");



            Options opts = ConnectionFactory.GetDefaultOptions();
            //opts.Name
            opts.Url = "mq.nats.yd.com:4221";

            //using IConnection c = new ConnectionFactory().CreateConnection(opts);


            //EventHandler<MsgHandlerEventArgs> msgHandler = (sender, args) =>
            //{
            //    Console.WriteLine("Received: " + args.Message);
            //};
            //IAsyncSubscription s11 = c.SubscribeAsync($"FRONT.>", msgHandler);

            //IAsyncSubscription s12 = c.SubscribeAsync($"{streamName}.>", msgHandler);



            while (true)
            {
                Console.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start(); //  开始监视代码运行时间


                var Testbytes = Encoding.UTF8.GetBytes($"Knock Knock");


                for (int i = 0; i < 3; i++)
                {
                    await Task.Factory.StartNew(async () =>
                    {
                        for (int j = 0; j < 3; j++)
                        {
                            await Task.Factory.StartNew(async () =>
                            {
                                var sub = $"{streamName}.Apply." + DateTime.Now.Millisecond;
                                Console.WriteLine("消息主题 {0}", sub);
                                await client.PublishAsync(sub, Testbytes);
                            });
                        }
                    });

                    //var Testbytes = Encoding.UTF8.GetBytes($"序号 {msg_sq++} [Test2]这是一个客户端测试消息-特殊标记" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

                    //await client.PublishAsync("Test2", Testbytes);

                    //var ApiGatewaybytes = Encoding.UTF8.GetBytes($"序号 {msg_sq++} [ApiGateway]这是一个客户端测试消息-特殊标记" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

                    //await client.PublishAsync("ApiGateway.EventTrigger.4343", ApiGatewaybytes);

                    //var ApiGatewaybytes2 = Encoding.UTF8.GetBytes($"序号 {msg_sq++} [ApiGateway2]这是一个客户端测试消息-特殊标记" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

                    //await client.PublishAsync("ApiGateway.EventTrigger.4242", ApiGatewaybytes2);
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