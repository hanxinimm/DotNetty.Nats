using System;
using System.Collections.Generic;
using System.Text;

namespace Hunter.NATS.Client
{
    /// <summary>
    /// The exception that is thrown when there is a NATS error condition.  All
    /// NATS exception inherit from this class.
    /// </summary>
    public class NATSException : Exception
    {
        internal NATSException() : base() { }
        internal NATSException(string err) : base(err) { }
        internal NATSException(string err, Exception innerEx) : base(err, innerEx) { }
    }
}
