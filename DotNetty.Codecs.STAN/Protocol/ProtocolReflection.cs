using System;
using System.Collections.Generic;
using System.Text;
using pbr = global::Google.Protobuf.Reflection;

namespace DotNetty.Codecs.STAN.Protocol
{
    public static partial class ProtocolReflection
    {

        #region Descriptor
        /// <summary>File descriptor for protocol.proto</summary>
        public static pbr::FileDescriptor Descriptor
        {
            get { return descriptor; }
        }
        private static pbr::FileDescriptor descriptor;

        static ProtocolReflection()
        {
            byte[] descriptorData = global::System.Convert.FromBase64String(
                string.Concat(
                  "Cg5wcm90b2NvbC5wcm90bxILU1RBTi5DbGllbnQiZgoGUHViTXNnEhAKCGNs",
                  "aWVudElEGAEgASgJEgwKBGd1aWQYAiABKAkSDwoHc3ViamVjdBgDIAEoCRIN",
                  "CgVyZXBseRgEIAEoCRIMCgRkYXRhGAUgASgMEg4KBnNoYTI1NhgKIAEoDCIl",
                  "CgZQdWJBY2sSDAoEZ3VpZBgBIAEoCRINCgVlcnJvchgCIAEoCSKBAQoITXNn",
                  "UHJvdG8SEAoIc2VxdWVuY2UYASABKAQSDwoHc3ViamVjdBgCIAEoCRINCgVy",
                  "ZXBseRgDIAEoCRIMCgRkYXRhGAQgASgMEhEKCXRpbWVzdGFtcBgFIAEoAxIT",
                  "CgtyZWRlbGl2ZXJlZBgGIAEoCBINCgVDUkMzMhgKIAEoDSIoCgNBY2sSDwoH",
                  "c3ViamVjdBgBIAEoCRIQCghzZXF1ZW5jZRgCIAEoBCI6Cg5Db25uZWN0UmVx",
                  "dWVzdBIQCghjbGllbnRJRBgBIAEoCRIWCg5oZWFydGJlYXRJbmJveBgCIAEo",
                  "CSKjAQoPQ29ubmVjdFJlc3BvbnNlEhEKCXB1YlByZWZpeBgBIAEoCRITCgtz",
                  "dWJSZXF1ZXN0cxgCIAEoCRIVCg11bnN1YlJlcXVlc3RzGAMgASgJEhUKDWNs",
                  "b3NlUmVxdWVzdHMYBCABKAkSDQoFZXJyb3IYBSABKAkSGAoQc3ViQ2xvc2VS",
                  "ZXF1ZXN0cxgGIAEoCRIRCglwdWJsaWNLZXkYZCABKAki+gEKE1N1YnNjcmlw",
                  "dGlvblJlcXVlc3QSEAoIY2xpZW50SUQYASABKAkSDwoHc3ViamVjdBgCIAEo",
                  "CRIOCgZxR3JvdXAYAyABKAkSDQoFaW5ib3gYBCABKAkSEwoLbWF4SW5GbGln",
                  "aHQYBSABKAUSFQoNYWNrV2FpdEluU2VjcxgGIAEoBRITCgtkdXJhYmxlTmFt",
                  "ZRgHIAEoCRIxCg1zdGFydFBvc2l0aW9uGAogASgOMhouU1RBTi5DbGllbnQu",
                  "U3RhcnRQb3NpdGlvbhIVCg1zdGFydFNlcXVlbmNlGAsgASgEEhYKDnN0YXJ0",
                  "VGltZURlbHRhGAwgASgDIjcKFFN1YnNjcmlwdGlvblJlc3BvbnNlEhAKCGFj",
                  "a0luYm94GAIgASgJEg0KBWVycm9yGAMgASgJIlsKElVuc3Vic2NyaWJlUmVx",
                  "dWVzdBIQCghjbGllbnRJRBgBIAEoCRIPCgdzdWJqZWN0GAIgASgJEg0KBWlu",
                  "Ym94GAMgASgJEhMKC2R1cmFibGVOYW1lGAQgASgJIiAKDENsb3NlUmVxdWVz",
                  "dBIQCghjbGllbnRJRBgBIAEoCSIeCg1DbG9zZVJlc3BvbnNlEg0KBWVycm9y",
                  "GAEgASgJKmAKDVN0YXJ0UG9zaXRpb24SCwoHTmV3T25seRAAEhAKDExhc3RS",
                  "ZWNlaXZlZBABEhIKDlRpbWVEZWx0YVN0YXJ0EAISEQoNU2VxdWVuY2VTdGFy",
                  "dBADEgkKBUZpcnN0EARiBnByb3RvMw=="));
            descriptor = pbr::FileDescriptor.FromGeneratedCode(descriptorData,
                new pbr::FileDescriptor[] { },
                new pbr::GeneratedClrTypeInfo(new[] { typeof(StartPosition), }, new pbr::GeneratedClrTypeInfo[] {
            new pbr::GeneratedClrTypeInfo(typeof(PubMsg), PubMsg.Parser, new[]{ "ClientID", "Guid", "Subject", "Reply", "Data", "Sha256" }, null, null, null),
            new pbr::GeneratedClrTypeInfo(typeof(PubAck), PubAck.Parser, new[]{ "Guid", "Error" }, null, null, null),
            new pbr::GeneratedClrTypeInfo(typeof(MsgProto), MsgProto.Parser, new[]{ "Sequence", "Subject", "Reply", "Data", "Timestamp", "Redelivered", "CRC32" }, null, null, null),
            new pbr::GeneratedClrTypeInfo(typeof(Ack), Ack.Parser, new[]{ "Subject", "Sequence" }, null, null, null),
            new pbr::GeneratedClrTypeInfo(typeof(ConnectRequest), ConnectRequest.Parser, new[]{ "ClientID", "HeartbeatInbox" }, null, null, null),
            new pbr::GeneratedClrTypeInfo(typeof(ConnectResponse), ConnectResponse.Parser, new[]{ "PubPrefix", "SubRequests", "UnsubRequests", "CloseRequests", "Error", "SubCloseRequests", "PublicKey" }, null, null, null),
            new pbr::GeneratedClrTypeInfo(typeof(SubscriptionRequest), SubscriptionRequest.Parser, new[]{ "ClientID", "Subject", "QGroup", "Inbox", "MaxInFlight", "AckWaitInSecs", "DurableName", "StartPosition", "StartSequence", "StartTimeDelta" }, null, null, null),
            new pbr::GeneratedClrTypeInfo(typeof(SubscriptionResponse), SubscriptionResponse.Parser, new[]{ "AckInbox", "Error" }, null, null, null),
            new pbr::GeneratedClrTypeInfo(typeof(UnsubscribeRequest), UnsubscribeRequest.Parser, new[]{ "ClientID", "Subject", "Inbox", "DurableName" }, null, null, null),
            new pbr::GeneratedClrTypeInfo(typeof(CloseRequest), CloseRequest.Parser, new[]{ "ClientID" }, null, null, null),
            new pbr::GeneratedClrTypeInfo(typeof(CloseResponse), CloseResponse.Parser, new[]{ "Error" }, null, null, null)
                }));
        }
        #endregion

    }
}
