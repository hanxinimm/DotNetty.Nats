using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace DotNetty.Codecs.STAN.Packets
{
    public class PermissionsViolationForSubscriptionErrorPacket : ErrorPacket
    {
        public PermissionsViolationForSubscriptionErrorPacket(string subject)
        {
            Subject = subject;
        }

        /// <summary>
        /// 错误的消息主题
        /// </summary>
        [DataMember]
        public string Subject { get; }
    }
}
