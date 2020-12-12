using System;
using System.Collections.Generic;
using System.Text;

namespace Hunter.NATS.Client
{
    public enum NATSConnectionState
    {
        /// <summary>
        /// The <see cref="IConnection"/> is disconnected.
        /// </summary>
        Disconnected = 0,
        
        /// <summary>
        /// The <see cref="IConnection"/> is connected to a NATS Server.
        /// </summary>
        Connected,

        /// <summary>
        /// The <see cref="IConnection"/> has been closed.
        /// </summary>
        Closed,

        /// <summary>
        /// The <see cref="IConnection"/> is currently reconnecting
        /// to a NATS Server.
        /// </summary>
        Reconnecting,

        /// <summary>
        /// The <see cref="IConnection"/> is currently connecting
        /// to a NATS Server.
        /// </summary>
        Connecting,

        /// <summary>
        /// The <see cref="IConnection"/> is currently disposeting
        /// to a NATS Server.
        /// </summary>
        Dispose,

        /// <summary>
        /// The <see cref="IConnection"/> is currently draining subscriptions.
        /// </summary>
        DrainingSubs,

        /// <summary>
        /// The <see cref="IConnection"/> is currently connecting draining
        /// publishers.
        /// </summary>
        DrainingPubs
    }
}
