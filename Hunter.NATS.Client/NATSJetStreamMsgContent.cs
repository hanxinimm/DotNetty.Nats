using DotNetty.Codecs.NATSJetStream.Protocol;
using System;
using System.Collections.Generic;
using System.Text;

namespace Hunter.NATS.Client
{
    public class NATSJetStreamMsgContent : NATSMsgContent
    {
        /// <summary>
        /// 元数据
        /// </summary>
        public MessageMetadata Metadata { get; set; }
    }
}
