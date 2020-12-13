using System;
using System.Collections.Generic;
using System.Text;
using DotNetty.Transport.Channels;
using DotNetty.Transport.Bootstrapping;

namespace Hunter.STAN.Client
{
    public enum STANConnectionState
    {
        /// <summary>
        /// 未初始化
        /// The <see cref="Bootstrap"/> is uninitialized.
        /// </summary>
        Uninitialized = 0,

        /// <summary>
        /// The <see cref="IChannel"/> is disconnected.
        /// </summary>
        Disconnected = 0,
        
        /// <summary>
        /// The <see cref="IChannel"/> is connected to a NATS Server.
        /// </summary>
        Connected,

        /// <summary>
        /// The <see cref="IChannel"/> has been closed.
        /// </summary>
        Closed,

        /// <summary>
        /// The <see cref="IChannel"/> is currently reconnecting
        /// to a NATS Server.
        /// </summary>
        Reconnecting,

        /// <summary>
        /// The <see cref="IChannel"/> is currently connecting
        /// to a NATS Server.
        /// </summary>
        Connecting,

        /// <summary>
        /// The <see cref="IChannel"/> is currently draining subscriptions.
        /// </summary>
        DrainingSubs,

        /// <summary>
        /// The <see cref="IChannel"/> is currently connecting draining
        /// publishers.
        /// </summary>
        DrainingPubs
    }
}
