using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace DotNetty.Codecs.NATS.Packets
{
    /// <summary>
    /// Client attempted to publish a message with a payload size that exceeds the max_payload size configured on the server. 
    /// This value is supplied to the client upon connection in the initial INFO message. 
    /// The client is expected to do proper accounting of byte size to be sent to the server in order to handle this error synchronously.
    /// </summary>
    public class MaximumPayloadViolationErrorPacket : DeadEndErrorPacket
    {
        
    }
}
