using DotNetty.Codecs.NATS.Packets;
using System;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Hunter.NATS.Client
{
    public partial class NATSClient
    {
        public async Task ContentcAsync()
        {
            if (!_options.ClusterNodes.Any())
            {
                IPHostEntry hostInfo = Dns.GetHostEntry(_options.Host);
                _options.ClusterNodes.AddRange(hostInfo.AddressList.Select(v => new IPEndPoint(v, _options.Port)));
            }

            var ClusterNode = _options.ClusterNodes.First();

            _channel = await _bootstrap.ConnectAsync(ClusterNode);

            _info = await ConnectAsync();
        }
        private async Task<InfoPacket> ConnectAsync()
        {

            var Packet = new ConnectPacket(true, false, false, null, null, _clientId, null);

            _infoTaskCompletionSource = new TaskCompletionSource<InfoPacket>();

            await _channel.WriteAndFlushAsync(Packet);

            var ConnectInfoResult = await _infoTaskCompletionSource.Task;

            return ConnectInfoResult;

        }
        public async Task<string> SubscribeAsync(string subject, string queueGroup, Action<NATSMsgContent> handler, string subscribeId = null)
        {
            var SubscribeId = subscribeId ?? $"sid{Interlocked.Increment(ref _subscribeId)}";

            _localSubscriptionConfig[SubscribeId] = new NATSSubscriptionConfig(subject, SubscribeId, handler);

            var SubscribePacket = new SubscribePacket(SubscribeId, subject, queueGroup);

            await _channel.WriteAndFlushAsync(SubscribePacket);

            return SubscribeId;

        }
        public Task<string> SubscribeAsync(string subject, Action<NATSMsgContent> handler, string subscribeId = null)
        {
            return SubscribeAsync(subject, string.Empty, handler, subscribeId);
        }
        public async Task<string> SubscribeAsync(string subject, string queueGroup, Action<NATSMsgContent> handler)
        {
            var SubscribeId = $"sid{Interlocked.Increment(ref _subscribeId)}";

            _localSubscriptionConfig[SubscribeId] = new NATSSubscriptionConfig(subject, SubscribeId, handler);

            var SubscribePacket = new SubscribePacket(SubscribeId, subject, queueGroup);

            await _channel.WriteAndFlushAsync(SubscribePacket);

            return SubscribeId;

        }
        public Task<string> SubscribeAsync(string subject, Action<NATSMsgContent> handler)
        {
            return SubscribeAsync(subject, string.Empty, handler);
        }
        public async Task UnSubscribeAsync(string subscribeId)
        {
            if (_localSubscriptionConfig.Remove(subscribeId))
            {
                var UnSubscribePacket = new UnSubscribePacket(subscribeId);

                await _channel.WriteAndFlushAsync(UnSubscribePacket);
            }
        }
        public async Task AutoUnSubscribeAsync(string subscribeId, int max_msgs)
        {
            if (_localSubscriptionConfig.TryGetValue(subscribeId, out var subscriptionConfig))
            {
                subscriptionConfig.MaxProcessed = max_msgs;

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

        protected void MessageAsync(MessagePacket message)
        {
            if (_localSubscriptionConfig.TryGetValue(message.SubscribeId, out var subscriptionConfig))
            {
                if (subscriptionConfig.MaxProcessed.HasValue)
                {
                    subscriptionConfig.MaxProcessed--;

                    if (subscriptionConfig.MaxProcessed <= 0)
                        _localSubscriptionConfig.Remove(message.SubscribeId);
                }

                subscriptionConfig.Handler(new NATSMsgContent(message.SubscribeId, message.Subject, message.ReplyTo, message.Payload));
            }
        }
    }
}
