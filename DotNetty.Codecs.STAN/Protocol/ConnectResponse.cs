using System;
using System.Collections.Generic;
using System.Text;
using pb = global::Google.Protobuf;
using pbr = global::Google.Protobuf.Reflection;

namespace DotNetty.Codecs.STAN.Protocol
{
    /// <summary>
    /// Response to a client connect
    /// </summary>
    public sealed partial class ConnectResponse : pb::IMessage<ConnectResponse>
    {
        [global::System.Diagnostics.DebuggerNonUserCode]
        public static pb::MessageParser<ConnectResponse> Parser { get; } = new pb::MessageParser<ConnectResponse>(() => new ConnectResponse());

        [global::System.Diagnostics.DebuggerNonUserCode]
        public static pbr::MessageDescriptor Descriptor
        {
            get { return ProtocolReflection.Descriptor.MessageTypes[5]; }
        }

        [global::System.Diagnostics.DebuggerNonUserCode]
        pbr::MessageDescriptor pb::IMessage.Descriptor
        {
            get { return Descriptor; }
        }

        [global::System.Diagnostics.DebuggerNonUserCode]
        public ConnectResponse()
        {
            OnConstruction();
        }

        partial void OnConstruction();

        [global::System.Diagnostics.DebuggerNonUserCode]
        public ConnectResponse(ConnectResponse other) : this()
        {
            pubPrefix_ = other.pubPrefix_;
            subRequests_ = other.subRequests_;
            unsubRequests_ = other.unsubRequests_;
            closeRequests_ = other.closeRequests_;
            error_ = other.error_;
            subCloseRequests_ = other.subCloseRequests_;
            publicKey_ = other.publicKey_;
        }

        [global::System.Diagnostics.DebuggerNonUserCode]
        public ConnectResponse Clone()
        {
            return new ConnectResponse(this);
        }

        /// <summary>Field number for the "pubPrefix" field.</summary>
        public const int PubPrefixFieldNumber = 1;
        private string pubPrefix_ = string.Empty;
        /// <summary>
        /// Prefix to use when publishing to this STAN cluster
        /// </summary>
        [global::System.Diagnostics.DebuggerNonUserCode]
        public string PubPrefix
        {
            get { return pubPrefix_; }
            set
            {
                pubPrefix_ = pb::ProtoPreconditions.CheckNotNull(value, "value");
            }
        }

        /// <summary>Field number for the "subRequests" field.</summary>
        public const int SubRequestsFieldNumber = 2;
        private string subRequests_ = string.Empty;
        /// <summary>
        /// Subject to use for subscription requests
        /// </summary>
        [global::System.Diagnostics.DebuggerNonUserCode]
        public string SubRequests
        {
            get { return subRequests_; }
            set
            {
                subRequests_ = pb::ProtoPreconditions.CheckNotNull(value, "value");
            }
        }

        /// <summary>Field number for the "unsubRequests" field.</summary>
        public const int UnsubRequestsFieldNumber = 3;
        private string unsubRequests_ = string.Empty;
        /// <summary>
        /// Subject to use for unsubscribe requests
        /// </summary>
        [global::System.Diagnostics.DebuggerNonUserCode]
        public string UnsubRequests
        {
            get { return unsubRequests_; }
            set
            {
                unsubRequests_ = pb::ProtoPreconditions.CheckNotNull(value, "value");
            }
        }

        /// <summary>Field number for the "closeRequests" field.</summary>
        public const int CloseRequestsFieldNumber = 4;
        private string closeRequests_ = string.Empty;
        /// <summary>
        /// Subject for closing the stan connection
        /// </summary>
        [global::System.Diagnostics.DebuggerNonUserCode]
        public string CloseRequests
        {
            get { return closeRequests_; }
            set
            {
                closeRequests_ = pb::ProtoPreconditions.CheckNotNull(value, "value");
            }
        }

        /// <summary>Field number for the "error" field.</summary>
        public const int ErrorFieldNumber = 5;
        private string error_ = string.Empty;
        /// <summary>
        /// err string, empty/omitted if no error
        /// </summary>
        [global::System.Diagnostics.DebuggerNonUserCode]
        public string Error
        {
            get { return error_; }
            set
            {
                error_ = pb::ProtoPreconditions.CheckNotNull(value, "value");
            }
        }

        /// <summary>Field number for the "subCloseRequests" field.</summary>
        public const int SubCloseRequestsFieldNumber = 6;
        private string subCloseRequests_ = string.Empty;
        /// <summary>
        /// Subject to use for subscription close requests
        /// </summary>
        [global::System.Diagnostics.DebuggerNonUserCode]
        public string SubCloseRequests
        {
            get { return subCloseRequests_; }
            set
            {
                subCloseRequests_ = pb::ProtoPreconditions.CheckNotNull(value, "value");
            }
        }

        /// <summary>Field number for the "publicKey" field.</summary>
        public const int PublicKeyFieldNumber = 100;
        private string publicKey_ = string.Empty;
        /// <summary>
        /// Possibly used to sign acks, etc.
        /// </summary>
        [global::System.Diagnostics.DebuggerNonUserCode]
        public string PublicKey
        {
            get { return publicKey_; }
            set
            {
                publicKey_ = pb::ProtoPreconditions.CheckNotNull(value, "value");
            }
        }

        [global::System.Diagnostics.DebuggerNonUserCode]
        public override bool Equals(object other)
        {
            return Equals(other as ConnectResponse);
        }

        [global::System.Diagnostics.DebuggerNonUserCode]
        public bool Equals(ConnectResponse other)
        {
            if (other is null)
            {
                return false;
            }
            if (ReferenceEquals(other, this))
            {
                return true;
            }
            if (PubPrefix != other.PubPrefix) return false;
            if (SubRequests != other.SubRequests) return false;
            if (UnsubRequests != other.UnsubRequests) return false;
            if (CloseRequests != other.CloseRequests) return false;
            if (Error != other.Error) return false;
            if (SubCloseRequests != other.SubCloseRequests) return false;
            if (PublicKey != other.PublicKey) return false;
            return true;
        }

        [global::System.Diagnostics.DebuggerNonUserCode]
        public override int GetHashCode()
        {
            int hash = 1;
            if (PubPrefix.Length != 0) hash ^= PubPrefix.GetHashCode();
            if (SubRequests.Length != 0) hash ^= SubRequests.GetHashCode();
            if (UnsubRequests.Length != 0) hash ^= UnsubRequests.GetHashCode();
            if (CloseRequests.Length != 0) hash ^= CloseRequests.GetHashCode();
            if (Error.Length != 0) hash ^= Error.GetHashCode();
            if (SubCloseRequests.Length != 0) hash ^= SubCloseRequests.GetHashCode();
            if (PublicKey.Length != 0) hash ^= PublicKey.GetHashCode();
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
            if (PubPrefix.Length != 0)
            {
                output.WriteRawTag(10);
                output.WriteString(PubPrefix);
            }
            if (SubRequests.Length != 0)
            {
                output.WriteRawTag(18);
                output.WriteString(SubRequests);
            }
            if (UnsubRequests.Length != 0)
            {
                output.WriteRawTag(26);
                output.WriteString(UnsubRequests);
            }
            if (CloseRequests.Length != 0)
            {
                output.WriteRawTag(34);
                output.WriteString(CloseRequests);
            }
            if (Error.Length != 0)
            {
                output.WriteRawTag(42);
                output.WriteString(Error);
            }
            if (SubCloseRequests.Length != 0)
            {
                output.WriteRawTag(50);
                output.WriteString(SubCloseRequests);
            }
            if (PublicKey.Length != 0)
            {
                output.WriteRawTag(162, 6);
                output.WriteString(PublicKey);
            }
        }

        [global::System.Diagnostics.DebuggerNonUserCode]
        public int CalculateSize()
        {
            int size = 0;
            if (PubPrefix.Length != 0)
            {
                size += 1 + pb::CodedOutputStream.ComputeStringSize(PubPrefix);
            }
            if (SubRequests.Length != 0)
            {
                size += 1 + pb::CodedOutputStream.ComputeStringSize(SubRequests);
            }
            if (UnsubRequests.Length != 0)
            {
                size += 1 + pb::CodedOutputStream.ComputeStringSize(UnsubRequests);
            }
            if (CloseRequests.Length != 0)
            {
                size += 1 + pb::CodedOutputStream.ComputeStringSize(CloseRequests);
            }
            if (Error.Length != 0)
            {
                size += 1 + pb::CodedOutputStream.ComputeStringSize(Error);
            }
            if (SubCloseRequests.Length != 0)
            {
                size += 1 + pb::CodedOutputStream.ComputeStringSize(SubCloseRequests);
            }
            if (PublicKey.Length != 0)
            {
                size += 2 + pb::CodedOutputStream.ComputeStringSize(PublicKey);
            }
            return size;
        }

        [global::System.Diagnostics.DebuggerNonUserCode]
        public void MergeFrom(ConnectResponse other)
        {
            if (other == null)
            {
                return;
            }
            if (other.PubPrefix.Length != 0)
            {
                PubPrefix = other.PubPrefix;
            }
            if (other.SubRequests.Length != 0)
            {
                SubRequests = other.SubRequests;
            }
            if (other.UnsubRequests.Length != 0)
            {
                UnsubRequests = other.UnsubRequests;
            }
            if (other.CloseRequests.Length != 0)
            {
                CloseRequests = other.CloseRequests;
            }
            if (other.Error.Length != 0)
            {
                Error = other.Error;
            }
            if (other.SubCloseRequests.Length != 0)
            {
                SubCloseRequests = other.SubCloseRequests;
            }
            if (other.PublicKey.Length != 0)
            {
                PublicKey = other.PublicKey;
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
                            PubPrefix = input.ReadString();
                            break;
                        }
                    case 18:
                        {
                            SubRequests = input.ReadString();
                            break;
                        }
                    case 26:
                        {
                            UnsubRequests = input.ReadString();
                            break;
                        }
                    case 34:
                        {
                            CloseRequests = input.ReadString();
                            break;
                        }
                    case 42:
                        {
                            Error = input.ReadString();
                            break;
                        }
                    case 50:
                        {
                            SubCloseRequests = input.ReadString();
                            break;
                        }
                    case 802:
                        {
                            PublicKey = input.ReadString();
                            break;
                        }
                }
            }
        }

    }
}
