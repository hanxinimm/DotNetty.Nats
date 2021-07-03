using System;
using System.Collections.Generic;
using System.Text;

namespace DotNetty.Codecs.NATSJetStream.Protocol
{
    public class ProtocolSignatures
    {
        internal const string JS_PREFIX = "$JS.";

        internal const string JSAPI_PREFIX = JS_PREFIX + "API.";

        #region Stream

        // JSAPI_STREAM_CREATE is the endpoint to create new streams.
        internal const string JSAPI_STREAM_CREATE = JSAPI_PREFIX + "STREAM.CREATE";

        // JSAPI_STREAM_UPDATE is the endpoint to update existing streams.
        internal const string JSAPI_STREAM_UPDATE = JSAPI_PREFIX + "STREAM.UPDATE";

        // JSAPI_STREAM_NAMES is the endpoint that will return a list of stream names
        internal const string JSAPI_STREAM_NAMES = JSAPI_PREFIX + "STREAM.NAMES";

        // JSAPI_STREAM_LIST is the endpoint that will return all detailed stream information
        internal const string JSAPI_STREAM_LIST = JSAPI_PREFIX + "STREAM.LIST";

        // JSAPI_STREAM_INFO is the endpoint to get information on a stream.
        internal const string JSAPI_STREAM_INFO = JSAPI_PREFIX + "STREAM.INFO";

        // JSAPI_STREAM_DELETE is the endpoint to delete streams.
        internal const string JSAPI_STREAM_DELETE = JSAPI_PREFIX + "STREAM.DELETE";

        // JSAPI_STREAM_PURGE is the endpoint to purge streams.
        internal const string JSAPI_STREAM_PURGE = JSAPI_PREFIX + "STREAM.PURGE";

        // JSAPI_MSG_GET is the endpoint to get a message.
        internal const string JSAPI_MSG_GET = JSAPI_PREFIX + "STREAM.MSG.GET";

        // JSAPI_MSG_DELETE is the endpoint to remove a message.
        internal const string JSAPI_MSG_DELETE = JSAPI_PREFIX + "STREAM.MSG.DELETE";

        // JSAPI_STREAM_SNAPSHOT 
        internal const string JSAPI_STREAM_SNAPSHOT = JSAPI_PREFIX + "STREAM.SNAPSHOT";

        // JSAPI_STREAM_RESTORE 
        internal const string JSAPI_STREAM_RESTORE = JSAPI_PREFIX + "STREAM.RESTORE";

        // JSAPI_STREAM_PEER_REMOVE 
        internal const string JSAPI_STREAM_PEER_REMOVE = JSAPI_PREFIX + "STREAM.PEER.REMOVE";

        // JSAPI_STREAM_LEADER_STEPDOWN 
        internal const string JSAPI_STREAM_LEADER_STEPDOWN = JSAPI_PREFIX + "STREAM.LEADER.STEPDOWN";

        #endregion;

        #region Consumer

        // JSAPI_CONSUMER_CREATE is used to create consumers.
        internal const string JSAPI_CONSUMER_CREATE = JSAPI_PREFIX + "CONSUMER.CREATE";

        // JSAPI_DURABLE_CREATE is used to create durable consumers.
        internal const string JSAPI_DURABLE_CREATE = JSAPI_PREFIX + "CONSUMER.DURABLE.CREATE";

        // JSAPI_CONSUMER_INFO is used to create consumers.
        internal const string JSAPI_CONSUMER_INFO = JSAPI_PREFIX + "CONSUMER.INFO";

        // JSAPI_CONSUMER_MSG_NEXT is the prefix for the request next message(s) for a consumer in worker/pull mode.
        internal const string JSAPI_CONSUMER_MSG_NEXT = JSAPI_PREFIX + "CONSUMER.MSG.NEXT";

        // JSAPI_CONSUMER_DELETE is used to delete consumers.
        internal const string JSAPI_CONSUMER_DELETE = JSAPI_PREFIX + "CONSUMER.DELETE";

        // JSAPI_CONSUMER_NAMES is used to return a list of consumer names
        internal const string JSAPI_CONSUMER_NAMES = JSAPI_PREFIX + "CONSUMER.NAMES";

        // JSAPI_CONSUMER_LIST is used to return all detailed consumer information
        internal const string JSAPI_CONSUMER_LIST = JSAPI_PREFIX + "CONSUMER.LIST";

        #endregion;
    }
}
