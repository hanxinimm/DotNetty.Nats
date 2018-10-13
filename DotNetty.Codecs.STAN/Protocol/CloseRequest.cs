using System;
using System.Collections.Generic;
using System.Text;
using pb = global::Google.Protobuf;
using pbr = global::Google.Protobuf.Reflection;

namespace DotNetty.Codecs.STAN.Protocol
{
    /// <summary>
    /// Protocol for a client to close a connection
    /// </summary>
    public sealed partial class CloseRequest : pb::IMessage<CloseRequest>
    {
        [global::System.Diagnostics.DebuggerNonUserCode]
        public static pb::MessageParser<CloseRequest> Parser { get; } = new pb::MessageParser<CloseRequest>(() => new CloseRequest());

        [global::System.Diagnostics.DebuggerNonUserCode]
        public static pbr::MessageDescriptor Descriptor
        {
            get { return ProtocolReflection.Descriptor.MessageTypes[9]; }
        }

        [global::System.Diagnostics.DebuggerNonUserCode]
        pbr::MessageDescriptor pb::IMessage.Descriptor
        {
            get { return Descriptor; }
        }

        [global::System.Diagnostics.DebuggerNonUserCode]
        public CloseRequest()
        {
            OnConstruction();
        }

        partial void OnConstruction();

        [global::System.Diagnostics.DebuggerNonUserCode]
        public CloseRequest(CloseRequest other) : this()
        {
            clientID_ = other.clientID_;
        }

        [global::System.Diagnostics.DebuggerNonUserCode]
        public CloseRequest Clone()
        {
            return new CloseRequest(this);
        }

        /// <summary>Field number for the "clientID" field.</summary>
        public const int ClientIDFieldNumber = 1;
        private string clientID_ = string.Empty;
        /// <summary>
        /// Client name provided to Connect() requests
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

        [global::System.Diagnostics.DebuggerNonUserCode]
        public override bool Equals(object other)
        {
            return Equals(other as CloseRequest);
        }

        [global::System.Diagnostics.DebuggerNonUserCode]
        public bool Equals(CloseRequest other)
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
            return true;
        }

        [global::System.Diagnostics.DebuggerNonUserCode]
        public override int GetHashCode()
        {
            int hash = 1;
            if (ClientID.Length != 0) hash ^= ClientID.GetHashCode();
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
        }

        [global::System.Diagnostics.DebuggerNonUserCode]
        public int CalculateSize()
        {
            int size = 0;
            if (ClientID.Length != 0)
            {
                size += 1 + pb::CodedOutputStream.ComputeStringSize(ClientID);
            }
            return size;
        }

        [global::System.Diagnostics.DebuggerNonUserCode]
        public void MergeFrom(CloseRequest other)
        {
            if (other == null)
            {
                return;
            }
            if (other.ClientID.Length != 0)
            {
                ClientID = other.ClientID;
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
                }
            }
        }

    }
}
