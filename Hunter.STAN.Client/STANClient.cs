using DotNetty.Codecs.STAN;
using DotNetty.Codecs.STAN.Packets;
using DotNetty.Codecs.STAN.Protocol;
using DotNetty.Handlers.STAN;
using DotNetty.Transport.Bootstrapping;
using DotNetty.Transport.Channels;
using DotNetty.Transport.Channels.Sockets;
using Hunter.STAN.Client.Handlers;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Hunter.STAN.Client
{
    public partial class STANClient : IDisposable
    {
        private readonly ILogger _logger;
        /// <summary>
        /// STAN配置
        /// </summary>
        private readonly STANOptions _options;

        /// <summary>
        /// 客户端编号
        /// </summary>
        private readonly string _clientId;

        /// <summary>
        /// 心跳收件箱
        /// </summary>
        private readonly string _heartbeatInboxId;

        /// <summary>
        /// 消息应答收件箱
        /// </summary>
        private readonly string _replyInboxId;

        /// <summary>
        /// 
        /// </summary>
        private readonly ValueTask<bool> _ackSuccessResult = new ValueTask<bool>(true);

        ///// <summary>
        ///// 通道引导
        ///// </summary>
        private Bootstrap _bootstrap;

        /// <summary>
        /// 连接通道
        /// </summary>
        private IChannel _channel;

        /// <summary>
        /// 连接配置
        /// </summary>
        private STANConnectionConfig _config;

        /// <summary>
        /// 等待发送消息确认安排表
        /// </summary>
        private ConcurrentDictionary<string, TaskCompletionSource<PubAckPacket>> _waitPubAckTaskSchedule;

        ///// <summary>
        ///// 等待订阅消息响应安排表
        ///// </summary>
        //private ConcurrentDictionary<string, TaskCompletionSource<SubscriptionResponsePacket>> _waitSubResponseTaskSchedule;

        /// <summary>
        /// 等待取消订阅消息响应安排表
        /// </summary>
        private ConcurrentDictionary<string, TaskCompletionSource<UnSubscriptionResponsePacket>> _waitUnSubResponseTaskSchedule;

        public STANClient(STANOptions options)
        {
            _options = options ?? throw new ArgumentNullException(nameof(STANOptions));
            _clientId = _options.ClientId;
            _heartbeatInboxId = GenerateInboxId();
            _replyInboxId = GenerateInboxId();
            _waitPubAckTaskSchedule = new ConcurrentDictionary<string, TaskCompletionSource<PubAckPacket>>();
            //TODO:改变当前写法，注入管道处理
            //_waitSubResponseTaskSchedule = new ConcurrentDictionary<string, TaskCompletionSource<SubscriptionResponsePacket>>();
            _waitUnSubResponseTaskSchedule = new ConcurrentDictionary<string, TaskCompletionSource<UnSubscriptionResponsePacket>>();
            _bootstrap = InitBootstrap();

        }

        public STANClient(IOptions<STANOptions> options) : this(options.Value)
        {

        }

        public bool IsOpen => _channel?.Open ?? false;

        private Bootstrap InitBootstrap()
        {
            return new Bootstrap()
            .Group(new MultithreadEventLoopGroup())
            .Channel<TcpSocketChannel>()
            .Option(ChannelOption.TcpNodelay, false)
            .Handler(new ActionChannelInitializer<ISocketChannel>(channel =>
            {
                channel.Pipeline.AddLast(new ReconnectChannelHandler(ReconnectIfNeedAsync));
                channel.Pipeline.AddLast(STANEncoder.Instance, STANDecoder.Instance);
                channel.Pipeline.AddLast(new ErrorPacketHandler());
                channel.Pipeline.AddLast(new HeartbeatPacketHandler());
                //channel.Pipeline.AddLast(new PubAckPacketSyncHandler(_waitPubAckTaskSchedule));
                //if (_options.PubAckCallback != null)
                //    channel.Pipeline.AddLast(new PubAckPacketAsynHandler(PubAckCallback));
                //channel.Pipeline.AddLast(new SubscriptionResponsePacketSyncHandler(_waitSubResponseTaskSchedule));
                //TODO:异步订阅写法有问题
                //channel.Pipeline.AddLast(new SubscriptionResponsePacketAsynHandler((subReps) => { }));
                //channel.Pipeline.AddLast(new UnSubscriptionResponsePacketSyncHandler(_waitUnSubResponseTaskSchedule));
                //channel.Pipeline.AddLast(new UnSubscriptionResponsePacketHandler((unSubReps) => { }));
                channel.Pipeline.AddLast(new PingPacketHandler());
                channel.Pipeline.AddLast(new PongPacketHandler());
            }));
        }

        private bool IsChannelInactive
        {
            get
            {
                return !_channel.Active;
            }
        }

        private async Task ReconnectIfNeedAsync(EndPoint socketAddress)
        {
            if (this.IsChannelInactive)
            {
                //await this.semaphoreSlim.WaitAsync();
                try
                {
                    if (this.IsChannelInactive)
                    {
                        while (true)
                        {
                            try
                            {
                                _channel = await _bootstrap.ConnectAsync(socketAddress);
                                break;
                            }
                            catch (Exception)
                            {
                                await Task.Delay(TimeSpan.FromSeconds(3));
                            }
                        }
                        // this.clientRpcHandler = channel.Pipeline.Get<RpcClientHandler>();
                    }
                }
                finally
                {
                    //this.semaphoreSlim.Release();
                }
            }
        }

        #region 消息发送确认

        private void PubAckCallback(PubAckPacket pubAck)
        {
            _options.PubAckCallback(new STANMsgPubAck(pubAck.Message.Guid, pubAck.Message.Error));
        }

        #endregion;

        private void CheckSubject(string subject)
        {
            if (string.IsNullOrEmpty(subject)) throw new ArgumentNullException(nameof(subject));
        }

        private void CheckQueueGroup(string queueGroup)
        {
            if (string.IsNullOrEmpty(queueGroup)) throw new ArgumentNullException(nameof(queueGroup));
        }

        private static string GenerateInboxId()
        {
            return Guid.NewGuid().ToString("N");
        }

        //TODO:2.1框架好像支持释放资源
        public void Dispose()
        {
            try
            {
                this.CloseRequestAsync().GetAwaiter().GetResult();
                this._channel.CloseAsync().GetAwaiter().GetResult();
            }
            catch { }
        }
    }
}
