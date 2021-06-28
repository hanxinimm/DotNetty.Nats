using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace DotNetty.Codecs.NATSJetStream.Protocol
{
    public class IterableRequest
    {
        public IterableRequest() { }

        public IterableRequest(int offset)
        {
            Offset = offset;
        }


        /// <summary>
        /// 偏移量
        /// </summary>
        [JsonProperty("offset")]
        public int Offset { get; set; }
    }
}
