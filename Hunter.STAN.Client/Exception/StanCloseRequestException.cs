using System;
using System.Collections.Generic;
using System.Text;

namespace Hunter.STAN.Client
{
    /// <summary>
    /// An exception representing the case when an error occurs closing a connection.
    /// </summary>
    public class StanCloseRequestException : StanException
    {
        internal StanCloseRequestException() : base("Close request timeout.") { }
        internal StanCloseRequestException(string msg) : base(msg) { }
        internal StanCloseRequestException(Exception e) : base("Close request timeout.", e) { }
    }
}
