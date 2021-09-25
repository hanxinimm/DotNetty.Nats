using DotNetty.Codecs.NATS.Packets;
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
        async Task ChannelConnectAsync()
        {

            _logger.LogInformation($"开始连接Nats客户端 客户端编号 {_clientId}");

            _connectionState = NATSConnectionState.Connecting;

            _logger.LogInformation($"开始执行Nats客户端 客户端编号 {_clientId}");

            await _connectPolicy.ExecuteAsync(async () => await ExecuteConnectAsync());

            _logger.LogInformation($"完成执行Nats客户端 客户端编号 {_clientId}");
        }

        async Task ExecuteConnectAsync()
        {
            //TODO:集群节点待优化
            if (!_options.ClusterNodes.Any())
            {
                IPHostEntry hostInfo = Dns.GetHostEntry(_options.Host);
                _options.ClusterNodes.AddRange(hostInfo.AddressList.Select(v => new IPEndPoint(v, _options.Port)));
            }

            var ClusterNode = _options.ClusterNodes.First();

            if (_info == null)
            {
                _embed_channel = await _bootstrap.ConnectAsync(ClusterNode);

                _info = await ConnectRequestAsync();

                if (_info.JetStream)
                    await SubscribeReplyInboxAsync(_embed_channel);

                _connectionState = NATSConnectionState.Connected;
            }
            else
            {
                _embed_channel = await _bootstrap.ConnectAsync(ClusterNode);

                _info = await ConnectRequestAsync();

                if (_info.JetStream)
                    await SubscribeReplyInboxAsync(_embed_channel);

                await SubscriptionMessageAsync();

                _connectionState = NATSConnectionState.Connected;
            }
        }

        private async Task<InfoPacket> ConnectRequestAsync()
        {
            var Packet = _options.IsAuthentication ?
                new ConnectPacket(_options.IsVerbose, false, false, _options.UserName, _options.Password, _clientId, null)
                : new ConnectPacket(_options.IsVerbose, false, false, _clientId);

            await _embed_channel.WriteAndFlushAsync(Packet);

            _info = await _infoTaskCompletionSource.Task;

            return _info;
        }

        private async Task SubscriptionMessageAsync()
        {
            foreach (var subscriptionMessageHandler in _subscriptionMessageHandler)
            {
                _logger.LogDebug($"开始设置主题处理器 Subject = {subscriptionMessageHandler.SubscriptionConfig.Subject}");

                _embed_channel.Pipeline.AddLast(subscriptionMessageHandler);

                await _embed_channel.WriteAndFlushAsync(new SubscribePacket(subscriptionMessageHandler.SubscriptionConfig.SubscribeId, subscriptionMessageHandler.SubscriptionConfig.Subject, subscriptionMessageHandler.SubscriptionConfig.SubscribeGroup));

                _logger.LogDebug($"完成设置主题处理器 Subject = {subscriptionMessageHandler.SubscriptionConfig.Subject}");
            }
        }

        public async Task<string> HandleSubscribeAsync(string subject, string queueGroup,
            Func<NATSSubscriptionConfig, SubscriptionMessageHandler> messageHandlerSetup, int? maxMsg = null, string subscribeId = null)
        {
            return await _policy.ExecuteAsync(async (content) =>
            {
                return await InternalSubscribeAsync(subject, queueGroup, messageHandlerSetup, maxMsg, subscribeId);
            }, new Dictionary<string, object>() { { "hld", "HandleSubscribeAsync" } });
        }

        

        public async Task<string> InternalSubscribeAsync(string subject, string queueGroup,
            Func<NATSSubscriptionConfig, SubscriptionMessageHandler> messageHandlerSetup, int? maxMsg = null, string subscribeId = null)
        {
            var _channel = await ConnectAsync();

            var SubscribeId = subscribeId ?? $"sid{Interlocked.Increment(ref _subscribeId)}";

            _logger.LogDebug($"设置订阅消息队列订阅编号 Subject = {subject} QueueGroup = {queueGroup} SubscribeId = {SubscribeId}");

            var SubscriptionConfig = new NATSSubscriptionConfig(subject, SubscribeId, queueGroup, maxMsg);

            //处理订阅响应的管道
            var messageHandler = messageHandlerSetup(SubscriptionConfig);

            _logger.LogDebug($"开始添加消息队列处理器 Subject = {subject} QueueGroup = {queueGroup} SubscribeId = {SubscribeId}");

            //添加订阅响应管道
            _channel.Pipeline.AddLast(messageHandler);

            _logger.LogDebug($"结束添加消息队列处理器 Subject = {subject} QueueGroup = {queueGroup} SubscribeId = {SubscribeId}");


            _logger.LogDebug($"开始发送订阅请求 订阅主题 {subject } 订阅编号 {SubscribeId}");

            var SubscribePacketMsg = new SubscribePacket(SubscribeId, subject, queueGroup);

            await _channel.WriteAndFlushAsync(SubscribePacketMsg);

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
            await _policy.ExecuteAsync(async (content) =>
            {
                var _channel = await ConnectAsync();

                var UnSubscribePacket = new UnSubscribePacket(subscriptionConfig.SubscribeId);

                await _channel.WriteAndFlushAsync(UnSubscribePacket);
            }, new Dictionary<string, object>() { { "hld", "UnSubscribeAsync" } });
        }

        /// <summary>
        /// 异步发送
        /// </summary>
        /// <param name="subject">主体</param>
        /// <param name="data">数据</param>
        /// <returns></returns>
        public async Task PublishAsync(string subject, byte[] data)
        {
            await _policy.ExecuteAsync(async (content) =>
            {
                var _channel = await ConnectAsync();

                var Packet = new PublishPacket(subject, data);

                await _channel.WriteAndFlushAsync(Packet);
            }, new Dictionary<string, object>() { { "hld", "PublishAsync" } });
        }

        protected void InfoAsync(InfoPacket info)
        {
            _infoTaskCompletionSource.TrySetResult(info);
        }

        public async Task PingAsync()
        {
            await _policy.ExecuteAsync(async (content) =>
            {
                var _channel = await ConnectAsync();

                var Packet = new PingPacket();

                await _channel.WriteAndFlushAsync(Packet);
            }, new Dictionary<string, object>() { { "hld", "PingAsync" } });
        }

        public async Task PongAsync()
        {
            await _policy.ExecuteAsync(async (content) =>
            {
                var _channel = await ConnectAsync();

                var Packet = new PongPacket();

                await _channel.WriteAndFlushAsync(Packet);
            }, new Dictionary<string, object>() { { "hld", "PongAsync" } });
        }

        public async ValueTask DisposeAsync()
        {
            _logger.LogWarning($"开始释放Nats客户端 客户端编号 {_clientId}");

            _connectionState = NATSConnectionState.Disconnecting;

            var _channel = await ConnectAsync(TimeSpan.FromSeconds(5));

            if (_channel != null && _channel.Active)
            {
                await _channel.DisconnectAsync();

                await _channel.EventLoop.ShutdownGracefullyAsync(TimeSpan.FromMilliseconds(100), TimeSpan.FromSeconds(1));
            }

            _connectionState = NATSConnectionState.Disconnected;

            _logger.LogWarning($"结束释放Nats客户端 客户端编号 {_clientId}");
        }
    }
}
