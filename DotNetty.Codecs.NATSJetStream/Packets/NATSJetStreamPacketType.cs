using System;
using System.Collections.Generic;
using System.Text;

namespace DotNetty.Codecs.NATSJetStream.Packets
{
    public enum NATSJetStreamPacketType
    {
        /// <summary>
        /// Server	Sent to client after initial TCP/IP connection
        /// </summary>
        INFO,
        /// <summary>
        /// Client	Sent to server to specify connection information
        /// </summary>
        Create,
        CreateResponse,
    }
}
