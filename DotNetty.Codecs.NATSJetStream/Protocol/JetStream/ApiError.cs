using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace DotNetty.Codecs.NATSJetStream.Protocol
{
    /// <summary>
    /// 错误信息
    /// </summary>
    /// <see cref="http://nats-io.github.io/nats.c/status_8h.html#a36c934157b663b7b5fb5d6609c897c80"/>
    public class ApiError
    {
        /// <summary>
        /// 代码
        /// </summary>
        [JsonProperty("code")]
        public string Code { get; set; }

        /// <summary>
        /// 描述
        /// </summary>
        [JsonProperty("description")]
        public string Description { get; set; }

        public override string ToString()
        {
            return $@"ApiError {{
                        code='{ Code }'
                        , description={ Description }
                    }}";
        }
    }
}
