using System;
using System.Collections.Generic;
using System.Text;
using pb = global::Google.Protobuf;
using pbr = global::Google.Protobuf.Reflection;

namespace DotNetty.Codecs.STAN.Protocol
{
    /// <summary>
    /// Response for CloseRequest
    /// </summary>
    public sealed partial class CloseResponse : pb::IMessage<CloseResponse>
    {
        [global::System.Diagnostics.DebuggerNonUserCode]
        public static pb::MessageParser<CloseResponse> Parser { get; } = new pb::MessageParser<CloseResponse>(() => new CloseResponse());

        [global::System.Diagnostics.DebuggerNonUserCode]
        public static pbr::MessageDescriptor Descriptor
        {
            get { return ProtocolReflection.Descriptor.MessageTypes[10]; }
        }

        [global::System.Diagnostics.DebuggerNonUserCode]
        pbr::MessageDescriptor pb::IMessage.Descriptor
        {
            get { return Descriptor; }
        }

        [global::System.Diagnostics.DebuggerNonUserCode]
        public CloseResponse()
        {
            OnConstruction();
        }

        partial void OnConstruction();

        [global::System.Diagnostics.DebuggerNonUserCode]
        public CloseResponse(CloseResponse other) : this()
        {
            error_ = other.error_;
        }

        [global::System.Diagnostics.DebuggerNonUserCode]
        public CloseResponse Clone()
        {
            return new CloseResponse(this);
        }

        /// <summary>Field number for the "error" field.</summary>
        public const int ErrorFieldNumber = 1;
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
            return Equals(other as CloseResponse);
        }

        [global::System.Diagnostics.DebuggerNonUserCode]
        public bool Equals(CloseResponse other)
        {
            if (other is null)
            {
                return false;
            }
            if (ReferenceEquals(other, this))
            {
                return true;
            }
            if (Error != other.Error) return false;
            return true;
        }

        [global::System.Diagnostics.DebuggerNonUserCode]
        public override int GetHashCode()
        {
            int hash = 1;
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
            if (Error.Length != 0)
            {
                output.WriteRawTag(10);
                output.WriteString(Error);
            }
        }

        [global::System.Diagnostics.DebuggerNonUserCode]
        public int CalculateSize()
        {
            int size = 0;
            if (Error.Length != 0)
            {
                size += 1 + pb::CodedOutputStream.ComputeStringSize(Error);
            }
            return size;
        }

        [global::System.Diagnostics.DebuggerNonUserCode]
        public void MergeFrom(CloseResponse other)
        {
            if (other == null)
            {
                return;
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
                    case 10:
                        {
                            Error = input.ReadString();
                            break;
                        }
                }
            }
        }

    }
}
