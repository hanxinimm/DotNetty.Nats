using System;
using System.Collections.Generic;
using System.Text;

namespace Hunter.NATS.Client
{
    /// <summary>
    /// The exception that is thrown when there is a connection error.
    /// </summary>
    public class NATSConnectionException : NATSException
    {
        internal NATSConnectionException(string err) : base(err) { }
        internal NATSConnectionException(string err, Exception innerEx) : base(err, innerEx) { }
    }
}
