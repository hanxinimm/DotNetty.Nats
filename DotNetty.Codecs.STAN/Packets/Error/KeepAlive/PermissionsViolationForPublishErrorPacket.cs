﻿using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace DotNetty.Codecs.STAN.Packets
{
    public class PermissionsViolationForPublishErrorPacket : ErrorPacket
    {
        public PermissionsViolationForPublishErrorPacket(string subject)
        {
            Subject = subject;
        }

        // <summary>
        /// 主题
        /// </summary>
        public string Subject { get; }
    }
}
