using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Hunter.NATS.Client.JetStream
{
    public class ExternalStream
    {
        [JsonProperty("api")]
        public string ApiPrefix { get; set; }
        [JsonProperty("deliver")]
        public string DeliverPrefix { get; set; }

        public override string ToString()
        {
            return $"External {{ api='{ ApiPrefix }',deliver='{DeliverPrefix}'}}";
        }
    }
}
