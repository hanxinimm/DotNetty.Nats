using System;
using System.Collections.Generic;
using System.Text;

namespace Hunter.NATS.Client
{
    //TODO:待弄清楚这个协议什么意思
    internal enum ClientProtcolVersion
    {
        // clientProtoZero is the original client protocol from 2009.
        // http://nats.io/documentation/internals/nats-protocol/
        ClientProtoZero = 0,

        // ClientProtoInfo signals a client can receive more then the original INFO block.
        // This can be used to update clients on other cluster members, etc.
        ClientProtoInfo
    }
}
