using System;
using System.Collections.Generic;
using System.Text;

namespace DotNetty.Codecs.NATSJetStream.Protocol
{
    public class NATSJetStreamSignatures
    {
        public const string COLON = ":";

        public const string JS_PREFIX = "$JS.";

        public const string JSAPI_PREFIX = JS_PREFIX + "API.";

        public const string JSAuditAdvisory = "$JS.EVENT.ADVISORY.API";

        public const string JSMetricPrefix = "$JS.EVENT.METRIC";

        public const string JSAdvisoryPrefix = "$JS.EVENT.ADVISORY";

        public const string JSApiAccountInfo = "$JS.API.INFO";

        #region Stream

        // JSAPI_STREAM_CREATE is the endpoint to create new streams.
        public const string JSAPI_STREAM_CREATE = JSAPI_PREFIX + "STREAM.CREATE";

        // JSAPI_STREAM_UPDATE is the endpoint to update existing streams.
        public const string JSAPI_STREAM_UPDATE = JSAPI_PREFIX + "STREAM.UPDATE";

        // JSAPI_STREAM_NAMES is the endpoint that will return a list of stream names
        public const string JSAPI_STREAM_NAMES = JSAPI_PREFIX + "STREAM.NAMES";

        // JSAPI_STREAM_LIST is the endpoint that will return all detailed stream information
        public const string JSAPI_STREAM_LIST = JSAPI_PREFIX + "STREAM.LIST";

        // JSAPI_STREAM_INFO is the endpoint to get information on a stream.
        public const string JSAPI_STREAM_INFO = JSAPI_PREFIX + "STREAM.INFO";

        // JSAPI_STREAM_DELETE is the endpoint to delete streams.
        public const string JSAPI_STREAM_DELETE = JSAPI_PREFIX + "STREAM.DELETE";

        // JSAPI_STREAM_PURGE is the endpoint to purge streams.
        public const string JSAPI_STREAM_PURGE = JSAPI_PREFIX + "STREAM.PURGE";

        // JSAPI_MSG_GET is the endpoint to get a message.
        public const string JSAPI_MSG_GET = JSAPI_PREFIX + "STREAM.MSG.GET";

        // JSAPI_MSG_DELETE is the endpoint to remove a message.
        public const string JSAPI_MSG_DELETE = JSAPI_PREFIX + "STREAM.MSG.DELETE";

        // JSAPI_STREAM_SNAPSHOT 
        public const string JSAPI_STREAM_SNAPSHOT = JSAPI_PREFIX + "STREAM.SNAPSHOT";

        // JSAPI_STREAM_RESTORE 
        public const string JSAPI_STREAM_RESTORE = JSAPI_PREFIX + "STREAM.RESTORE";

        // JSAPI_STREAM_PEER_REMOVE 
        public const string JSAPI_STREAM_PEER_REMOVE = JSAPI_PREFIX + "STREAM.PEER.REMOVE";

        // JSAPI_STREAM_LEADER_STEPDOWN 
        public const string JSAPI_STREAM_LEADER_STEPDOWN = JSAPI_PREFIX + "STREAM.LEADER.STEPDOWN";

        #endregion;

        #region Consumer

        // JSAPI_CONSUMER_CREATE is used to create consumers.
        public const string JSAPI_CONSUMER_CREATE = JSAPI_PREFIX + "CONSUMER.CREATE";

        // JSAPI_DURABLE_CREATE is used to create durable consumers.
        public const string JSAPI_DURABLE_CREATE = JSAPI_PREFIX + "CONSUMER.DURABLE.CREATE";

        // JSAPI_CONSUMER_INFO is used to create consumers.
        public const string JSAPI_CONSUMER_INFO = JSAPI_PREFIX + "CONSUMER.INFO";

        // JSAPI_CONSUMER_MSG_NEXT is the prefix for the request next message(s) for a consumer in worker/pull mode.
        public const string JSAPI_CONSUMER_MSG_NEXT = JSAPI_PREFIX + "CONSUMER.MSG.NEXT";

        // JSAPI_CONSUMER_DELETE is used to delete consumers.
        public const string JSAPI_CONSUMER_DELETE = JSAPI_PREFIX + "CONSUMER.DELETE";

        // JSAPI_CONSUMER_NAMES is used to return a list of consumer names
        public const string JSAPI_CONSUMER_NAMES = JSAPI_PREFIX + "CONSUMER.NAMES";

        // JSAPI_CONSUMER_LIST is used to return all detailed consumer information
        public const string JSAPI_CONSUMER_LIST = JSAPI_PREFIX + "CONSUMER.LIST";

        public const string JSAPI_CONSUMER_LEADER_STEPDOWN = JSAPI_PREFIX + "CONSUMER.LEADER.STEPDOWN";

        #endregion;
    }
}
