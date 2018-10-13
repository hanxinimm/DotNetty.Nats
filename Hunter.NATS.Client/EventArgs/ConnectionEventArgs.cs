using System;
using System.Collections.Generic;
using System.Text;

namespace Hunter.NATS.Client
{
    /// <summary>
    /// Provides the details when the state of a <see cref="Connection"/>
    /// changes.
    /// </summary>
    public class ConnectionEventArgs : EventArgs
    {
        internal ConnectionEventArgs(INATSConnection connection)
        {
            Connection = connection;
        }

        /// <summary>
        /// Gets the <see cref="Connection"/> associated with the event.
        /// </summary>
        public INATSConnection Connection { get; private set; }
    }
}
