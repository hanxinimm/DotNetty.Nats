using System;
using System.Collections.Generic;
using System.Text;
using pb = global::Google.Protobuf;
using pbr = global::Google.Protobuf.Reflection;

namespace DotNetty.Codecs.STAN.Protocol
{
    /// <summary>
    /// Response for SubscriptionRequest and UnsubscribeRequests
    /// </summary>
    public sealed partial class SubscriptionResponse : pb::IMessage<SubscriptionResponse>
    {
        [global::System.Diagnostics.DebuggerNonUserCode]
        public static pb::MessageParser<SubscriptionResponse> Parser { get; } = new pb::MessageParser<SubscriptionResponse>(() => new SubscriptionResponse());

        [global::System.Diagnostics.DebuggerNonUserCode]
        public static pbr::MessageDescriptor Descriptor
        {
            get { return ProtocolReflection.Descriptor.MessageTypes[7]; }
        }

        [global::System.Diagnostics.DebuggerNonUserCode]
        pbr::MessageDescriptor pb::IMessage.Descriptor
        {
            get { return Descriptor; }
        }

        [global::System.Diagnostics.DebuggerNonUserCode]
        public SubscriptionResponse()
        {
            OnConstruction();
        }

        partial void OnConstruction();

        [global::System.Diagnostics.DebuggerNonUserCode]
        public SubscriptionResponse(SubscriptionResponse other) : this()
        {
            ackInbox_ = other.ackInbox_;
            error_ = other.error_;
        }

        [global::System.Diagnostics.DebuggerNonUserCode]
        public SubscriptionResponse Clone()
        {
            return new SubscriptionResponse(this);
        }

        /// <summary>Field number for the "ackInbox" field.</summary>
        public const int AckInboxFieldNumber = 2;
        private string ackInbox_ = string.Empty;
        /// <summary>
        /// ackInbox for sending acks
        /// </summary>
        [global::System.Diagnostics.DebuggerNonUserCode]
        public string AckInbox
        {
            get { return ackInbox_; }
            set
            {
                ackInbox_ = pb::ProtoPreconditions.CheckNotNull(value, "value");
            }
        }

        /// <summary>Field number for the "error" field.</summary>
        public const int ErrorFieldNumber = 3;
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

        [global::System.Diagnostics.DebuggerNonUserCode]
        public override bool Equals(object other)
        {
            return Equals(other as SubscriptionResponse);
        }

        [global::System.Diagnostics.DebuggerNonUserCode]
        public bool Equals(SubscriptionResponse other)
        {
            if (other is null)
            {
                return false;
            }
            if (ReferenceEquals(other, this))
            {
                return true;
            }
            if (AckInbox != other.AckInbox) return false;
            if (Error != other.Error) return false;
            return true;
        }

        [global::System.Diagnostics.DebuggerNonUserCode]
        public override int GetHashCode()
        {
            int hash = 1;
            if (AckInbox.Length != 0) hash ^= AckInbox.GetHashCode();
            if (Error.Length != 0) hash ^= Error.GetHashCode();
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
            if (AckInbox.Length != 0)
            {
                output.WriteRawTag(18);
                output.WriteString(AckInbox);
            }
            if (Error.Length != 0)
            {
                output.WriteRawTag(26);
                output.WriteString(Error);
            }
        }

        [global::System.Diagnostics.DebuggerNonUserCode]
        public int CalculateSize()
        {
            int size = 0;
            if (AckInbox.Length != 0)
            {
                size += 1 + pb::CodedOutputStream.ComputeStringSize(AckInbox);
            }
            if (Error.Length != 0)
            {
                size += 1 + pb::CodedOutputStream.ComputeStringSize(Error);
            }
            return size;
        }

        [global::System.Diagnostics.DebuggerNonUserCode]
        public void MergeFrom(SubscriptionResponse other)
        {
            if (other == null)
            {
                return;
            }
            if (other.AckInbox.Length != 0)
            {
                AckInbox = other.AckInbox;
            }
            if (other.Error.Length != 0)
            {
                Error = other.Error;
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
                    case 18:
                        {
                            AckInbox = input.ReadString();
                            break;
                        }
                    case 26:
                        {
                            Error = input.ReadString();
                            break;
                        }
                }
            }
        }

    }
}
