using System;
using System.Collections.Generic;
using System.Text;

namespace Hunter.NATS.Client
{
    /// <summary>
    /// The exception that is thrown when a message payload exceeds what
    /// the maximum configured.
    /// </summary>
    public class NATSMaxPayloadException : NATSException
    {
        internal NATSMaxPayloadException() : base("Maximum payload size has been exceeded") { }
        internal NATSMaxPayloadException(string err) : base(err) { }
    }
}
