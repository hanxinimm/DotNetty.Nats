using System;
using System.Collections.Generic;
using System.Text;

namespace DotNetty.Codecs.Protocol
{
    public static class JetStreamConstants
    {
        internal const string JS_PREFIX = "$JS.";

        internal const string JSAPI_PREFIX = JS_PREFIX + "API.";

        // JSAPI_ACCOUNT_INFO is for obtaining general information about JetStream.
        internal const string JSAPI_ACCOUNT_INFO = "INFO";

        // JSAPI_CONSUMER_CREATE is used to create consumers.
        internal const string JSAPI_CONSUMER_CREATE = "CONSUMER.CREATE.%s";

        // JSAPI_DURABLE_CREATE is used to create durable consumers.
        internal const string JSAPI_DURABLE_CREATE = "CONSUMER.DURABLE.CREATE.%s.%s";

        // JSAPI_CONSUMER_INFO is used to create consumers.
        internal const string JSAPI_CONSUMER_INFO = "CONSUMER.INFO.%s.%s";

        // JSAPI_CONSUMER_MSG_NEXT is the prefix for the request next message(s) for a consumer in worker/pull mode.
        internal const string JSAPI_CONSUMER_MSG_NEXT = "CONSUMER.MSG.NEXT.%s.%s";

        // JSAPI_CONSUMER_DELETE is used to delete consumers.
        internal const string JSAPI_CONSUMER_DELETE = "CONSUMER.DELETE.%s.%s";

        // JSAPI_CONSUMER_NAMES is used to return a list of consumer names
        internal const string JSAPI_CONSUMER_NAMES = "CONSUMER.NAMES.%s";

        // JSAPI_CONSUMER_LIST is used to return all detailed consumer information
        internal const string JSAPI_CONSUMER_LIST = "CONSUMER.LIST.%s";

        // JSAPI_STREAMS can lookup a stream by subject.
        internal const string JSAPI_STREAMS = "STREAM.NAMES";

        // JSAPI_STREAM_CREATE is the endpoint to create new streams.
        internal const string JSAPI_STREAM_CREATE = "STREAM.CREATE.%s";

        // JSAPI_STREAM_INFO is the endpoint to get information on a stream.
        internal const string JSAPI_STREAM_INFO = "STREAM.INFO.%s";

        // JSAPI_STREAM_UPDATE is the endpoint to update existing streams.
        internal const string JSAPI_STREAM_UPDATE = "STREAM.UPDATE.%s";

        // JSAPI_STREAM_DELETE is the endpoint to delete streams.
        internal const string JSAPI_STREAM_DELETE = "STREAM.DELETE.%s";

        // JSAPI_STREAM_PURGE is the endpoint to purge streams.
        internal const string JSAPI_STREAM_PURGE = "STREAM.PURGE.%s";

        // JSAPI_STREAM_NAMES is the endpoint that will return a list of stream names
        internal const string JSAPI_STREAM_NAMES = "STREAM.NAMES";

        // JSAPI_STREAM_LIST is the endpoint that will return all detailed stream information
        internal const string JSAPI_STREAM_LIST = "STREAM.LIST";

        // JSAPI_MSG_GET is the endpoint to get a message.
        internal const string JSAPI_MSG_GET = "STREAM.MSG.GET.%s";

        // JSAPI_MSG_DELETE is the endpoint to remove a message.
        internal const string JSAPI_MSG_DELETE = "STREAM.MSG.DELETE.%s";

        internal const string MSG_ID_HDR = "Nats-Msg-Id";
        internal const string EXPECTED_STREAM_HDR = "Nats-Expected-Stream";
        internal const string EXPECTED_LAST_SEQ_HDR = "Nats-Expected-Last-Sequence";
        internal const string EXPECTED_LAST_MSG_ID_HDR = "Nats-Expected-Last-Msg-Id";

        internal const int MAX_PULL_SIZE = 256;
    }
}
