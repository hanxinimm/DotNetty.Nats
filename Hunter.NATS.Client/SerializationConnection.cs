using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Hunter.NATS.Client
{

    /// <summary>
    /// Represents an <see cref="NATSConnection"/> which uses a client specified
    /// encoding scheme.
    /// </summary>
    public class SerializationConnection : NATSConnection, ISerializationConnectionAsync
    {
        private readonly INATSSerializer _serializer;
        public SerializationConnection(NATSOptions options, ITcpConnection tcpConnection, INATSSerializer serializer) : base(options, tcpConnection)
        {
            _serializer = serializer;
        }

        public Task PublishAsync(string subject, object obj)
        {
            return PublishAsync(subject, _serializer.Serializer(obj));
        }

        public Task PublishAsync(string subject, string reply, object obj)
        {
            return PublishAsync(subject, reply, _serializer.Serializer(obj));
        }

        public Task<T> RequestAsync<T>(string subject, object obj, int timeout)
        {
            throw new NotImplementedException();
        }

        public Task<T> RequestAsync<T>(string subject, object obj)
        {
            throw new NotImplementedException();
        }

        public IAsyncSubscription SubscribeAsync(string subject, EventHandler<EncodedMessageEventArgs> handler)
        {
            throw new NotImplementedException();
        }

        public IAsyncSubscription SubscribeAsync(string subject, string queue, EventHandler<EncodedMessageEventArgs> handler)
        {
            throw new NotImplementedException();
        }
    }
}
