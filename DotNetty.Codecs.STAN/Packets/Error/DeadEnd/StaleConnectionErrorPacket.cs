using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace DotNetty.Codecs.STAN.Packets
{
    /// <summary>
    /// The server hasn’t received a message from the client, including a PONG in too long.
    /// </summary>
    public class StaleConnectionErrorPacket : DeadEndErrorPacket
    {
        
    }
}
