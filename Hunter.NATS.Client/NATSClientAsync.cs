using DotNetty.Codecs.NATS.Packets;
using DotNetty.Handlers.NATS;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Hunter.NATS.Client
{
    public partial class NATSClient
    {
        public async Task ConnectAsync()
        {
            await _semaphoreSlim.WaitAsync();

            _logger.LogInformation($"开始连接Nats客户端 客户端编号 {_clientId}");

            if (_connectionState == NATSConnectionState.Connected)
            {
                _logger.LogError($"Nats客户端已经连接 客户端编号 {_clientId}");
                return;
            }

            _connectionState = NATSConnectionState.Connecting;

            try
            {
                if (_channel == null)
                {
                    await ExecuteConnectAsync();
                }
            }
            finally
            {
                _semaphoreSlim.Release(100);
            }

            _logger.LogInformation($"结束连接Nats客户端 客户端编号 {_clientId}");
        }

        public async Task<bool> CheckConnectAsync()
        {
            if (_connectionState == NATSConnectionState.Connected)
            {
                return true;
            }

            _logger.LogInformation($"开始等待Nats客户端连接 客户端编号 {_clientId}");

            await _semaphoreSlim.WaitAsync(TimeSpan.FromSeconds(15));

            if (_connectionState == NATSConnectionState.Connected)
            {
                _logger.LogInformation($"Nats客户端已连接 客户端编号 {_clientId}");
                return true;
            }

            _logger.LogWarning($"Nats客户端未能正常连接 客户端编号 {_clientId}");

            return false;
        }


        private async Task ReconnectAsync()
        {
            try
            {
                int retryCount = 0;

                while (true)
                {
                    try
                    {
                        await ExecuteConnectAsync();

                        break;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"NATS 连接服务器异常 第 {++retryCount} 次尝试");
                        await Task.Delay(TimeSpan.FromSeconds(3));
                    }
                }
            }
            finally
            {
                _semaphoreSlim.Release();
            }
        }

        private async Task ExecuteConnectAsync()
        {
            //TODO:集群节点待优化
            if (!_options.ClusterNodes.Any())
            {
                IPHostEntry hostInfo = Dns.GetHostEntry(_options.Host);
                _options.ClusterNodes.AddRange(hostInfo.AddressList.Select(v => new IPEndPoint(v, _options.Port)));
            }

            var ClusterNode = _options.ClusterNodes.First();

            if (_channel != null && _channel.Active)
            {
                _logger.LogDebug("NATS 开始释放断开的通讯连接频道");

                await _channel.DisconnectAsync();

                _logger.LogDebug("NATS 完成释放断开的通讯连接频道");
            }

            if (_info == null)
            {
                _channel = await _bootstrap.ConnectAsync(ClusterNode);

                _info = await ConnectRequestAsync();
            }
            else
            {
                _channel = await _bootstrap.ConnectAsync(ClusterNode);

                _info = await ConnectRequestAsync();

                _info = await ConnectRequestAsync();

                await SubscriptionMessageAsync();
            }

            _connectionState = NATSConnectionState.Connected;
        }

        private async Task<InfoPacket> ConnectRequestAsync()
        {
            var Packet = _options.IsAuthentication ?
                new ConnectPacket(_options.IsVerbose, false, false, _options.UserName, _options.Password, _clientId, null)
                : new ConnectPacket(_options.IsVerbose, false, false, _clientId);

            _infoTaskCompletionSource = new TaskCompletionSource<InfoPacket>();

            await _channel.WriteAndFlushAsync(Packet);

            _info = await _infoTaskCompletionSource.Task;

            return _info;
        }

        private async Task SubscriptionMessageAsync()
        {
            foreach (var subscriptionMessageHandler in _subscriptionMessageHandler)
            {
                _logger.LogDebug($"开始设置主题处理器 Subject = {subscriptionMessageHandler.SubscriptionConfig.Subject}");

                _channel.Pipeline.AddLast(subscriptionMessageHandler);

                await _channel.WriteAndFlushAsync(new SubscribePacket(subscriptionMessageHandler.SubscriptionConfig.SubscribeId, subscriptionMessageHandler.SubscriptionConfig.Subject, subscriptionMessageHandler.SubscriptionConfig.SubscribeGroup));

                _logger.LogDebug($"完成设置主题处理器 Subject = {subscriptionMessageHandler.SubscriptionConfig.Subject}");
            }
        }

        public async Task<string> HandleSubscribeAsync(string subject, string queueGroup,
            Func<NATSSubscriptionConfig, SubscriptionMessageHandler> messageHandlerSetup, int? maxMsg = null, string subscribeId = null)
        {

            if (!await CheckConnectAsync()) throw new NATSConnectionFailureException();

            var SubscribeId = subscribeId ?? $"sid{Interlocked.Increment(ref _subscribeId)}";

            _logger.LogDebug($"设置订阅消息队列订阅编号 Subject = {subject} QueueGroup = {queueGroup} SubscribeId = {SubscribeId}");

            var SubscriptionConfig = new NATSSubscriptionConfig(subject, SubscribeId, queueGroup,  maxMsg);

            //处理订阅响应的管道
            var messageHandler = messageHandlerSetup(SubscriptionConfig);

            _logger.LogDebug($"开始添加消息队列处理器 Subject = {subject} QueueGroup = {queueGroup} SubscribeId = {SubscribeId}");

            //添加订阅响应管道
            _channel.Pipeline.AddLast(messageHandler);

            _logger.LogDebug($"结束添加消息队列处理器 Subject = {subject} QueueGroup = {queueGroup} SubscribeId = {SubscribeId}");


            _logger.LogDebug($"开始发送订阅请求 订阅主题 {subject } 订阅编号 {SubscribeId}");

            var SubscribePacket = new SubscribePacket(SubscribeId, subject, queueGroup);

            await _channel.WriteAndFlushAsync(SubscribePacket);

            _logger.LogDebug($"结束发送订阅请求 订阅主题 {subject } 订阅编号 {SubscribeId}");

            //添加消息处理到消息处理集合
            _subscriptionMessageHandler.Add(messageHandler);

            return SubscribeId;
        }


        #region 异步处理消息

        public Task<string> SubscribeAsync(string subject, string queueGroup, Func<NATSMsgContent, ValueTask> handler, string subscribeId = null)
        {
            return HandleSubscribeAsync(subject, queueGroup, (config) =>
                new SubscriptionMessageAsynHandler(_logger, config, handler), subscribeId: subscribeId);
        }
        public Task<string> SubscribeAsync(string subject, string queueGroup, int maxMsg, Func<NATSMsgContent, ValueTask> handler)
        {
            return HandleSubscribeAsync(subject, queueGroup, (config) =>
                new SubscriptionMessageAsynHandler(_logger, config, handler, UnSubscribeAsync), maxMsg);
        }

        public Task<string> SubscribeAsync(string subject, Func<NATSMsgContent, ValueTask> handler, string subscribeId = null)
        {
            return HandleSubscribeAsync(subject, null, (config) =>
                new SubscriptionMessageAsynHandler(_logger, config, handler), subscribeId: subscribeId);
        }

        public Task<string> SubscribeAsync(string subject, int maxMsg, Func<NATSMsgContent, ValueTask> handler)
        {
            return HandleSubscribeAsync(subject, null, (config) =>
                new SubscriptionMessageAsynHandler(_logger, config, handler, UnSubscribeAsync), maxMsg: maxMsg);
        }

        #endregion;

        #region 同步处理消息

        public Task<string> SubscribeAsync(string subject, string queueGroup, Action<NATSMsgContent> handler, string subscribeId = null)
        {
            return HandleSubscribeAsync(subject, queueGroup, (config) =>
                new SubscriptionMessageSyncHandler(_logger, config, handler), subscribeId: subscribeId);
        }
        public Task<string> SubscribeAsync(string subject, string queueGroup, int maxMsg, Action<NATSMsgContent> handler)
        {
            return HandleSubscribeAsync(subject, queueGroup, (config) =>
                new SubscriptionMessageSyncHandler(_logger, config, handler, UnSubscribeAsync), maxMsg);
        }
        public Task<string> SubscribeAsync(string subject, Action<NATSMsgContent> handler, string subscribeId = null)
        {
            return HandleSubscribeAsync(subject, null, (config) =>
                new SubscriptionMessageSyncHandler(_logger, config, handler), subscribeId: subscribeId);
        }
        public Task<string> SubscribeAsync(string subject, int maxMsg, Action<NATSMsgContent> handler)
        {
            return HandleSubscribeAsync(subject, null, (config) =>
                new SubscriptionMessageSyncHandler(_logger, config, handler, UnSubscribeAsync), maxMsg: maxMsg);
        }

        #endregion;


        public async Task UnSubscribeAsync(NATSSubscriptionConfig subscriptionConfig)
        {
            if (!await CheckConnectAsync()) throw new NATSConnectionFailureException();

            var UnSubscribePacket = new UnSubscribePacket(subscriptionConfig.SubscribeId);

            await _channel.WriteAndFlushAsync(UnSubscribePacket);
        }

        //TODO:待完善逻辑，增加消息队列服务器连接断开失败后的发送消息锁，和重连消息队列发送机制

        /// <summary>
        /// 异步发送
        /// </summary>
        /// <param name="subject">主体</param>
        /// <param name="data">数据</param>
        /// <returns></returns>
        public async Task PublishAsync(string subject, byte[] data)
        {
            if (!await CheckConnectAsync()) throw new NATSConnectionFailureException();

            var Packet = new PublishPacket(subject, data);

            await _channel.WriteAndFlushAsync(Packet);
        }

        protected void InfoAsync(InfoPacket info)
        {
            _infoTaskCompletionSource.TrySetResult(info);
        }

        public async Task PingAsync()
        {
            if (!await CheckConnectAsync()) throw new NATSConnectionFailureException();

            var Packet = new PingPacket();

            await _channel.WriteAndFlushAsync(Packet);
        }

        public async Task PongAsync()
        {
            if (!await CheckConnectAsync()) throw new NATSConnectionFailureException();

            var Packet = new PongPacket();

            await _channel.WriteAndFlushAsync(Packet);
        }

        public async ValueTask DisposeAsync()
        {
            await _semaphoreSlim.WaitAsync(TimeSpan.FromSeconds(20));

            _logger.LogWarning($"开始释放Nats客户端 客户端编号 {_clientId}");

            _connectionState = NATSConnectionState.Disconnecting;

            if (_channel != null && _channel.Active)
            {
                await _channel.DisconnectAsync();

                await _channel.EventLoop.ShutdownGracefullyAsync(TimeSpan.FromMilliseconds(100), TimeSpan.FromSeconds(1));
            }

            _connectionState = NATSConnectionState.Disconnected;

            _logger.LogWarning($"结束释放Nats客户端 客户端编号 {_clientId}");

            _semaphoreSlim.Release();
        }
    }
}
