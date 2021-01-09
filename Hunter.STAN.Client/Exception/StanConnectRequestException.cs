using System;
using System.Collections.Generic;
using System.Text;

namespace Hunter.STAN.Client
{
    /// <summary>
    /// An exception representing the case when an error occurs in the connection request.
    /// </summary>
    public class StanConnectRequestException : StanException
    {
        internal StanConnectRequestException() : base("Connection request timeout.") { }
        internal StanConnectRequestException(string msg) : base(msg) { }
        internal StanConnectRequestException(Exception e) : base("Connection request timeout.", e) { }
    }
}
