using System;
using System.Collections.Generic;
using System.Text;
using pb = global::Google.Protobuf;
using pbr = global::Google.Protobuf.Reflection;

namespace DotNetty.Codecs.STAN.Protocol
{
    /// <summary>
    /// Connection Request
    /// </summary>
    public sealed partial class ConnectRequest : pb::IMessage<ConnectRequest>
    {
        [global::System.Diagnostics.DebuggerNonUserCode]
        public static pb::MessageParser<ConnectRequest> Parser { get; } = new pb::MessageParser<ConnectRequest>(() => new ConnectRequest());

        [global::System.Diagnostics.DebuggerNonUserCode]
        public static pbr::MessageDescriptor Descriptor
        {
            get { return ProtocolReflection.Descriptor.MessageTypes[4]; }
        }

        [global::System.Diagnostics.DebuggerNonUserCode]
        pbr::MessageDescriptor pb::IMessage.Descriptor
        {
            get { return Descriptor; }
        }

        [global::System.Diagnostics.DebuggerNonUserCode]
        public ConnectRequest()
        {
            OnConstruction();
        }

        partial void OnConstruction();

        [global::System.Diagnostics.DebuggerNonUserCode]
        public ConnectRequest(ConnectRequest other) : this()
        {
            clientID_ = other.clientID_;
            heartbeatInbox_ = other.heartbeatInbox_;
        }

        [global::System.Diagnostics.DebuggerNonUserCode]
        public ConnectRequest Clone()
        {
            return new ConnectRequest(this);
        }

        /// <summary>Field number for the "clientID" field.</summary>
        public const int ClientIDFieldNumber = 1;
        private string clientID_ = string.Empty;
        /// <summary>
        /// Client name/identifier.
        /// </summary>
        [global::System.Diagnostics.DebuggerNonUserCode]
        public string ClientID
        {
            get { return clientID_; }
            set
            {
                clientID_ = pb::ProtoPreconditions.CheckNotNull(value, "value");
            }
        }

        /// <summary>Field number for the "heartbeatInbox" field.</summary>
        public const int HeartbeatInboxFieldNumber = 2;
        private string heartbeatInbox_ = string.Empty;
        /// <summary>
        /// Inbox for server initiated heartbeats.
        /// </summary>
        [global::System.Diagnostics.DebuggerNonUserCode]
        public string HeartbeatInbox
        {
            get { return heartbeatInbox_; }
            set
            {
                heartbeatInbox_ = pb::ProtoPreconditions.CheckNotNull(value, "value");
            }
        }

        [global::System.Diagnostics.DebuggerNonUserCode]
        public override bool Equals(object other)
        {
            return Equals(other as ConnectRequest);
        }

        [global::System.Diagnostics.DebuggerNonUserCode]
        public bool Equals(ConnectRequest other)
        {
            if (other is null)
            {
                return false;
            }
            if (ReferenceEquals(other, this))
            {
                return true;
            }
            if (ClientID != other.ClientID) return false;
            if (HeartbeatInbox != other.HeartbeatInbox) return false;
            return true;
        }

        [global::System.Diagnostics.DebuggerNonUserCode]
        public override int GetHashCode()
        {
            int hash = 1;
            if (ClientID.Length != 0) hash ^= ClientID.GetHashCode();
            if (HeartbeatInbox.Length != 0) hash ^= HeartbeatInbox.GetHashCode();
            return hash;
        }

        [global::System.Diagnostics.DebuggerNonUserCode]
        public override string ToString()
        {
            return pb::JsonFormatter.ToDiagnosticString(this);
        }

        [global::System.Diagnostics.DebuggerNonUserCode]
        public void WriteTo(pb::CodedOutputStream output)
        {
            if (ClientID.Length != 0)
            {
                output.WriteRawTag(10);
                output.WriteString(ClientID);
            }
            if (HeartbeatInbox.Length != 0)
            {
                output.WriteRawTag(18);
                output.WriteString(HeartbeatInbox);
            }
        }

        [global::System.Diagnostics.DebuggerNonUserCode]
        public int CalculateSize()
        {
            int size = 0;
            if (ClientID.Length != 0)
            {
                size += 1 + pb::CodedOutputStream.ComputeStringSize(ClientID);
            }
            if (HeartbeatInbox.Length != 0)
            {
                size += 1 + pb::CodedOutputStream.ComputeStringSize(HeartbeatInbox);
            }
            return size;
        }

        [global::System.Diagnostics.DebuggerNonUserCode]
        public void MergeFrom(ConnectRequest other)
        {
            if (other == null)
            {
                return;
            }
            if (other.ClientID.Length != 0)
            {
                ClientID = other.ClientID;
            }
            if (other.HeartbeatInbox.Length != 0)
            {
                HeartbeatInbox = other.HeartbeatInbox;
            }
        }

        [global::System.Diagnostics.DebuggerNonUserCode]
        public void MergeFrom(pb::CodedInputStream input)
        {
            uint tag;
            while ((tag = input.ReadTag()) != 0)
            {
                switch (tag)
                {
                    default:
                        input.SkipLastField();
                        break;
                    case 10:
                        {
                            ClientID = input.ReadString();
                            break;
                        }
                    case 18:
                        {
                            HeartbeatInbox = input.ReadString();
                            break;
                        }
                }
            }
        }

    }
}
