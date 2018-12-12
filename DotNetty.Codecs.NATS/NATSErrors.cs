using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace DotNetty.Codecs.NATS
{
    public class NATSErrors
    {
        #region DeadEnd

        public const string UnknownProtocolOperation = "Unknown Protocol Operation";
        public const string AttemptedToConnectToRoutePort = "Attempted To Connect To Route Port";
        public const string AuthorizationViolation = "Authorization Violation";
        public const string AuthorizationTimeout = "Authorization Timeout";
        public const string InvalidClientProtocol = "Invalid Client Protocol";
        public const string MaximumControlLineExceeded = "Maximum Control Line Exceeded";
        public const string ParserError = "Parser Error";
        public const string SecureConnection_TLSRequired = "Secure Connection - TLS Required";
        public const string StaleConnection = "Stale Connection";
        public const string MaximumConnectionsExceeded = "Maximum Connections Exceeded";
        public const string SlowConsumer = "Slow Consumer";
        public const string MaximumPayloadViolation = "Maximum Payload Violation";

        #endregion

        #region KeepAlive

        public const string InvalidSubject = "Invalid Subject";
        public static Regex PermissionsViolationForSubscription = new Regex(@"^Permissions Violation for Subscription to ([\\w\\d]+(.[\\w\\d]+)*)", RegexOptions.Compiled);
        public static Regex PermissionsViolationForPublish = new Regex(@"^Permissions Violation for Publish to ([\\w\\d]+(.[\\w\\d]+)*)$", RegexOptions.Compiled);

        #endregion;
    }
}
