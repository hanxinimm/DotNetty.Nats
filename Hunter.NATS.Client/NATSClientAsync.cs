﻿using DotNetty.Codecs.NATS.Packets;
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
        /// <summary>
        /// 本地订阅配置
        /// </summary>
        private Dictionary<string, NATSSubscriptionAsyncConfig> _localSubscriptionAsyncConfig = new Dictionary<string, NATSSubscriptionAsyncConfig>();

        public async Task ConnectAsync()
        {
            if (!_options.ClusterNodes.Any())
            {
                IPHostEntry hostInfo = Dns.GetHostEntry(_options.Host);
                _options.ClusterNodes.AddRange(hostInfo.AddressList.Select(v => new IPEndPoint(v, _options.Port)));
            }

            var ClusterNode = _options.ClusterNodes.First();

            if (_channel != null)
            {
                _logger.LogDebug("NATS 开始释放断开的通讯连接频道");

                await _channel.DisconnectAsync();
                await _channel.CloseAsync();

                _logger.LogDebug("NATS 完成释放断开的通讯连接频道");
            }

            _channel = await _bootstrap.ConnectAsync(ClusterNode);

            _info = await ConnectRequestAsync();
        }
        private async Task<InfoPacket> ConnectRequestAsync()
        {

            var Packet = new ConnectPacket(true, false, false, null, null, _clientId, null);

            _infoTaskCompletionSource = new TaskCompletionSource<InfoPacket>();

            await _channel.WriteAndFlushAsync(Packet);

            var ConnectInfoResult = await _infoTaskCompletionSource.Task;

            return ConnectInfoResult;

        }

        #region 异步处理消息

        public async Task<string> SubscribeAsync(string subject, string queueGroup, Func<NATSMsgContent, ValueTask> handler, string subscribeId = null)
        {
            var SubscribeId = subscribeId ?? $"sid{Interlocked.Increment(ref _subscribeId)}";

            _localSubscriptionAsyncConfig[SubscribeId] = new NATSSubscriptionAsyncConfig(subject, SubscribeId, handler);

            var SubscribePacket = new SubscribePacket(SubscribeId, subject, queueGroup);

            await _channel.WriteAndFlushAsync(SubscribePacket);

            return SubscribeId;
        }
        public async Task<string> SubscribeAsync(string subject, string queueGroup, int maxMsg, Func<NATSMsgContent, ValueTask> handler)
        {
            var SubscribeId = $"sid{Interlocked.Increment(ref _subscribeId)}";

            _localSubscriptionAsyncConfig[SubscribeId] = new NATSSubscriptionAsyncConfig(subject, SubscribeId, maxMsg, handler);

            var SubscribePacket = new SubscribePacket(SubscribeId, subject, queueGroup);

            await _channel.WriteAndFlushAsync(SubscribePacket);

            return SubscribeId;
        }
        public async Task<string> SubscribeAsync(string subject, Func<NATSMsgContent, ValueTask> handler, string subscribeId = null)
        {
            var SubscribeId = subscribeId ?? $"sid{Interlocked.Increment(ref _subscribeId)}";

            _localSubscriptionAsyncConfig[SubscribeId] = new NATSSubscriptionAsyncConfig(subject, SubscribeId, handler);

            var SubscribePacket = new SubscribePacket(SubscribeId, subject);

            await _channel.WriteAndFlushAsync(SubscribePacket);

            return SubscribeId;
        }
        public async Task<string> SubscribeAsync(string subject, int maxMsg, Func<NATSMsgContent, ValueTask> handler)
        {
            var SubscribeId = $"sid{Interlocked.Increment(ref _subscribeId)}";

            _localSubscriptionAsyncConfig[SubscribeId] = new NATSSubscriptionAsyncConfig(subject, SubscribeId, maxMsg, handler);

            var SubscribePacket = new SubscribePacket(SubscribeId, subject);

            await _channel.WriteAndFlushAsync(SubscribePacket);

            return SubscribeId;
        }

        #endregion;

        #region 同步处理消息

        public async Task<string> SubscribeAsync(string subject, string queueGroup, Action<NATSMsgContent> handler, string subscribeId = null)
        {
            var SubscribeId = subscribeId ?? $"sid{Interlocked.Increment(ref _subscribeId)}";

            _localSubscriptionAsyncConfig[SubscribeId] = new NATSSubscriptionAsyncConfig(subject, SubscribeId, handler);

            var SubscribePacket = new SubscribePacket(SubscribeId, subject, queueGroup);

            await _channel.WriteAndFlushAsync(SubscribePacket);

            return SubscribeId;
        }
        public async Task<string> SubscribeAsync(string subject, string queueGroup, int maxMsg, Action<NATSMsgContent> handler)
        {
            var SubscribeId = $"sid{Interlocked.Increment(ref _subscribeId)}";

            _localSubscriptionAsyncConfig[SubscribeId] = new NATSSubscriptionAsyncConfig(subject, SubscribeId, maxMsg, handler);

            var SubscribePacket = new SubscribePacket(SubscribeId, subject, queueGroup);

            await _channel.WriteAndFlushAsync(SubscribePacket);

            return SubscribeId;
        }
        public async Task<string> SubscribeAsync(string subject, Action<NATSMsgContent> handler, string subscribeId = null)
        {
            var SubscribeId = subscribeId ?? $"sid{Interlocked.Increment(ref _subscribeId)}";

            _localSubscriptionAsyncConfig[SubscribeId] = new NATSSubscriptionAsyncConfig(subject, SubscribeId, handler);

            var SubscribePacket = new SubscribePacket(SubscribeId, subject);

            await _channel.WriteAndFlushAsync(SubscribePacket);

            return SubscribeId;
        }
        public async Task<string> SubscribeAsync(string subject, int maxMsg, Action<NATSMsgContent> handler)
        {
            var SubscribeId = $"sid{Interlocked.Increment(ref _subscribeId)}";

            _localSubscriptionAsyncConfig[SubscribeId] = new NATSSubscriptionAsyncConfig(subject, SubscribeId, maxMsg, handler);

            var SubscribePacket = new SubscribePacket(SubscribeId, subject);

            await _channel.WriteAndFlushAsync(SubscribePacket);

            return SubscribeId;
        }

        #endregion;


        public async Task UnSubscribeAsync(string subscribeId)
        {
            if (_localSubscriptionAsyncConfig.Remove(subscribeId))
            {
                var UnSubscribePacket = new UnSubscribePacket(subscribeId);

                await _channel.WriteAndFlushAsync(UnSubscribePacket);
            }
        }

        public async Task AutoUnSubscribeAsync(string subscribeId, int max_msgs)
        {
            if (_localSubscriptionAsyncConfig.TryGetValue(subscribeId, out var subscriptionConfig))
            {
                subscriptionConfig.MaxMsg = max_msgs;

                var UnSubscribePacket = new UnSubscribePacket(subscribeId, max_msgs);

                await _channel.WriteAndFlushAsync(UnSubscribePacket);
            }
        }

        /// <summary>
        /// 异步发送
        /// </summary>
        /// <param name="subject">主体</param>
        /// <param name="data">数据</param>
        /// <returns></returns>
        public Task PublishAsync(string subject, byte[] data)
        {
            var Packet = new PublishPacket(subject, data);

            return _channel.WriteAndFlushAsync(Packet);
        }

        public Task PingAsync()
        {
            var Packet = new PingPacket();

            return _channel.WriteAndFlushAsync(Packet);
        }

        public Task PongAsync()
        {
            var Packet = new PongPacket();

            return _channel.WriteAndFlushAsync(Packet);
        }

        protected void InfoAsync(InfoPacket info)
        {
            _infoTaskCompletionSource.TrySetResult(info);
        }

        protected void MessageProcessingAsync(MessagePacket message)
        {
            if (_localSubscriptionAsyncConfig.TryGetValue(message.SubscribeId, out var subscriptionConfig))
            {
                if (subscriptionConfig.MaxMsg.HasValue)
                {
                    subscriptionConfig.MaxMsg--;

                    if (subscriptionConfig.MaxMsg <= 0)
                        _localSubscriptionAsyncConfig.Remove(message.SubscribeId);
                }

                if (subscriptionConfig.IsAsyncHandler)
                {
                    subscriptionConfig.AsyncHandler(
                        new NATSMsgContent(message.SubscribeId, message.Subject, message.ReplyTo, message.Payload)
                        ).GetAwaiter().GetResult();
                }
                else
                {
                    subscriptionConfig.Handler(new NATSMsgContent(message.SubscribeId, message.Subject, message.ReplyTo, message.Payload));
                }
            }
        }

        public Task CloseAsync()
        {
            return _channel.CloseAsync();
        }
    }
}
