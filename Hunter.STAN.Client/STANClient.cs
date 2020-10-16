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
    public partial class STANClient : IAsyncDisposable
    {
        private readonly ILogger _logger;
        /// <summary>
        /// STAN配置
        /// </summary>
        private readonly STANOptions _options;

        /// <summary>
        /// 客户端标识
        /// </summary>
        public readonly string _identity;

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
        private ConcurrentDictionary<string, TaskCompletionSource<PubAckPacket>> _waitPubAckTaskSchedule 
            = new ConcurrentDictionary<string, TaskCompletionSource<PubAckPacket>>();

        public STANClient(
            ILogger<STANClient> logger,
            STANOptions options)
        {
            _options = options ?? throw new ArgumentNullException(nameof(STANOptions));
            _identity = Guid.NewGuid().ToString("n");
            _clientId = _options.ClientId;
            _heartbeatInboxId = _identity;
            _replyInboxId = _identity;
            _bootstrap = InitBootstrap();
            _logger = logger;
        }

        public STANClient(
            ILogger<STANClient> logger,
            IOptions<STANOptions> options) : this(logger, options.Value)
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
                channel.Pipeline.AddLast(STANEncoder.Instance, STANDecoder.Instance);
                channel.Pipeline.AddLast(new ReconnectChannelHandler(_logger, ReconnectIfNeedAsync));
                channel.Pipeline.AddLast(new ErrorPacketHandler(_logger));
                channel.Pipeline.AddLast(new HeartbeatPacketHandler());
                channel.Pipeline.AddLast(new PubAckPacketSyncHandler(_logger, _waitPubAckTaskSchedule));
                channel.Pipeline.AddLast(new PubAckPacketAsynSyncHandler(_logger));
                channel.Pipeline.AddLast(new PingPacketHandler(_logger));
                channel.Pipeline.AddLast(new PongPacketHandler(_logger));
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
                _logger.LogDebug("STAN 开始重新连接");

                //await this.semaphoreSlim.WaitAsync();
                try
                {
                    if (this.IsChannelInactive)
                    {
                        while (true)
                        {
                            try
                            {
                                _logger.LogDebug("STAN 开始尝试重新连接");

                                await ContentcAsync();

                                _logger.LogDebug("STAN 结束尝试重新连接");

                                break;
                            }
                            catch (Exception ex)
                            {
                                _logger.LogError(ex, "STAN 尝试重新连接异常");
                                await Task.Delay(TimeSpan.FromSeconds(3));
                            }
                        }
                        // this.clientRpcHandler = channel.Pipeline.Get<RpcClientHandler>();
                    }
                }
                finally
                {
                    _logger.LogDebug("STAN 完成重新连接");
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

        public async ValueTask DisposeAsync()
        {
            await CloseRequestAsync();
            await _channel.CloseAsync();
        }
    }
}
