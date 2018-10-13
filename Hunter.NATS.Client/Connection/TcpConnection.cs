using System;
using System.Collections.Generic;
using System.IO;
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
    internal sealed class TcpConnection : IDisposable
    {
        private readonly TcpClient _client;
        public TcpConnection()
        {
            _client = new TcpClient();
        }
        /// A note on the use of streams.  .NET provides a BufferedStream
        /// that can sit on top of an IO stream, in this case the network
        /// stream. It increases performance by providing an additional
        /// buffer.
        /// 
        /// So, here's what we have for writing:
        ///     Client code
        ///          ->BufferedStream (bw)
        ///              ->NetworkStream/SslStream (srvStream)
        ///                  ->TCPClient (srvClient);
        ///                  
        ///  For reading:
        ///     Client code
        ///          ->NetworkStream/SslStream (srvStream)
        ///              ->TCPClient (srvClient);
        /// 
        object mu = new object();
        NetworkStream stream = null;
        SslStream sslStream = null;

        string hostName = null;

        internal async Task ConnectAsync(Srv s, int timeoutMillis)
        {
            //lock (mu)
            //{
                // If a connection was lost during a reconnect we 
                // we could have a defunct SSL stream remaining and 
                // need to clean up.
                if (sslStream != null)
                {
                    try
                    {
                        sslStream.Dispose();
                    }
                    catch (Exception) { }
                    sslStream = null;
                }

                //client = new TcpClient();
                var CancellationToken = new CancellationTokenSource(TimeSpan.FromSeconds(timeoutMillis));
                await Task.Run(()=>_client.ConnectAsync(s.url.Host, s.url.Port), CancellationToken.Token);
                
                if (CancellationToken.IsCancellationRequested)
                {
                    //_client = null;
                    throw new NATSConnectionException("timeout");
                }

                _client.NoDelay = false;

                _client.ReceiveBufferSize = DefaultsOptions.DefaultBufSize * 2;
                _client.SendBufferSize = DefaultsOptions.DefaultBufSize;

                stream = _client.GetStream();

                // save off the hostname
                hostName = s.url.Host;
            //}
        }

        private static bool remoteCertificateValidation(
              object sender,
              X509Certificate certificate,
              X509Chain chain,
              SslPolicyErrors sslPolicyErrors)
        {
            if (sslPolicyErrors == SslPolicyErrors.None)
                return true;

            return false;
        }

        internal void closeClient(TcpClient c)
        {
            if (c != null)
            {
#if NET45
                    c.Close();
#else
                c.Dispose();
#endif
            }
        }

        internal void makeTLS(NATSOptions options)
        {
            RemoteCertificateValidationCallback cb = null;

            if (stream == null)
                throw new NATSException("Internal error:  Cannot create SslStream from null stream.");

            cb = options.TLSRemoteCertificationValidationCallback;
            if (cb == null)
                cb = remoteCertificateValidation;

            sslStream = new SslStream(stream, false, cb, null,
                EncryptionPolicy.RequireEncryption);

            try
            {
                SslProtocols protocol = (SslProtocols)Enum.Parse(typeof(SslProtocols), "Tls12");
                sslStream.AuthenticateAsClientAsync(hostName, options.certificates, protocol, true).Wait();
            }
            catch (Exception ex)
            {
                closeClient(_client);
                throw new NATSConnectionException("TLS Authentication error", ex);
            }
        }

        internal int SendTimeout
        {
            set
            {
                if (_client != null)
                    _client.SendTimeout = value;
            }
        }

        internal bool isSetup()
        {
            return (_client != null);
        }

        internal void teardown()
        {
            TcpClient c;
            Stream s;

            lock (mu)
            {
                c = _client;
                s = getReadBufferedStream();

                //TODO:设置客户端为空
                //_client = null;
                stream = null;
                sslStream = null;
            }

            try
            {
                if (s != null)
                    s.Dispose();

                if (c != null)
                    closeClient(c);
            }
            catch (Exception) { }
        }

        internal Stream getReadBufferedStream()
        {
            if (sslStream != null)
                return sslStream;

            return stream;
        }

        internal Stream getWriteBufferedStream(int size)
        {
            BufferedStream bs = null;

            if (sslStream != null)
                bs = new BufferedStream(sslStream, size);
            else
                bs = new BufferedStream(stream, size);

            return bs;
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

        internal bool DataAvailable
        {
            get
            {
                if (stream == null)
                    return false;

                return stream.DataAvailable;
            }
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (sslStream != null)
                    sslStream.Dispose();
                if (stream != null)
                    stream.Dispose();
                if (_client != null)
                    closeClient(_client);

                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }
        #endregion
    }
}
