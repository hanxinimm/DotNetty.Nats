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

using System.Threading.Tasks;

namespace Hunter.NATS.Client
{
    /// <summary>
    /// Provides factory methods to create connections to NATS Servers.
    /// </summary>
    public sealed class ConnectionFactory
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ConnectionFactory"/> class,
        /// providing factory methods to create connections to NATS Servers.
        /// </summary>
        public ConnectionFactory() { }

        /// <summary>
        /// Attempt to connect to the NATS server referenced by <paramref name="url"/>.
        /// </summary>
        /// <remarks>
        /// <para><paramref name="url"/> can contain username/password semantics.
        /// Comma seperated arrays are also supported, e.g. <c>&quot;urlA, urlB&quot;</c>.</para>
        /// </remarks>
        /// <param name="url">A string containing the URL (or URLs) to the NATS Server. See the Remarks
        /// section for more information.</param>
        /// <returns>An <see cref="INATSConnection"/> object connected to the NATS server.</returns>
        /// <exception cref="NATSNoServersException">No connection to a NATS Server could be established.</exception>
        /// <exception cref="NATSConnectionException"><para>A timeout occurred connecting to a NATS Server.</para>
        /// <para>-or-</para>
        /// <para>An exception was encountered while connecting to a NATS Server. See <see cref="System.Exception.InnerException"/> for more
        /// details.</para></exception>
        public Task<INATSConnection> CreateConnection(string url)
        {
            NATSOptions opts = new NATSOptions();
            opts.processUrlString(url);
            return CreateConnectionAsync(opts);
        }

        /// <summary>
        /// Retrieves the default set of client options.
        /// </summary>
        /// <returns>The default <see cref="NATSOptions"/> object for the NATS client.</returns>
        public static NATSOptions GetDefaultOptions()
        {
            return new NATSOptions();
        }

        /// <summary>
        /// Attempt to connect to the NATS server using TLS referenced by <paramref name="url"/>.
        /// </summary>
        /// <remarks>
        /// <para><paramref name="url"/> can contain username/password semantics.
        /// Comma seperated arrays are also supported, e.g. urlA, urlB.</para>
        /// </remarks>
        /// <param name="url">A string containing the URL (or URLs) to the NATS Server. See the Remarks
        /// section for more information.</param>
        /// <returns>An <see cref="INATSConnection"/> object connected to the NATS server.</returns>
        /// <exception cref="NATSNoServersException">No connection to a NATS Server could be established.</exception>
        /// <exception cref="NATSConnectionException"><para>A timeout occurred connecting to a NATS Server.</para>
        /// <para>-or-</para>
        /// <para>An exception was encountered while connecting to a NATS Server. See <see cref="System.Exception.InnerException"/> for more
        /// details.</para></exception>
        public Task<INATSConnection> CreateSecureConnection(string url)
        {
            NATSOptions opts = new NATSOptions();
            opts.processUrlString(url);
            opts.Secure = true;
            return CreateConnectionAsync(opts);
        }

        /// <summary>
        /// Create a connection to the NATs server using the default options.
        /// </summary>
        /// <returns>An <see cref="INATSConnection"/> object connected to the NATS server.</returns>
        /// <exception cref="NATSNoServersException">No connection to a NATS Server could be established.</exception>
        /// <exception cref="NATSConnectionException"><para>A timeout occurred connecting to a NATS Server.</para>
        /// <para>-or-</para>
        /// <para>An exception was encountered while connecting to a NATS Server. See <see cref="System.Exception.InnerException"/> for more
        /// details.</para></exception>
        /// <seealso cref="GetDefaultOptions"/>
        public Task<INATSConnection> CreateConnection()
        {
            return CreateConnectionAsync(GetDefaultOptions());
        }

        /// <summary>
        /// Create a connection to a NATS Server defined by the given options.
        /// </summary>
        /// <param name="opts">The NATS client options to use for this connection.</param>
        /// <returns>An <see cref="INATSConnection"/> object connected to the NATS server.</returns>
        /// <exception cref="NATSNoServersException">No connection to a NATS Server could be established.</exception>
        /// <exception cref="NATSConnectionException"><para>A timeout occurred connecting to a NATS Server.</para>
        /// <para>-or-</para>
        /// <para>An exception was encountered while connecting to a NATS Server. See <see cref="System.Exception.InnerException"/> for more
        /// details.</para></exception>
        public async Task<INATSConnection> CreateConnectionAsync(NATSOptions opts)
        {
            var tcpconn = new SslTcpConnection(opts, null, null, null, null);
            NATSConnection nc = new NATSConnection(opts, new SslTcpConnection(opts, null, null, null, null));
            try
            {
                await tcpconn.ConnectAsync();
            }
            catch (System.Exception)
            {
                nc.Dispose();
                throw;
            }
            return nc;
        }

        /// <summary>
        /// Attempt to connect to the NATS server, with an encoded connection, using the default options.
        /// </summary>
        /// <returns>An <see cref="ISerializationConnectionAsync"/> object connected to the NATS server.</returns>
        /// <seealso cref="GetDefaultOptions"/>
        /// <exception cref="NATSNoServersException">No connection to a NATS Server could be established.</exception>
        /// <exception cref="NATSConnectionException"><para>A timeout occurred connecting to a NATS Server.</para>
        /// <para>-or-</para>
        /// <para>An exception was encountered while connecting to a NATS Server. See <see cref="System.Exception.InnerException"/> for more
        /// details.</para></exception>
        public ISerializationConnectionAsync CreateEncodedConnection()
        {
            return CreateEncodedConnection(GetDefaultOptions());
        }

        /// <summary>
        /// Attempt to connect to the NATS server, with an encoded connection, referenced by <paramref name="url"/>.
        /// </summary>
        /// <remarks>
        /// <para><paramref name="url"/> can contain username/password semantics.
        /// Comma seperated arrays are also supported, e.g. urlA, urlB.</para>
        /// </remarks>
        /// <param name="url">A string containing the URL (or URLs) to the NATS Server. See the Remarks
        /// section for more information.</param>
        /// <returns>An <see cref="ISerializationConnectionAsync"/> object connected to the NATS server.</returns>
        /// <exception cref="NATSNoServersException">No connection to a NATS Server could be established.</exception>
        /// <exception cref="NATSConnectionException"><para>A timeout occurred connecting to a NATS Server.</para>
        /// <para>-or-</para>
        /// <para>An exception was encountered while connecting to a NATS Server. See <see cref="System.Exception.InnerException"/> for more
        /// details.</para></exception>
        public ISerializationConnectionAsync CreateEncodedConnection(string url)
        {
            NATSOptions opts = new NATSOptions();
            opts.processUrlString(url);
            return CreateEncodedConnection(opts);
        }

        /// <summary>
        /// Attempt to connect to the NATS server, with an encoded connection, using the given options.
        /// </summary>
        /// <param name="opts">The NATS client options to use for this connection.</param>
        /// <returns>An <see cref="ISerializationConnectionAsync"/> object connected to the NATS server.</returns>
        /// <exception cref="NATSNoServersException">No connection to a NATS Server could be established.</exception>
        /// <exception cref="NATSConnectionException"><para>A timeout occurred connecting to a NATS Server.</para>
        /// <para>-or-</para>
        /// <para>An exception was encountered while connecting to a NATS Server. See <see cref="System.Exception.InnerException"/> for more
        /// details.</para></exception>
        public ISerializationConnectionAsync CreateEncodedConnection(NATSOptions opts)
        {
            //TODO:创建解析链接
            //SerializationConnection nc = new SerializationConnection(opts);
            //try
            //{
            //    nc.connect();
            //}
            //catch (System.Exception)
            //{
            //    nc.Dispose();
            //    throw;
            //}
            //return nc;
            return null;
        }
    }
}
