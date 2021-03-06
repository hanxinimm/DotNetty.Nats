﻿using DotNetty.Codecs.STAN.Packets;
using DotNetty.Codecs.STAN.Protocol;
using DotNetty.Handlers.STAN;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;
using Microsoft.Extensions.Logging;
using Polly;

namespace Hunter.STAN.Client
{
    public sealed partial class STANClient
    {
        public async Task ConnectAsync(CancellationToken cancellationToken = default)
        {
            _autoResetEvent.WaitOne();

            _logger.LogInformation($"开始连接Stan客户端 客户端编号 {_clientId}");

            if (_connectionState == STANConnectionState.Connected)
            {
                _autoResetEvent.Set();
                _logger.LogWarning($"Stan客户端已经连接 客户端编号 {_clientId}");
                return;
            }

            _connectionState = STANConnectionState.Connecting;

            _logger.LogInformation($"开始执行Stan客户端 客户端编号 {_clientId}");

            await _connectPolicy.ExecuteAsync(async (_) => await ExecuteConnectAsync(), cancellationToken);

            _logger.LogInformation($"完成执行Stan客户端 客户端编号 {_clientId}");

            _autoResetEvent.Set();
        }

        public async Task<bool> CheckConnectAsync()
        {
            if (_connectionState == STANConnectionState.Connected)
            {
                return true;
            }

            _logger.LogInformation($"开始等待Stan客户端连接 客户端编号 {_clientId}");

            await ConnectAsync();

            if (_connectionState == STANConnectionState.Connected)
            {
                _logger.LogInformation($"Stan客户端已连接 客户端编号 {_clientId}");
                return true;
            }

            _logger.LogWarning($"Stan客户端未能正常连接 当前状态 {_connectionState} 客户端编号 {_clientId}");

            return false;
        }

        public async Task ExecuteConnectAsync()
        {
            _logger.LogInformation("STAN 执行客户端通讯连接频道");

            if (!_options.ClusterNodes.Any())
            {
                IPHostEntry hostInfo = Dns.GetHostEntry(_options.Host);
                _options.ClusterNodes.AddRange(hostInfo.AddressList.Select(v => new IPEndPoint(v, _options.Port)));
            }

            var ClusterNode = _options.ClusterNodes.First();

            if (_channel != null && _channel.Active)
            {
                _logger.LogWarning("STAN 开始释放断开的通讯连接频道");

                await _channel.DisconnectAsync();

                _logger.LogWarning("STAN 完成释放断开的通讯连接频道");
            }


            _logger.LogInformation("STAN 开始连接频道");

            _channel = await _bootstrap.ConnectAsync(ClusterNode);

            _logger.LogInformation("STAN 开始订阅心跳箱");

            await SubscribeHeartBeatInboxAsync();

            _logger.LogInformation("STAN 完成订阅心跳箱");

            _logger.LogInformation("STAN 开始订阅答复箱");

            await SubscribeReplyInboxAsync();

            _logger.LogInformation("STAN 完成订阅答复箱");


            if (_connectionState == STANConnectionState.Reconnecting)
            {
                var pingResponse = await ConnectPingAsync();

                if (pingResponse == null || !string.IsNullOrEmpty(pingResponse.Error))
                {
                    _logger.LogError($"STAN 连接不再有效 错误:{pingResponse?.Error}");

                    _logger.LogInformation("STAN 开始客户端连接请求");

                    _config = await ConnectRequestAsync();

                    _logger.LogInformation("STAN 完成客户端连接请求");
                }

                _logger.LogInformation("STAN 开始订阅之前订阅的消息");

                await SubscriptionMessageAsync();

                _logger.LogInformation("STAN 完成订阅之前订阅的消息");
            }
            else
            {
                _logger.LogInformation("STAN 开始客户端连接请求");

                _config = await ConnectRequestAsync();

                _logger.LogInformation("STAN 完成客户端连接请求");

            }

            _logger.LogInformation("STAN 完成连接频道");

            _connectionState = STANConnectionState.Connected;
        }

        private async Task<STANConnectionConfig> ConnectRequestAsync()
        {
            var ConnectId = Guid.NewGuid().ToString("N");

            var Packet = new ConnectRequestPacket(
                _replyInboxId,
                _options.ClusterID,
                _clientId,
                ConnectId,
                _heartbeatInboxId);

            var ConnectResponseReady = new TaskCompletionSource<ConnectResponsePacket>();

            var Handler = new ReplyPacketHandler<ConnectResponsePacket>(Packet.ReplyTo, ConnectResponseReady);

            _channel.Pipeline.AddLast(Handler);

            await _channel.WriteAndFlushAsync(Packet);

            var ConnectResponseCancellationToken = new CancellationTokenSource(TimeSpan.FromSeconds(15)).Token.Register(() =>
            {
                ConnectResponseReady.TrySetResult(null);
            });

            var ConnectResponse = await ConnectResponseReady.Task;

            await ConnectResponseCancellationToken.DisposeAsync();

            _channel.Pipeline.Remove(Handler);

            if (ConnectResponse == null) throw new StanConnectRequestException();

            if (!string.IsNullOrEmpty(ConnectResponse.Message.Error))
            {
                _logger.LogError("STAN 连接请求发生异常 {0}", ConnectResponse.Message.Error);
            }

            return new STANConnectionConfig(
                ConnectId,
                ConnectResponse.Message.PubPrefix,
                ConnectResponse.Message.SubRequests,
                ConnectResponse.Message.UnsubRequests,
                ConnectResponse.Message.CloseRequests,
                ConnectResponse.Message.SubCloseRequests,
                ConnectResponse.Message.PingRequests,
                ConnectResponse.Message.PingInterval,
                ConnectResponse.Message.PingMaxOut,
                ConnectResponse.Message.Protocol,
                ConnectResponse.Message.PublicKey);
        }

        private async Task SubscribeHeartBeatInboxAsync()
        {
            _logger.LogDebug($"开始订阅消息服务器心跳消息 HeartbeatInbox = {_heartbeatInboxId}");

            var Packet = new HeartbeatInboxPacket(_heartbeatInboxId);

            await _channel.WriteAndFlushAsync(Packet);

            _logger.LogDebug("结束订阅消息服务器心跳消息");
        }

        private async Task<PingResponse> ConnectPingAsync()
        {
            _logger.LogDebug($"开始Ping消息服务器 Ping = {_config.ConnectionId}");

            var Packet = new ConnectPingPacket(_replyInboxId, _config.PingRequests, _config.ConnectionId);

            var ConnectPingResponseReady = new TaskCompletionSource<ConnectPingResponsePacket>();

            var Handler = new ReplyPacketHandler<ConnectPingResponsePacket>(Packet.ReplyTo, ConnectPingResponseReady);

            _channel.Pipeline.AddLast(Handler);

            await _channel.WriteAndFlushAsync(Packet);

            var ConnectResponseCancellationToken = new CancellationTokenSource(TimeSpan.FromSeconds(15)).Token.Register(() =>
            {
                ConnectPingResponseReady.TrySetResult(null);
            });

            var ConnectPingResponse = await ConnectPingResponseReady.Task;

            await ConnectResponseCancellationToken.DisposeAsync();

            _channel.Pipeline.Remove(Handler);

            _logger.LogDebug("结束Ping消息服务器");

            return ConnectPingResponse?.Message;
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

                await _channel.WriteAndFlushAsync(new SubscribePacket(subscriptionMessageHandler.SubscriptionConfig.Inbox));

                _logger.LogDebug($"完成设置主题处理器 Subject = {subscriptionMessageHandler.SubscriptionConfig.Subject}");
            }
        }

        /// <summary>
        /// 订阅
        /// </summary>
        /// <param name="subject">主题</param>
        /// <param name="queueGroup">分组名称</param>
        /// <param name="persistenceName">持久化名称</param>
        /// <param name="subscribeOptions">订阅配置</param>
        /// <param name="messageHandler">消息处理</param>
        /// <returns></returns>
        private async Task<STANSubscriptionConfig> HandleSubscribeAsync(
             string subject,
             string queueGroup,
             string persistenceName,
             STANSubscribeOptions subscribeOptions,
             Func<STANSubscriptionConfig, SubscriptionMessageHandler> messageHandlerSetup)
        {

            return await _policy.ExecuteAsync(async (cnt) =>
            {
                await CheckConnectAsync();
                return await InternalSubscribeAsync(subject, queueGroup, persistenceName, subscribeOptions, messageHandlerSetup);
            }, new Dictionary<string, object>() { { "hld", "HandleSubscribeAsync" }, { "sub", subject } });
        }

        /// <summary>
        /// 订阅
        /// </summary>
        /// <param name="subject">主题</param>
        /// <param name="queueGroup">分组名称</param>
        /// <param name="persistenceName">持久化名称</param>
        /// <param name="subscribeOptions">订阅配置</param>
        /// <param name="messageHandler">消息处理</param>
        /// <returns></returns>
        private async Task<STANSubscriptionConfig> InternalSubscribeAsync(
             string subject,
             string queueGroup,
             string persistenceName,
             STANSubscribeOptions subscribeOptions,
             Func<STANSubscriptionConfig, SubscriptionMessageHandler> messageHandlerSetup)
        {
            var SubscribePacket = new SubscribePacket();

            _logger.LogDebug($"开始设置订阅消息队列收件箱 ReplyInboxId = {_replyInboxId}");

            //订阅侦听消息
            await _channel.WriteAndFlushAsync(SubscribePacket);

            _logger.LogDebug($"结束设置订阅消息队列收件箱 ReplyInboxId = {_replyInboxId}");

            var Packet = new SubscriptionRequestPacket(
                    _replyInboxId,
                    _config.SubRequests,
                    _clientId,
                    subject,
                    queueGroup,
                    SubscribePacket.Subject,
                    subscribeOptions.MaxInFlight ?? 1024,
                    subscribeOptions.AckWaitInSecs ?? 30,
                    persistenceName,
                    subscribeOptions.Position);

            if (subscribeOptions.StartSequence.HasValue)
            {
                Packet.Message.StartSequence = subscribeOptions.StartSequence.Value;
            }

            if (subscribeOptions.StartTimeDelta.HasValue)
            {
                Packet.Message.StartTimeDelta = subscribeOptions.StartTimeDelta.Value;
            }

            //订阅配置信息
            var SubscriptionConfig = new STANSubscriptionConfig(subject, Packet.ReplyTo, Packet.Message.Inbox);

            //订阅响应任务源
            var SubscriptionResponseReady = new TaskCompletionSource<SubscriptionResponsePacket>(SubscriptionConfig);

            //处理订阅响应的管道
            var SubscriptionResponseHandler = new SubscriptionResponseHandler(SubscriptionConfig, SubscriptionResponseReady);

            //添加订阅响应管道
            _channel.Pipeline.AddLast(SubscriptionResponseHandler);

            //订阅消息处理器
            var messageHandler = messageHandlerSetup(SubscriptionConfig);

            //添加消息处理到消息处理集合
            _subscriptionMessageHandler.Add(messageHandler);

            //订阅消息处理器添加到管道
            _channel.Pipeline.AddLast(messageHandler);

            _logger.LogDebug($"开始发送订阅请求 包裹主题 {Packet.Subject } 订阅主题 {Packet.Message.Subject}");

            //发送订阅请求
            await _channel.WriteAndFlushAsync(Packet);

            _logger.LogDebug($"结束发送订阅请求 包裹主题 {Packet.Subject } 订阅主题 {Packet.Message.Subject}");

            //等待订阅结果响应
            var SubscriptionResponseResult = await SubscriptionResponseReady.Task;

            //移除处理订阅响应的管道
            _channel.Pipeline.Remove(SubscriptionResponseHandler);

            //如果订阅错误,同时移除订阅消息处理管道
            if (!string.IsNullOrEmpty(SubscriptionResponseResult.Message.Error))
            {
                _channel.Pipeline.Remove(messageHandler);

                _subscriptionMessageHandler.Remove(messageHandler);

                _logger.LogError($"订阅消息发生异常 错误信息 {SubscriptionResponseResult.Message.Error}");

                //TODO:待完善异常
                throw new Exception(SubscriptionResponseResult.Message.Error);
            }

            _logger.LogDebug($"成功订阅消息 包裹主题 {Packet.Subject } 订阅主题 {Packet.Message.Subject}");

            return SubscriptionConfig;
        }

        #region 订阅自动确认 异步回调

        public async Task<STANSubscriptionConfig> SubscribeAsync(string subject, Func<STANMsgContent, ValueTask> handler)
        {
            CheckSubject(subject);
            return await SubscribeAsync(subject, string.Empty, new STANSubscribeOptions() { Position = StartPosition.NewOnly }, handler);
        }
        public async Task<STANSubscriptionConfig> SubscribeAsync(string subject, STANSubscribeOptions subscribeOptions, Func<STANMsgContent, ValueTask> handler)
        {
            CheckSubject(subject);
            return await SubscribeAsync(subject, string.Empty, subscribeOptions, handler);
        }

        public async Task<STANSubscriptionConfig> SubscribeAsync(string subject, string queueGroup, Func<STANMsgContent, ValueTask> handler)
        {
            CheckSubject(subject);
            CheckQueueGroup(queueGroup);

            return await _policy.ExecuteAsync(async (cnt) =>
            {
                await CheckConnectAsync();
                return await SubscribeAsync(subject, queueGroup, new STANSubscribeOptions() { Position = StartPosition.NewOnly }, handler);
            }, new Dictionary<string, object>() { { "hld", "SubscribeAsync" }, { "sub", subject } });
        }
        public Task<STANSubscriptionConfig> SubscribeAsync(string subject, string queueGroup, STANSubscribeOptions subscribeOptions, Func<STANMsgContent, ValueTask> handler)
        {
            CheckSubject(subject);
            return HandleSubscribeAsync(subject, queueGroup, string.Empty, subscribeOptions,
            (subscriptionConfig) => new SubscriptionMessageAsynHandler(_logger, subscriptionConfig, handler, AckAsync));
        }


        public Task<STANSubscriptionConfig> PersistenceSubscribeAsync(string subject, string PersistenceName, Func<STANMsgContent, ValueTask> handler)
        {
            return PersistenceSubscribeAsync(subject, string.Empty, PersistenceName, new STANSubscribeOptions() { Position = StartPosition.LastReceived }, handler);
        }
        public Task<STANSubscriptionConfig> PersistenceSubscribeAsync(string subject, string PersistenceName, STANSubscribeOptions subscribeOptions, Func<STANMsgContent, ValueTask> handler)
        {
            return PersistenceSubscribeAsync(subject, string.Empty, PersistenceName, subscribeOptions, handler);
        }
        public Task<STANSubscriptionConfig> PersistenceSubscribeAsync(string subject, string queueGroup, string PersistenceName, Func<STANMsgContent, ValueTask> handler)
        {
            CheckSubject(subject);
            CheckQueueGroup(queueGroup);
            return PersistenceSubscribeAsync(subject, queueGroup, PersistenceName, new STANSubscribeOptions() { Position = StartPosition.LastReceived }, handler);
        }

        public Task<STANSubscriptionConfig> PersistenceSubscribeAsync(string subject, string queueGroup, string PersistenceName, STANSubscribeOptions subscribeOptions, Func<STANMsgContent, ValueTask> handler)
        {
            return HandleSubscribeAsync(subject, queueGroup, PersistenceName, subscribeOptions,
            (subscriptionConfig) => new SubscriptionMessageAsynHandler(_logger, subscriptionConfig, handler, AckAsync));
        }

        #endregion;

        #region 订阅自动确认 同步回调

        public Task<STANSubscriptionConfig> SubscribeAsync(string subject, Action<STANMsgContent> handler)
        {
            CheckSubject(subject);
            return SubscribeAsync(subject, string.Empty, string.Empty, new STANSubscribeOptions() { Position = StartPosition.NewOnly }, handler);
        }
        public Task<STANSubscriptionConfig> SubscribeAsync(string subject, STANSubscribeOptions subscribeOptions, Action<STANMsgContent> handler)
        {
            CheckSubject(subject);
            return SubscribeAsync(subject, string.Empty, string.Empty, subscribeOptions, handler);
        }

        public Task<STANSubscriptionConfig> SubscribeAsync(string subject, string queueGroup, Action<STANMsgContent> handler)
        {
            CheckSubject(subject);
            CheckQueueGroup(queueGroup);
            return SubscribeAsync(subject, queueGroup, string.Empty, new STANSubscribeOptions() { Position = StartPosition.NewOnly }, handler);
        }
        public Task<STANSubscriptionConfig> SubscribeAsync(string subject, string queueGroup, STANSubscribeOptions subscribeOptions, Action<STANMsgContent> handler)
        {
            CheckSubject(subject);
            CheckQueueGroup(queueGroup);
            return SubscribeAsync(subject, queueGroup, string.Empty, subscribeOptions, handler);
        }

        public Task<STANSubscriptionConfig> PersistenceSubscribeAsync(string subject, string PersistenceName, Action<STANMsgContent> handler)
        {
            return SubscribeAsync(subject, string.Empty, PersistenceName, new STANSubscribeOptions() { Position = StartPosition.LastReceived }, handler);
        }
        public Task<STANSubscriptionConfig> PersistenceSubscribeAsync(string subject, string PersistenceName, STANSubscribeOptions subscribeOptions, Action<STANMsgContent> handler)
        {
            return SubscribeAsync(subject, string.Empty, PersistenceName, subscribeOptions, handler);
        }
        public Task<STANSubscriptionConfig> PersistenceSubscribeAsync(string subject, string queueGroup, string PersistenceName, Action<STANMsgContent> handler)
        {
            CheckSubject(subject);
            CheckQueueGroup(queueGroup);
            return SubscribeAsync(subject, queueGroup, PersistenceName, new STANSubscribeOptions() { Position = StartPosition.LastReceived }, handler);
        }
        public Task<STANSubscriptionConfig> SubscribeAsync(string subject, string queueGroup, string PersistenceName, STANSubscribeOptions subscribeOptions, Action<STANMsgContent> handler)
        {
            return HandleSubscribeAsync(subject, queueGroup, PersistenceName, subscribeOptions,
            (subscriptionConfig) => new SubscriptionMessageSyncHandler(_logger, subscriptionConfig, handler, AckAsync));
        }

        #endregion;

        #region 订阅手动确认 同步回调

        public Task<STANSubscriptionConfig> SubscribeAsync(string subject, Func<STANMsgContent, bool> handler)
        {
            CheckSubject(subject);
            return SubscribeAsync(subject, string.Empty, string.Empty, new STANSubscribeOptions() { Position = StartPosition.NewOnly }, handler);
        }
        public Task<STANSubscriptionConfig> SubscribeAsync(string subject, STANSubscribeOptions subscribeOptions, Func<STANMsgContent, bool> handler)
        {
            CheckSubject(subject);
            return SubscribeAsync(subject, string.Empty, string.Empty, subscribeOptions, handler);
        }

        public Task<STANSubscriptionConfig> SubscribeAsync(string subject, string queueGroup, Func<STANMsgContent, bool> handler)
        {
            CheckSubject(subject);
            CheckQueueGroup(queueGroup);
            return SubscribeAsync(subject, queueGroup, string.Empty, new STANSubscribeOptions() { Position = StartPosition.NewOnly }, handler);
        }
        public Task<STANSubscriptionConfig> SubscribeAsync(string subject, string queueGroup, STANSubscribeOptions subscribeOptions, Func<STANMsgContent, bool> handler)
        {
            CheckSubject(subject);
            CheckQueueGroup(queueGroup);
            return SubscribeAsync(subject, queueGroup, string.Empty, subscribeOptions, handler);
        }

        public Task<STANSubscriptionConfig> PersistenceSubscribeAsync(string subject, string PersistenceName, Func<STANMsgContent, bool> handler)
        {
            return SubscribeAsync(subject, string.Empty, PersistenceName, new STANSubscribeOptions() { Position = StartPosition.LastReceived }, handler);
        }
        public Task<STANSubscriptionConfig> PersistenceSubscribeAsync(string subject, string PersistenceName, STANSubscribeOptions subscribeOptions, Func<STANMsgContent, bool> handler)
        {
            return SubscribeAsync(subject, string.Empty, PersistenceName, subscribeOptions, handler);
        }
        public Task<STANSubscriptionConfig> PersistenceSubscribeAsync(string subject, string queueGroup, string PersistenceName, Func<STANMsgContent, bool> handler)
        {
            CheckSubject(subject);
            CheckQueueGroup(queueGroup);
            return SubscribeAsync(subject, queueGroup, PersistenceName, new STANSubscribeOptions() { Position = StartPosition.LastReceived }, handler);
        }

        public Task<STANSubscriptionConfig> SubscribeAsync(string subject, string queueGroup, string PersistenceName, STANSubscribeOptions subscribeOptions, Func<STANMsgContent, bool> handler)
        {
            return HandleSubscribeAsync(subject, queueGroup, PersistenceName, subscribeOptions,
            (subscriptionConfig) => new SubscriptionMessageAckSyncHandler(_logger, subscriptionConfig, handler, AckAsync));
        }

        #endregion;

        #region 订阅手动确认 异步回调

        public Task<STANSubscriptionConfig> SubscribeAsync(string subject, Func<STANMsgContent, ValueTask<bool>> handler)
        {
            CheckSubject(subject);
            return SubscribeAsync(subject, string.Empty, string.Empty, new STANSubscribeOptions() { Position = StartPosition.NewOnly }, handler);
        }
        public Task<STANSubscriptionConfig> SubscribeAsync(string subject, STANSubscribeOptions subscribeOptions, Func<STANMsgContent, ValueTask<bool>> handler)
        {
            CheckSubject(subject);
            return SubscribeAsync(subject, string.Empty, string.Empty, subscribeOptions, handler);
        }

        public Task<STANSubscriptionConfig> SubscribeAsync(string subject, string queueGroup, Func<STANMsgContent, ValueTask<bool>> handler)
        {
            CheckSubject(subject);
            CheckQueueGroup(queueGroup);
            return SubscribeAsync(subject, queueGroup, string.Empty, new STANSubscribeOptions() { Position = StartPosition.NewOnly }, handler);
        }
        public Task<STANSubscriptionConfig> SubscribeAsync(string subject, string queueGroup, STANSubscribeOptions subscribeOptions, Func<STANMsgContent, ValueTask<bool>> handler)
        {
            CheckSubject(subject);
            CheckQueueGroup(queueGroup);
            return SubscribeAsync(subject, queueGroup, string.Empty, subscribeOptions, handler);
        }


        public Task<STANSubscriptionConfig> PersistenceSubscribeAsync(string subject, string PersistenceName, Func<STANMsgContent, ValueTask<bool>> handler)
        {
            return SubscribeAsync(subject, string.Empty, PersistenceName, new STANSubscribeOptions() { Position = StartPosition.LastReceived }, handler);
        }
        public Task<STANSubscriptionConfig> PersistenceSubscribeAsync(string subject, string PersistenceName, STANSubscribeOptions subscribeOptions, Func<STANMsgContent, ValueTask<bool>> handler)
        {
            return SubscribeAsync(subject, string.Empty, PersistenceName, subscribeOptions, handler);
        }
        public Task<STANSubscriptionConfig> PersistenceSubscribeAsync(string subject, string queueGroup, string PersistenceName, Func<STANMsgContent, ValueTask<bool>> handler)
        {
            CheckSubject(subject);
            CheckQueueGroup(queueGroup);
            return SubscribeAsync(subject, queueGroup, PersistenceName, new STANSubscribeOptions() { Position = StartPosition.LastReceived }, handler);
        }

        public Task<STANSubscriptionConfig> SubscribeAsync(string subject, string queueGroup, string PersistenceName, STANSubscribeOptions subscribeOptions, Func<STANMsgContent, ValueTask<bool>> handler)
        {
            return HandleSubscribeAsync(subject, queueGroup, PersistenceName, subscribeOptions,
            (subscriptionConfig) => new SubscriptionMessageAckAsynHandler(_logger, subscriptionConfig, handler, AckAsync));
        }

        #endregion;

        #region; 订阅指定消息数量自动取消订阅

        public Task<STANSubscriptionConfig> SubscribeAsync(string subject, int maxMsg, Func<STANMsgContent, ValueTask<bool>> handler)
        {
            return SubscribeAsync(subject, string.Empty, string.Empty, maxMsg, new STANSubscribeOptions() { Position = StartPosition.NewOnly }, handler);
        }

        public Task<STANSubscriptionConfig> SubscribeAsync(string subject, int maxMsg, STANSubscribeOptions subscribeOptions, Func<STANMsgContent, ValueTask<bool>> handler)
        {
            return SubscribeAsync(subject, string.Empty, string.Empty, maxMsg, subscribeOptions, handler);
        }

        public Task<STANSubscriptionConfig> SubscribeAsync(string subject, string queueGroup, int maxMsg, Func<STANMsgContent, ValueTask<bool>> handler)
        {
            return SubscribeAsync(subject, queueGroup, string.Empty, maxMsg, new STANSubscribeOptions() { Position = StartPosition.NewOnly }, handler);
        }

        public Task<STANSubscriptionConfig> SubscribeAsync(string subject, string queueGroup, int maxMsg, STANSubscribeOptions subscribeOptions, Func<STANMsgContent, ValueTask<bool>> handler)
        {
            return SubscribeAsync(subject, queueGroup, string.Empty, maxMsg, subscribeOptions, handler);
        }

        public Task<STANSubscriptionConfig> SubscribeAsync(string subject, string queueGroup, string PersistenceName, int maxMsg, STANSubscribeOptions subscribeOptions, Func<STANMsgContent, ValueTask<bool>> handler)
        {
            return HandleSubscribeAsync(subject, queueGroup, PersistenceName, subscribeOptions,
           (subscriptionConfig) => new SubscriptionMessageAckAsynHandler(_logger, subscriptionConfig, handler, AckAsync, UnSubscribeAsync));
        }

        #endregion;

        #region; 读取消息

        public async Task<STANMsgContent> ReadFirstOrDefaultAsync(string subject, long start)
        {
            var MsgContents = await ReadAsync(subject, start, 1);
            return MsgContents.FirstOrDefault();
        }

        public Task<Queue<STANMsgContent>> ReadAsync(string subject, long start)
        {
            return ReadAsync(subject, null,new STANSubscribeOptions()
            { Position = StartPosition.SequenceStart, StartSequence = (ulong)(start < 1 ? 1 : start) });
        }

        public Task<Queue<STANMsgContent>> ReadAsync(string subject, long start, int count)
        {
            return ReadAsync(subject, count, new STANSubscribeOptions() { Position = StartPosition.SequenceStart, StartSequence = (ulong)(start < 1 ? 1 : start), MaxInFlight = count });
        }

        //TODO:待完善读取逻辑
        private async Task<Queue<STANMsgContent>> ReadAsync(string subject, int? count, STANSubscribeOptions subscribeOptions)
        {
            return await _policy.ExecuteAsync(async (cnt) =>
            {
                return await ExecuteReadAsync(subject, count, subscribeOptions);
            }, new Dictionary<string, object>() { { "hld", "ReadAsync" }, { "sub", subject } });
        }

        private async Task<Queue<STANMsgContent>> ExecuteReadAsync(string subject, int? count, STANSubscribeOptions subscribeOptions)
        {
            var SubscribePacket = new SubscribePacket();

            _logger.LogDebug($"开始设置订阅消息队列收件箱 ReplyInboxId = {_replyInboxId}");

            //订阅侦听消息
            await _channel.WriteAndFlushAsync(SubscribePacket);

            _logger.LogDebug($"结束设置订阅消息队列收件箱 ReplyInboxId = {_replyInboxId}");

            var Packet = new SubscriptionRequestPacket(
                    _replyInboxId,
                    _config.SubRequests,
                    _clientId,
                    subject,
                    null,
                    SubscribePacket.Subject,
                    subscribeOptions.MaxInFlight ?? 1024,
                    subscribeOptions.AckWaitInSecs ?? 30,
                    null,
                    subscribeOptions.Position);


            if (subscribeOptions.StartSequence.HasValue)
            {
                Packet.Message.StartSequence = subscribeOptions.StartSequence.Value;
            }

            if (subscribeOptions.StartTimeDelta.HasValue)
            {
                Packet.Message.StartTimeDelta = subscribeOptions.StartTimeDelta.Value;
            }

            //订阅配置信息
            var SubscriptionConfig = count.HasValue ? new STANSubscriptionConfig(subject, Packet.ReplyTo, Packet.Message.Inbox, count.Value) :
                new STANSubscriptionConfig(subject, Packet.ReplyTo, Packet.Message.Inbox);

            //订阅响应任务源
            var SubscriptionResponseReady = new TaskCompletionSource<SubscriptionResponsePacket>(SubscriptionConfig);

            //处理订阅响应的管道
            var SubscriptionResponseHandler = new SubscriptionResponseHandler(SubscriptionConfig, SubscriptionResponseReady);

            //添加订阅响应管道
            _channel.Pipeline.AddLast(SubscriptionResponseHandler);

            var SubscriptionMsgContentReady = new TaskCompletionSource<Queue<STANMsgContent>>(SubscriptionConfig);

            //订阅消息处理器

            var messageHandler = new ReadMessageHandler(SubscriptionConfig, SubscriptionMsgContentReady, UnSubscribeAsync);

            //订阅消息处理器添加到管道
            _channel.Pipeline.AddLast(messageHandler);

            _logger.LogDebug($"开始发送订阅请求 包裹主题 {Packet.Subject } 订阅主题 {Packet.Message.Subject}");

            //发送订阅请求
            await _channel.WriteAndFlushAsync(Packet);

            _logger.LogDebug($"结束发送订阅请求 包裹主题 {Packet.Subject } 订阅主题 {Packet.Message.Subject}");

            //等待订阅结果响应
            var SubscriptionResponseResult = await SubscriptionResponseReady.Task;

            //移除处理订阅响应的管道
            _channel.Pipeline.Remove(SubscriptionResponseHandler);

            //如果订阅错误,同时移除订阅消息处理管道
            if (!string.IsNullOrEmpty(SubscriptionResponseResult.Message.Error))
            {
                _channel.Pipeline.Remove(messageHandler);

                _logger.LogError($"订阅消息发生异常 错误信息 {SubscriptionResponseResult.Message.Error}");

                //TODO:待完善异常
                throw new Exception(SubscriptionResponseResult.Message.Error);
            }

            _logger.LogDebug($"成功订阅消息 包裹主题 {Packet.Subject } 订阅主题 {Packet.Message.Subject}");

            new CancellationTokenSource(TimeSpan.FromSeconds(5)).Token.Register(() =>
            {
                SubscriptionMsgContentReady.TrySetResult(new Queue<STANMsgContent>());
            });

            var msgContents = await SubscriptionMsgContentReady.Task;

            _channel.Pipeline.Remove(messageHandler);

            await UnSubscribeAsync(SubscriptionConfig);

            return msgContents;
        }


        #endregion;

        public async Task UnSubscribeAsync(STANSubscriptionConfig subscriptionConfig)
        {
            await _policy.ExecuteAsync(async (cnt) =>
            {
                await CheckConnectAsync();

                if (subscriptionConfig.IsUnSubscribe) return;

                subscriptionConfig.IsUnSubscribe = true;

                var Packet = new UnsubscribeRequestPacket(_replyInboxId,
                    _config.UnsubRequests,
                    _clientId,
                    subscriptionConfig.Subject,
                    subscriptionConfig.AckInbox,
                    subscriptionConfig.DurableName);

                //发送取消订阅请求
                await _channel.WriteAndFlushAsync(Packet);
            }, new Dictionary<string, object>() { { "hld", "UnSubscribeAsync" }, { "sub", subscriptionConfig.Subject } });
        }

        /// <summary>
        /// 同步发送
        /// </summary>
        /// <param name="subject">主体</param>
        /// <param name="data">数据</param>
        /// <returns></returns>
        public async Task<PubAckPacket> PublishWaitAckAsync(string subject, byte[] data)
        {


            return await _policy.ExecuteAsync(async (cnt) =>
            {
                await CheckConnectAsync();

                var Packet = new PubMsgPacket(
                    _replyInboxId,
                    _config.PubPrefix,
                    _clientId,
                    _config.ConnectionId,
                    subject,
                    data);

                var PubAckReady = new TaskCompletionSource<PubAckPacket>();

                _waitPubAckTaskSchedule[Packet.ReplyTo] = PubAckReady;

                var PublishTask = _channel.WriteAndFlushAsync(Packet);

                //发送订阅请求
                await PublishTask.ContinueWith(task => { if (task.Status != TaskStatus.RanToCompletion) PubAckReady.SetResult(null); });

                return await PubAckReady.Task;
            }, new Dictionary<string, object>() { { "hld", "PublishWaitAckAsync" }, { "sub", subject } });
        }

        /// <summary>
        /// 异步发送
        /// </summary>
        /// <param name="subject">主体</param>
        /// <param name="data">数据</param>
        /// <returns></returns>
        public async Task PublishAsync(string subject, byte[] data)
        {
            await _policy.ExecuteAsync(async (cnt) =>
            {
                await CheckConnectAsync();

                var Packet = new PubMsgPacket(
                _replyInboxId,
                _config.PubPrefix,
                _clientId,
                _config.ConnectionId,
                subject,
                data);

                await _channel.WriteAndFlushAsync(Packet);
            }, new Dictionary<string, object>() { { "hld", "PublishAsync" }, { "sub", subject } });
        }

        #region 消息发送确认


        /// <summary>
        /// 发送消息成功处理确认
        /// </summary>
        /// <param name="subscriptionConfig">订阅配置</param>
        /// <param name="msg">消息</param>
        /// <param name="isAck">是否确认</param>
        private async Task AckAsync(STANSubscriptionConfig subscriptionConfig, MsgProtoPacket msg, bool isAck = true)
        {
            if (isAck)
            {
                await _channel.WriteAndFlushAsync(new AckPacket(subscriptionConfig.AckInbox, msg.Message.Subject, msg.Message.Sequence));
            }
        }

        #endregion;
        private async Task<CloseResponsePacket> CloseRequestAsync()
        {

            var Packet = new CloseRequestPacket(_replyInboxId, _config.CloseRequests, _clientId);

            var CloseRequestReady = new TaskCompletionSource<CloseResponsePacket>();

            var Handler = new ReplyPacketHandler<CloseResponsePacket>(Packet.ReplyTo, CloseRequestReady);

            _channel.Pipeline.AddLast(Handler);

            //发送关闭
            await _channel.WriteAndFlushAsync(Packet);

            var Result = await CloseRequestReady.Task;

            _channel.Pipeline.Remove(Handler);

            _config = null;

            return Result;
        }

        public async ValueTask DisposeAsync()
        {
            _autoResetEvent.WaitOne(TimeSpan.FromSeconds(20));

            _logger.LogWarning($"开始释放Stan客户端 客户端编号 {_clientId}");

            _connectionState = STANConnectionState.Disconnecting;

            if (_channel != null && _channel.Active)
            {
                if (_channel.Open && _config != null)
                    await CloseRequestAsync();

                await _channel.DisconnectAsync();
                await _channel.EventLoop.ShutdownGracefullyAsync(TimeSpan.FromMilliseconds(100), TimeSpan.FromSeconds(1));
            }

            _connectionState = STANConnectionState.Disconnected;

            _logger.LogWarning($"结束释放Stan客户端 客户端编号 {_clientId}");

            _autoResetEvent.Set();
        }
    }
}
