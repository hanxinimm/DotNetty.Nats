using System;

namespace Hunter.STAN.Client
{
    /// <summary>
    /// A general exception thrown by the NATS streaming server client.
    /// </summary>
    public class StanException : Exception
    {
        internal StanException() : base() { }
        internal StanException(string err) : base (err) {}
        internal StanException(string err, Exception innerEx) : base(err, innerEx) { }
    }


    /// <summary>
    /// An exception representing the case when an operation is performed on a subscription
    /// that is no longer valid.
    /// </summary>
    public class StanBadSubscriptionException : StanException
    {
        internal StanBadSubscriptionException() : base("Invalid subscription.") { }
        internal StanBadSubscriptionException(Exception e) : base("Invalid subscription.", e) { }
    }

    /// <summary>
    /// An exception representing the general case when an operation times out.
    /// </summary>
    public class StanTimeoutException : StanException
    {
        internal StanTimeoutException() : base("Operation timed out.") { }
        internal StanTimeoutException(Exception e) : base("Operation timed out.", e) { }
        internal StanTimeoutException(string msg) : base(msg) { }
    }

    /// <summary>
    /// An exception representing the case when a streaming connection attempt
    /// fails due to a mismatched cluster id or connectivity with a streaming
    /// server.
    /// </summary>
    /// <remarks>
    /// This is exception is thrown due to a mismatch between the cluster ID
    /// of the client and the streaming server or when there is connectivity
    /// with a core NATS server but not a streaming server.
    /// </remarks>
    public class StanConnectRequestTimeoutException : StanTimeoutException
    {
        internal StanConnectRequestTimeoutException() : base("Connection Request Timed out.") { }
        internal StanConnectRequestTimeoutException(string msg) : base(msg) { }
    }

    /// <summary>
    /// An exception representing the case when a connection cannot be established
    /// with the NATS streaming server or an operation is attempted while the underlying
    /// NATS connection is reconnecting.
    /// </summary>
    public class StanConnectionException : StanException
    {
        internal StanConnectionException() : base("Invalid connection.") { }
        internal StanConnectionException(Exception e) : base("Invalid connection.", e) { }
        internal StanConnectionException(string msg) : base(msg) { }

    }

    /// <summary>
    /// An exception representing the case when the application attempts 
    /// to manually acknowledge a message while the subscriber is configured
    /// to automatically acknowledge messages.
    /// </summary>
    public class StanManualAckException : StanException
    {
        internal StanManualAckException() : base("Cannot manually ack in auto-ack mode.") { }
        internal StanManualAckException(Exception e) : base("Cannot manually ack in auto-ack mode.", e) { }
    }

    /// <summary>
    /// An exception representing the case when the application attempts 
    /// to manually acknowledge a message while the subscriber is configured
    /// to automatically acknowledge messages.
    /// </summary>
    public class StanNoServerSupport : StanException
    {
        internal StanNoServerSupport() : base("Operation not supported by the server.") { }
        internal StanNoServerSupport(Exception e) : base("Operation not supported by the server.", e) { }
    }

    /// <summary>
    /// An exception indicating connectivity with the streaming server has 
    /// been lost due to exceeding the maximum number of outstanding pings.
    /// </summary>
    public class StanMaxPingsException : StanException
    {
        internal StanMaxPingsException() : base("Connection lost due to PING failure.") { }
        internal StanMaxPingsException(Exception e) : base(err: "Connection lost due to PING failure.", innerEx: e) { }
    }
}
