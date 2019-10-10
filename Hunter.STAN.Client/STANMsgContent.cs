using System;
using System.Collections.Generic;
using System.Text;

namespace Hunter.STAN.Client
{
    public class STANMsgContent
    {
        public ulong Sequence { get; set; }

        public string Subject { get; set; }

        public string Reply { get; set; }

        public byte[] Data { get; set; } = Array.Empty<byte>();

        public long Timestamp { get; set; }

        public bool Redelivered { get; set; }

        public uint CRC32 { get; set; }
    }
}
