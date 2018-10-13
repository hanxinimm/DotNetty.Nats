using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Hunter.NATS.Client
{
    /// <summary>
    /// Convenience class representing the TCP connection to prevent 
    /// managing two variables throughout the NATs client code.
    /// </summary>
    internal sealed class SslTcpConnection : ITcpConnection, IDisposable
    {

        private readonly NATSOptions _options;
        private readonly ILogger<SslTcpConnection> _logger;
        private TcpClient _client;
        private SslStream _sslStream;
        public SslTcpConnection(NATSOptions options, ILogger<SslTcpConnection> logger, 
            Action<ITcpConnection> onConnectionEstablished,
            Action<ITcpConnection, SocketError> onConnectionFailed,
            Action<ITcpConnection, SocketError> onConnectionClosed)
        {
            _options = options;
            _logger = logger;
            _client = new TcpClient();
        }

        internal async Task ConnectAsync(Srv s, int timeoutMillis)
        {
            var CancellationToken = new CancellationTokenSource(TimeSpan.FromSeconds(timeoutMillis));
            await Task.Run(() => _client.ConnectAsync(s.url.Host, s.url.Port), CancellationToken.Token);

            if (CancellationToken.IsCancellationRequested)
            {
                //_client = null;
                throw new NATSConnectionException("timeout");
            }

            _client.NoDelay = false;

            _client.ReceiveBufferSize = DefaultsOptions.DefaultBufSize * 2;
            _client.SendBufferSize = DefaultsOptions.DefaultBufSize;

            _sslStream = new SslStream(
                _client.GetStream(),
                false,
                new RemoteCertificateValidationCallback(ValidateServerCertificate),
                null
                );

            try
            {
                await _sslStream.AuthenticateAsClientAsync(s.url.Host, _options.certificates, SslProtocols.Tls12, true);
            }
            catch (AuthenticationException exc)
            {
                _logger.LogInformation(exc, "[S{0}, L{1}]: Authentication exception on BeginAuthenticateAsClient.", RemoteEndPoint, LocalEndPoint);
                CloseInternal(SocketError.SocketError, exc.Message);
            }
            catch (ObjectDisposedException)
            {
                CloseInternal(SocketError.SocketError, "SslStream disposed.");
            }
            catch (Exception exc)
            {
                _logger.LogInformation(exc, "[S{0}, L{1}]: Exception on BeginAuthenticateAsClient.", RemoteEndPoint, LocalEndPoint);
                CloseInternal(SocketError.SocketError, exc.Message);
            }
        }

        public bool ValidateServerCertificate(
              object sender,
              X509Certificate certificate,
              X509Chain chain,
              SslPolicyErrors sslPolicyErrors)
        {
            if (sslPolicyErrors == SslPolicyErrors.None)
                return true;

            _logger.LogError("Certificate error: {0}", sslPolicyErrors);

            // Do not allow this client to communicate with unauthenticated servers.
            return false;
        }

        private void LoggingSslStreamInfo(SslStream stream)
        {
            var sb = new StringBuilder();
            sb.AppendFormat("[S{0}, L{1}]:\n", RemoteEndPoint, LocalEndPoint);
            sb.AppendFormat("Cipher: {0} strength {1}\n", stream.CipherAlgorithm, stream.CipherStrength);
            sb.AppendFormat("Hash: {0} strength {1}\n", stream.HashAlgorithm, stream.HashStrength);
            sb.AppendFormat("Key exchange: {0} strength {1}\n", stream.KeyExchangeAlgorithm, stream.KeyExchangeStrength);
            sb.AppendFormat("Protocol: {0}\n", stream.SslProtocol);
            sb.AppendFormat("Is authenticated: {0} as server? {1}\n", stream.IsAuthenticated, stream.IsServer);
            sb.AppendFormat("IsSigned: {0}\n", stream.IsSigned);
            sb.AppendFormat("Is Encrypted: {0}\n", stream.IsEncrypted);
            sb.AppendFormat("Can read: {0}, write {1}\n", stream.CanRead, stream.CanWrite);
            sb.AppendFormat("Can timeout: {0}\n", stream.CanTimeout);
            sb.AppendFormat("Certificate revocation list checked: {0}\n", stream.CheckCertRevocationStatus);

            X509Certificate2 localCert = stream.LocalCertificate as X509Certificate2;
            if (localCert != null)
                sb.AppendFormat("Local certificate was issued to {0} and is valid from {1} until {2}.\n",
                                localCert.Subject, localCert.NotBefore, localCert.NotAfter);
            else
                sb.AppendFormat("Local certificate is null.\n");

            // Display the properties of the client's certificate.
            X509Certificate2 remoteCert = stream.RemoteCertificate as X509Certificate2;
            if (remoteCert != null)
                sb.AppendFormat("Remote certificate was issued to {0} and is valid from {1} until {2}.\n",
                                remoteCert.Subject, remoteCert.NotBefore, remoteCert.NotAfter);
            else
                sb.AppendFormat("Remote certificate is null.\n");

            _logger.LogInformation(sb.ToString());
        }

        internal int SendTimeout
        {
            set
            {
                if (_client != null)
                    _client.SendTimeout = value;
            }
        }
        internal Stream getReadBufferedStream()
        {
            if (_sslStream != null)
                return _sslStream;

            return _sslStream;
        }

        internal Task ConnectAsync()
        {
            throw new NotImplementedException();
        }

        internal Stream getWriteBufferedStream(int size)
        {
            return new BufferedStream(_sslStream, size);
        }

        internal bool Connected
        {
            get
            {
                if (_client == null)
                    return false;

                return _client.Connected;
            }
        }

        public Guid ConnectionId => throw new NotImplementedException();

        public IPEndPoint RemoteEndPoint => throw new NotImplementedException();

        public IPEndPoint LocalEndPoint => throw new NotImplementedException();

        public int SendQueueSize => throw new NotImplementedException();

        public bool IsClosed => throw new NotImplementedException();

        public void Close(string reason)
        {
            CloseInternal(SocketError.Success, reason ?? "Normal socket close."); // normal socket closing
        }

        private void CloseInternal(SocketError socketError, string reason)
        {
            //TODO:需要大力整合 完善通知和日志记录
            //if (Interlocked.CompareExchange(ref _isClosed, 1, 0) != 0)
            //    return;

            ////NotifyClosed();

            //_logger.LogInformation("ClientAPI {0} closed [{1:HH:mm:ss.fff}: S{2}, L{3}, {4:B}]:", GetType().Name, DateTime.UtcNow, RemoteEndPoint, LocalEndPoint, _connectionId);
            //_logger.LogInformation("Received bytes: {0}, Sent bytes: {1}", TotalBytesReceived, TotalBytesSent);
            //_logger.LogInformation("Send calls: {0}, callbacks: {1}", SendCalls, SendCallbacks);
            //_logger.LogInformation("Receive calls: {0}, callbacks: {1}", ReceiveCalls, ReceiveCallbacks);
            //_logger.LogInformation("Close reason: [{0}] {1}", socketError, reason);

            if (_sslStream != null)
                _sslStream.Dispose();

            if (_client != null)
                _client.Close();

            //if (_onConnectionClosed != null)
            //    _onConnectionClosed(this, socketError);
        }

        public void Dispose()
        {
            if (_sslStream != null)
                _sslStream.Dispose();

            if (_client != null)
                _client.Close();
        }

        public void ReceiveAsync(Action<ITcpConnection, IEnumerable<ArraySegment<byte>>> callback)
        {
            throw new NotImplementedException();
        }

        public Task EnqueueSend(IEnumerable<ArraySegment<byte>> data)
        {
            throw new NotImplementedException();
        }
    }
}
