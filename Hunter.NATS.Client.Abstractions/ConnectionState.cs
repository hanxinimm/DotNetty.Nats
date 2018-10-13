using System;
using System.Collections.Generic;
using System.Text;

namespace Hunter.NATS.Client
{
    public enum ConnectionState
    {
        /// <summary>
        /// The <see cref="INATSConnection"/> is disconnected.
        /// </summary>
        Disconnected = 0,
        
        /// <summary>
        /// The <see cref="INATSConnection"/> is connected to a NATS Server.
        /// </summary>
        Connected,

        /// <summary>
        /// The <see cref="INATSConnection"/> has been closed.
        /// </summary>
        Closed,

        /// <summary>
        /// The <see cref="INATSConnection"/> is currently reconnecting
        /// to a NATS Server.
        /// </summary>
        Reconnecting,

        /// <summary>
        /// The <see cref="INATSConnection"/> is currently connecting
        /// to a NATS Server.
        /// </summary>
        Connecting
    }
}
