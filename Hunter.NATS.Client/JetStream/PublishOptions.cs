using System;
using System.Collections.Generic;
using System.Text;

namespace Hunter.NATS.Client.JetStream
{
    public class PublishOptions
    {
        /// <summary>
        /// 
        /// </summary>
        public TimeSpan TTL { get; set; }

        /// <summary>
        /// 消息编号
        /// </summary>
        public string MessageId { get; set; }
        /// <summary>
        /// 预期的消息编号
        /// </summary>
        /// <remarks>
        /// Expected last msgId
        /// </remarks>
        public string ExpectedMessageId { get; set; }
        /// <summary>
        /// 
        /// </summary>
        /// <remarks>
        /// Expected stream name
        /// </remarks>
        public string ExpectedStreamName { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <remarks>
        /// Expected last sequence
        /// </remarks>
        public long? ExpectedSequence { get; set; }
    }
}
