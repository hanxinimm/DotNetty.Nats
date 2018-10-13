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

namespace Hunter.NATS.Client
{
    /// <summary>
    /// Represents a connection to a NATS Server which uses a client specified
    /// encoding scheme.
    /// </summary>
    public interface ISerializationConnection : IDisposable
    {
        /// <summary>
        /// Publishes the serialized value of <paramref name="obj"/> to the given <paramref name="subject"/>.
        /// </summary>
        /// <param name="subject">The subject to publish <paramref name="obj"/> to over
        /// the current connection.</param>
        /// <param name="obj">The <see cref="Object"/> to serialize and publish to the connected NATS server.</param>
        /// <seealso cref="INATSConnection.Publish(string, byte[])"/>
        void Publish(string subject, object obj);

        /// <summary>
        /// Publishes the serialized value of <paramref name="obj"/> to the given <paramref name="subject"/>.
        /// </summary>
        /// <param name="subject">The subject to publish <paramref name="obj"/> to over
        /// the current connection.</param>
        /// <param name="reply">An optional reply subject.</param>
        /// <param name="obj">The <see cref="Object"/> to serialize and publish to the connected NATS server.</param>
        /// <seealso cref="INATSConnection.Publish(string, byte[])"/>
        void Publish(string subject, string reply, object obj);

        /// <summary>
        /// Sends a request payload and returns the deserialized response, or throws
        /// <see cref="NATSTimeoutException"/> if the <paramref name="timeout"/> expires.
        /// </summary>
        /// <remarks>
        /// <see cref="Request(string, object, int)"/> will create an unique inbox for this request, sharing a single
        /// subscription for all replies to this <see cref="ISerializationConnectionAsync"/> instance. However, if 
        /// <see cref="NATSOptions.UseOldRequestStyle"/> is set, each request will have its own underlying subscription. 
        /// The old behavior is not recommended as it may cause unnecessary overhead on connected NATS servers.
        /// </remarks>
        /// <param name="subject">The subject to publish <paramref name="obj"/> to over
        /// the current connection.</param>
        /// <param name="obj">The <see cref="Object"/> to serialize and publish to the connected NATS server.</param>
        /// <param name="timeout">The number of milliseconds to wait.</param>
        /// <returns>A <see cref="Object"/> with the deserialized response from the NATS server.</returns>
        /// <seealso cref="INATSConnection.Request(string, byte[])"/>
        object Request(string subject, object obj, int timeout);

        /// <summary>
        /// Sends a request payload and returns the deserialized response.
        /// </summary>
        /// <remarks>
        /// <see cref="Request(string, object)"/> will create an unique inbox for this request, sharing a single
        /// subscription for all replies to this <see cref="ISerializationConnectionAsync"/> instance. However, if 
        /// <see cref="NATSOptions.UseOldRequestStyle"/> is set, each request will have its own underlying subscription. 
        /// The old behavior is not recommended as it may cause unnecessary overhead on connected NATS servers.
        /// </remarks>
        /// <param name="subject">The subject to publish <paramref name="obj"/> to over
        /// the current connection.</param>
        /// <param name="obj">The <see cref="Object"/> to serialize and publish to the connected NATS server.</param>
        /// <returns>A <see cref="Object"/> with the deserialized response from the NATS server.</returns>
        /// <seealso cref="INATSConnection.Request(string, byte[])"/>
        object Request(string subject, object obj);

    }
}