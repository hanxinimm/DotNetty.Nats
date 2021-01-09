using System;
using System.Collections.Generic;
using System.Text;

namespace Hunter.STAN.Client
{

    /// <summary>
    /// An exception representing the case when a publish times out waiting for an 
    /// acknowledgement.
    /// </summary>
    public class StanPublishAckTimeoutException : StanException
    {
        internal StanPublishAckTimeoutException() : base("Publish acknowledgement timeout.") { }
        internal StanPublishAckTimeoutException(Exception e) : base("Publish acknowledgement timeout.", e) { }
    }
}
