using System;
using System.Collections.Generic;
using System.Text;

namespace Hunter.NATS.Client
{
    /// <summary>
    /// The exception that is thrown when a subscriber has exceeded the maximum
    /// number of messages that has been configured.
    /// </summary>
    public class NATSMaxMessagesException : NATSException
    {
        internal NATSMaxMessagesException() : base("Maximum number of messages have been exceeded.") { }
    }
}
