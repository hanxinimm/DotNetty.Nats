//using System;
//using System.Collections.Generic;
//using System.Text;

//namespace ConsoleAppTest
//{
//    public class NATSClass
//    {
//        static void TestNATS()
//        {
//            //NATSTestSubscribe
//            var cf = new ConnectionFactory();
//            //stopts.NatsURL = "nats://nats.cp.com:4221";

//            int j = 1;
//            string code = string.Empty;
//            while (code != "5")
//            {
//                string json = Newtonsoft.Json.JsonConvert.SerializeObject(new Order()
//                {
//                    AccountId = 1000,
//                    PeriodNo = 23408,
//                    OrderNo = "14586413134678743131347",
//                    Amount = 23034.45M,
//                    BetCount = 230,
//                    MoneyUnit = 123,
//                    TotalAmount = 2313,
//                    Content = "231313123131313",
//                    GameId = 233131313,
//                    PlayItemId = 233131313,
//                    CreateTime = DateTime.Now
//                });

//                var bytes = System.Text.Encoding.UTF8.GetBytes(json);
//                var opts = ConnectionFactory.GetDefaultOptions();
//                opts.Servers = new string[] { "nats://nats.cp.com:4221", "nats://nats.cp.com:4222" };
//                using (var c = cf.CreateConnection(opts))
//                {

//                    Stopwatch sw = new Stopwatch();
//                    sw.Start();
//                    for (int i = 0; i < j; i++)
//                    {

//                        c.Publish("foo", bytes);


//                    }

//                    sw.Stop();

//                    Console.WriteLine("{0} 条消息已经发送完毕,耗时 {1} 毫秒,请输入消息", j, sw.ElapsedMilliseconds);
//                    code = Console.ReadLine();
//                }

//            }

//        }
//    }
//}
