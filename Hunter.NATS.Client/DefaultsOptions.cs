using System;
using System.Collections.Generic;
using System.Text;

namespace Hunter.NATS.Client
{
    /// <summary>
    /// This class contains default values for fields used throughout NATS.
    /// </summary>
    public static class DefaultsOptions
    {
        /// <summary>
        /// Client version
        /// </summary>
        public const string Version = "0.0.1";

        /// <summary>
        /// The default NATS connect url ("nats://localhost:4222")
        /// </summary>
        public const string Url = "nats://localhost:4222";

        /// <summary>
        /// The default NATS connect port. (4222)
        /// </summary>
        public const int Port = 4222;

        /// <summary>
        /// Default number of times to attempt a reconnect. (60)
        /// </summary>
        public const int MaxReconnect = 60;

        /// <summary>
        /// Default ReconnectWait time (2 seconds)
        /// </summary>
        public const int ReconnectWait = 2000; // 2 seconds.

        /// <summary>
        /// Default timeout  (2 seconds).
        /// </summary>
        public const int Timeout = 2000; // 2 seconds.

        /// <summary>
        ///  Default ping interval (2 minutes);
        /// </summary>
        public const int PingInterval = 120000;// 2 minutes.

        /// <summary>
        /// Default MaxPingOut value (2);
        /// </summary>
        public const int MaxPingOut = 2;

        /// <summary>
        /// Default MaxChanLen (65536)
        /// </summary>
        public const int MaxChanLen = 65536;

        /// <summary>
        /// Default Request Channel Length
        /// </summary>
        public const int RequestChanLen = 4;

        /// <summary>
        /// Language string of this client, ".NET"
        /// </summary>
        public const string LangString = ".NET";

        /// <summary>
        /// Default subscriber pending messages limit.
        /// </summary>
        public const long SubPendingMsgsLimit = 65536;

        /// <summary>
        /// Default subscriber pending bytes limit.
        /// </summary>
        public const long SubPendingBytesLimit = 65536 * 1024;

        /*
         * Namespace level defaults
         */

        // Scratch storage for assembling protocol headers
        internal const int ScratchSize = 512;

        // The size of the bufio writer on top of the socket.
        internal const int DefaultBufSize = 32768;

        // The read size from the network stream.
        internal const int DefaultReadLength = 20480;

        // The size of the bufio while we are reconnecting
        internal const int DefaultPendingSize = 1024 * 1024;

        // Default server pool size
        internal const int ServerPoolSize = 4;
    }
}
