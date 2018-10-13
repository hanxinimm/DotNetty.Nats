// Copyright 2015-2018 The NATS Authors
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Security.Authentication;
using System.Globalization;
using System.Linq;

namespace Hunter.NATS.Client
{

    /// <summary>
    /// <see cref="NATSConnection"/> represents a bare connection to a NATS server.
    /// Users should create an <see cref="INATSConnection"/> instance using
    /// <see cref="ConnectionFactory"/> rather than directly using this class.
    /// </summary>
    // TODO - for a pure object model, we can create
    // an abstract subclass containing shared code between conn and 
    // encoded conn rather than using this class as
    // a base class.  This can happen anytime as we are using
    // interfaces.
    public class NATSConnection : INATSConnection
    {
        private readonly NATSOptions _options;
        private readonly ITcpConnection _tcpConnection;
        Object flusherLock = new Object();
        bool flusherKicked = false;
        object mu = new object();
        public NATSConnection(NATSOptions options,ITcpConnection tcpConnection)
        {
            _options = options;
            _tcpConnection = tcpConnection;
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        #region Async 异步方法

        public Task PublishAsync(string subject, byte[] data)
        {
            return PublishInternalAsync(subject, null, data, 0, data?.Length ?? 0);
        }

        public Task PublishAsync(string subject, byte[] data, int offset, int count)
        {
            throw new NotImplementedException();
        }

        public Task PublishAsync(Message message)
        {
            throw new NotImplementedException();
        }

        public Task PublishAsync(string subject, string reply, byte[] data)
        {
            throw new NotImplementedException();
        }

        public Task PublishAsync(string subject, string reply, byte[] data, int offset, int count)
        {
            throw new NotImplementedException();
        }

        // publish is the internal function to publish messages to a nats-server.
        // Sends a protocol data message by queueing into the bufio writer
        // and kicking the flush go routine. These writes should be protected.
        internal Task PublishInternalAsync(string subject, string reply, byte[] data, int offset, int count)
        {
            if (string.IsNullOrWhiteSpace(subject))
                throw new NATSBadSubscriptionException();
            if (offset < 0)
                throw new ArgumentOutOfRangeException("offset");
            if (count < 0)
                throw new ArgumentOutOfRangeException("count");
            if (data != null && data.Length - offset < count)
                throw new ArgumentException("Invalid offset and count for supplied data");

            lock (mu)
            {
                // Proactively reject payloads over the threshold set by server.

                //TODO:待判断服务器支持的最大载体
                //if (count > _options.MaxPayloadCapacity)
                //    throw new NATSMaxPayloadException();

                //TODO:找到合适位置判断
                //if (isClosed())
                //    throw new NATSConnectionClosedException();

                //TODO:判断最后一次读取是否有异常
                //if (lastEx != null)
                //    throw lastEx;

                var PublishProtocolBuffer = new byte[0];

                PublishProtocol.EnsurePublishProtocolBuffer(subject, reply, PublishProtocolBuffer);

                // write our pubProtoBuf buffer to the buffered writer.
                int PublishProtocolBufferLength = PublishProtocol.WritePublishProtocolBuffer(subject, reply, count, PublishProtocolBuffer);

                //var bw = new MemoryStream();

                //bw.Write(PublishProtocolBuffer, 0, PublishProtocolBufferLength);

                //if (count > 0)
                //{
                //    bw.Write(data, offset, count);
                //}
                //bw.Write(CRLF_BYTES, 0, CRLF_BYTES_LEN);

                var segments = new ArraySegment<byte>[3]
                {
                    new ArraySegment<byte>(PublishProtocolBuffer,0,PublishProtocolBufferLength),
                    new ArraySegment<byte>(data,offset,count),
                    new ArraySegment<byte>(PublishProtocol.CRLF_BYTES,offset,PublishProtocol.CRLF_BYTES_LEN)
                };

                return _tcpConnection.EnqueueSend(segments);

                //TODO:增加消息统计
                //stats.outMsgs++;
                //stats.outBytes += count;

                //TODO:清除锁定
                //kickFlusher();
            }

        } // publish

        private void kickFlusher()
        {
            lock (flusherLock)
            {
                if (!flusherKicked)
                    Monitor.Pulse(flusherLock);

                flusherKicked = true;
            }
        }

        public Task<Message> RequestAsync(string subject, byte[] data, int timeout)
        {
            throw new NotImplementedException();
        }

        public Task<Message> RequestAsync(string subject, byte[] data, int offset, int count, int timeout)
        {
            throw new NotImplementedException();
        }

        public Task<Message> RequestAsync(string subject, byte[] data)
        {
            throw new NotImplementedException();
        }

        public Task<Message> RequestAsync(string subject, byte[] data, int offset, int count)
        {
            throw new NotImplementedException();
        }

        public Task<Message> RequestAsync(string subject, byte[] data, int timeout, CancellationToken token)
        {
            throw new NotImplementedException();
        }

        public Task<Message> RequestAsync(string subject, byte[] data, CancellationToken token)
        {
            throw new NotImplementedException();
        }

        public Task<Message> RequestAsync(string subject, byte[] data, int offset, int count, CancellationToken token)
        {
            throw new NotImplementedException();
        }

        public IAsyncSubscription SubscribeAsync(string subject)
        {
            throw new NotImplementedException();
        }

        public IAsyncSubscription SubscribeAsync(string subject, EventHandler<MessageHandlerEventArgs> handler)
        {
            throw new NotImplementedException();
        }

        public IAsyncSubscription SubscribeAsync(string subject, string queue)
        {
            throw new NotImplementedException();
        }

        public IAsyncSubscription SubscribeAsync(string subject, string queue, EventHandler<MessageHandlerEventArgs> handler)
        {
            throw new NotImplementedException();
        }


        #endregion;

        #region 非异步方法

        public void Publish(string subject, byte[] data)
        {
            throw new NotImplementedException();
        }

        public void Publish(string subject, byte[] data, int offset, int count)
        {
            throw new NotImplementedException();
        }

        public void Publish(Message message)
        {
            throw new NotImplementedException();
        }

        public void Publish(string subject, string reply, byte[] data)
        {
            throw new NotImplementedException();
        }

        public void Publish(string subject, string reply, byte[] data, int offset, int count)
        {
            throw new NotImplementedException();
        }

        public Message Request(string subject, byte[] data, int timeout)
        {
            throw new NotImplementedException();
        }

        public Message Request(string subject, byte[] data, int offset, int count, int timeout)
        {
            throw new NotImplementedException();
        }

        public Message Request(string subject, byte[] data)
        {
            throw new NotImplementedException();
        }

        public Message Request(string subject, byte[] data, int offset, int count)
        {
            throw new NotImplementedException();
        }

        public ISyncSubscription Subscribe(string subject)
        {
            throw new NotImplementedException();
        }

        public ISyncSubscription Subscribe(string subject, string queue)
        {
            throw new NotImplementedException();
        }

        public IAsyncSubscription Subscribe(string subject, EventHandler<MessageHandlerEventArgs> handler)
        {
            throw new NotImplementedException();
        }

        public IAsyncSubscription Subscribe(string subject, string queue, EventHandler<MessageHandlerEventArgs> handler)
        {
            throw new NotImplementedException();
        }

        #endregion;
    }
}
