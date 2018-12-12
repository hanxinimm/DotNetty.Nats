using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace DotNetty.Codecs.NATS.Packets
{
    public class PermissionsViolationForSubscriptionErrorPacket : ErrorPacket
    {
        public PermissionsViolationForSubscriptionErrorPacket(string subject)
        {
            Subject = subject;
        }

        // <summary>
        /// 主题
        /// </summary>
        public string Subject { get; }
    }
}
