using System;
using System.Collections.Generic;
using System.Text;

namespace Hunter.STAN.Client
{
    /// <summary>
    /// An exception representing the case when an error occurs closing a connection.
    /// </summary>
    public class StanConnectionClosedException : StanException
    {
        internal StanConnectionClosedException() : base("Connection closed.") { }
        internal StanConnectionClosedException(Exception e) : base("Connection closed.", e) { }
    }
}
