﻿using DotNetty.Codecs.NATS.Packets;
using DotNetty.Codecs.NATSJetStream.Packets;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Hunter.NATS.Client
{
    public partial class NATSClient
    {
        public async Task ConnectAsync(CancellationToken cancellationToken = default)
        {
            _manualResetEvent.WaitOne();
            _manualResetEvent.Reset();

            _logger.LogInformation($"开始连接Nats客户端 客户端编号 {_clientId}");

            if (_connectionState == NATSConnectionState.Connected)
            {
                _logger.LogError($"Nats客户端已经连接 客户端编号 {_clientId}");
                return;
            }

            _connectionState = NATSConnectionState.Connecting;

            await _connectPolicy.ExecuteAsync((_) => ExecuteConnectAsync(), cancellationToken);

            _manualResetEvent.Set();
        }
    

        public async Task<bool> CheckConnectAsync()
        {
            if (_connectionState == NATSConnectionState.Connected)
            {
                return true;
            }

            _logger.LogInformation($"开始等待Nats客户端连接 客户端编号 {_clientId}");

            await ConnectAsync();

            if (_connectionState == NATSConnectionState.Connected)
            {
                _logger.LogInformation($"Nats客户端已连接 客户端编号 {_clientId}");
                return true;
            }

            _logger.LogWarning($"Nats客户端未能正常连接 当前状态 {_connectionState} 客户端编号 {_clientId}");

            return false;
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

                await SubscribeReplyInboxAsync();
            }
            else
            {
                _channel = await _bootstrap.ConnectAsync(ClusterNode);

                _info = await ConnectRequestAsync();

                await SubscribeReplyInboxAsync();

                await SubscriptionMessageAsync();
            }

            _connectionState = NATSConnectionState.Connected;
        }

        private async Task<DotNetty.Codecs.NATS.Packets.InfoPacket> ConnectRequestAsync()
        {
            var Packet = _options.IsAuthentication ?
                new ConnectPacket(_options.IsVerbose, false, false, _options.UserName, _options.Password, _clientId, null)
                : new ConnectPacket(_options.IsVerbose, false, false, _clientId);

            _infoTaskCompletionSource = new TaskCompletionSource<DotNetty.Codecs.NATS.Packets.InfoPacket>();

            await _channel.WriteAndFlushAsync(Packet);

            _info = await _infoTaskCompletionSource.Task;

            return _info;
        }

        private async Task SubscribeReplyInboxAsync()
        {
            _logger.LogDebug($"开始设置消息队列收件箱 ReplyInboxId = {_replyInboxId}");

            await _channel.WriteAndFlushAsync(new InboxPacket(DateTime.Now.Ticks.ToString(), _replyInboxId));

            _logger.LogDebug($"结束设置消息队列收件箱 ReplyInboxId = {_replyInboxId}");
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

            return await _policy.ExecuteAsync(async () =>
            {
                await CheckConnectAsync();
                return await InternalSubscribeAsync(subject, queueGroup, messageHandlerSetup, maxMsg, subscribeId);
            });
        }

        

        public async Task<string> InternalSubscribeAsync(string subject, string queueGroup,
            Func<NATSSubscriptionConfig, SubscriptionMessageHandler> messageHandlerSetup, int? maxMsg = null, string subscribeId = null)
        {
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
            await _policy.ExecuteAsync(async () =>
            {
                await CheckConnectAsync();

                var UnSubscribePacket = new UnSubscribePacket(subscriptionConfig.SubscribeId);

                await _channel.WriteAndFlushAsync(UnSubscribePacket);
            });
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
            await _policy.ExecuteAsync(async () =>
            {
                await CheckConnectAsync();

                var Packet = new PublishPacket(subject, data);

                await _channel.WriteAndFlushAsync(Packet);
            });
        }

        protected void InfoAsync(DotNetty.Codecs.NATS.Packets.InfoPacket info)
        {
            _infoTaskCompletionSource.TrySetResult(info);
        }

        public async Task PingAsync()
        {
            await _policy.ExecuteAsync(async () =>
            {
                await CheckConnectAsync();

                var Packet = new PingPacket();

                await _channel.WriteAndFlushAsync(Packet);
            });
        }

        public async Task PongAsync()
        {
            await _policy.ExecuteAsync(async () =>
            {
                await CheckConnectAsync();

                var Packet = new PongPacket();

                await _channel.WriteAndFlushAsync(Packet);
            });
        }

        public async ValueTask DisposeAsync()
        {
            //TODO:待完善逻辑
            _manualResetEvent.WaitOne(TimeSpan.FromSeconds(20));

            _logger.LogWarning($"开始释放Nats客户端 客户端编号 {_clientId}");

            _connectionState = NATSConnectionState.Disconnecting;

            if (_channel != null && _channel.Active)
            {
                await _channel.DisconnectAsync();

                await _channel.EventLoop.ShutdownGracefullyAsync(TimeSpan.FromMilliseconds(100), TimeSpan.FromSeconds(1));
            }

            _connectionState = NATSConnectionState.Disconnected;

            _logger.LogWarning($"结束释放Nats客户端 客户端编号 {_clientId}");

            _manualResetEvent.Set();
        }
    }
}
