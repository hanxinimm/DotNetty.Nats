﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace DotNetty.Codecs.NATSJetStream.Protocol
{
    public class LeaderStepDownResponse : JetStreamResponse
    {
        [JsonProperty("success")]
        public bool Success { get; set; }
    }
}
