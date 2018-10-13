using System;
using System.Collections.Generic;
using System.Text;

namespace Hunter.NATS.Client
{
    /// <summary>
    /// The exception that is thrown when a secure connection is required.
    /// </summary>
    public class NATSSecureConnRequiredException : NATSException
    {
        internal NATSSecureConnRequiredException() : base("A secure connection is required.") { }
        internal NATSSecureConnRequiredException(String s) : base(s) { }
    }
}
