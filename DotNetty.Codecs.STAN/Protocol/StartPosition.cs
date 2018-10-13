using System;
using System.Collections.Generic;
using System.Text;
using pbr = global::Google.Protobuf.Reflection;

namespace DotNetty.Codecs.STAN.Protocol
{
    /// <summary>
    /// Enum for start position type.
    /// </summary>
    public enum StartPosition
    {
        [pbr::OriginalName("NewOnly")] NewOnly = 0,
        [pbr::OriginalName("LastReceived")] LastReceived = 1,
        [pbr::OriginalName("TimeDeltaStart")] TimeDeltaStart = 2,
        [pbr::OriginalName("SequenceStart")] SequenceStart = 3,
        [pbr::OriginalName("First")] First = 4,
    }
}
