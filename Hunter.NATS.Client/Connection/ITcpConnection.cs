using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Hunter.NATS.Client
{
    public interface ITcpConnection
    {
        Guid ConnectionId { get; }
        IPEndPoint RemoteEndPoint { get; }
        IPEndPoint LocalEndPoint { get; }
        int SendQueueSize { get; }
        bool IsClosed { get; }

        void ReceiveAsync(Action<ITcpConnection, IEnumerable<ArraySegment<byte>>> callback);
        Task EnqueueSend(IEnumerable<ArraySegment<byte>> data);
        void Close(string reason);
    }
}
