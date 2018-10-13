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

/*! \mainpage %NATS .NET client.
 *
 * \section intro_sec Introduction
 *
 * The %NATS .NET Client is part of %NATS an open-source, cloud-native 
 * messaging system.
 * This client, written in C#, follows the go client closely, but
 * diverges in places to follow the common design semantics of a .NET API.
 *
 * \section install_sec Installation
 * 
 * Instructions to build and install the %NATS .NET C# client can be
 * found at the [NATS .NET C# GitHub page](https://github.com/nats-io/csharp-nats)
 *
 * \section other_doc_section Other Documentation
 * 
 * This documentation focuses on the %NATS .NET C# Client API; for additional
 * information, refer to the following:
 * 
 * - [General Documentation for nats.io](http://nats.io/documentation) 
 * - [NATS .NET C# Client found on GitHub](https://github.com/nats-io/csharp-nats) 
 * - [The NATS server (gnatsd) found on GitHub](https://github.com/nats-io/gnatsd)
 */

// Notes on the NATS .NET client.
// 
// While public and protected methods 
// and properties adhere to the .NET coding guidlines, 
// internal/private members and methods mirror the go client for
// maintenance purposes.  Public method and properties are
// documented with standard .NET doc.
// 
//     - Public/Protected members and methods are in PascalCase
//     - Public/Protected members and methods are documented in 
//       standard .NET documentation.
//     - Private/Internal members and methods are in camelCase.
//     - There are no "callbacks" - delegates only.
//     - Public members are accessed through a property.
//     - When possible, internal members are accessed directly.
//     - Internal Variable Names mirror those of the go client.
//     - Minimal/no reliance on third party packages.
//
//     Coding guidelines are based on:
//     http://blogs.msdn.com/b/brada/archive/2005/01/26/361363.aspx
//     although method location mirrors the go client to faciliate
//     maintenance.
//     
namespace Hunter.NATS.Client
{
    /// <summary>
    /// Provides the message received by an <see cref="IAsyncSubscription"/>.
    /// </summary>
    public class MessageHandlerEventArgs : EventArgs
    {
        public MessageHandlerEventArgs(Message message)
        {
            Message = message;
        }

        /// <summary>
        /// Retrieves the message.
        /// </summary>
        public Message Message { get; private set; }
    }
}
