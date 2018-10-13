using System;
using System.Collections.Generic;
using System.Text;
using pb = global::Google.Protobuf;
using pbr = global::Google.Protobuf.Reflection;

namespace DotNetty.Codecs.STAN.Protocol
{
    /// <summary>
    ///  Used to ACK to publishers
    /// </summary>
    public sealed partial class PubAck : pb::IMessage<PubAck>
    {
        [global::System.Diagnostics.DebuggerNonUserCode]
        public static pb::MessageParser<PubAck> Parser { get; } = new pb::MessageParser<PubAck>(() => new PubAck());

        [global::System.Diagnostics.DebuggerNonUserCode]
        public static pbr::MessageDescriptor Descriptor
        {
            get { return ProtocolReflection.Descriptor.MessageTypes[1]; }
        }

        [global::System.Diagnostics.DebuggerNonUserCode]
        pbr::MessageDescriptor pb::IMessage.Descriptor
        {
            get { return Descriptor; }
        }

        [global::System.Diagnostics.DebuggerNonUserCode]
        public PubAck()
        {
            OnConstruction();
        }

        partial void OnConstruction();

        [global::System.Diagnostics.DebuggerNonUserCode]
        public PubAck(PubAck other) : this()
        {
            guid_ = other.guid_;
            error_ = other.error_;
        }

        [global::System.Diagnostics.DebuggerNonUserCode]
        public PubAck Clone()
        {
            return new PubAck(this);
        }

        /// <summary>Field number for the "guid" field.</summary>
        public const int GuidFieldNumber = 1;
        private string guid_ = string.Empty;
        /// <summary>
        /// guid
        /// </summary>
        [global::System.Diagnostics.DebuggerNonUserCode]
        public string Guid
        {
            get { return guid_; }
            set
            {
                guid_ = pb::ProtoPreconditions.CheckNotNull(value, "value");
            }
        }

        /// <summary>Field number for the "error" field.</summary>
        public const int ErrorFieldNumber = 2;
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
            return Equals(other as PubAck);
        }

        [global::System.Diagnostics.DebuggerNonUserCode]
        public bool Equals(PubAck other)
        {
            if (other is null)
            {
                return false;
            }
            if (ReferenceEquals(other, this))
            {
                return true;
            }
            if (Guid != other.Guid) return false;
            if (Error != other.Error) return false;
            return true;
        }

        [global::System.Diagnostics.DebuggerNonUserCode]
        public override int GetHashCode()
        {
            int hash = 1;
            if (Guid.Length != 0) hash ^= Guid.GetHashCode();
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
            if (Guid.Length != 0)
            {
                output.WriteRawTag(10);
                output.WriteString(Guid);
            }
            if (Error.Length != 0)
            {
                output.WriteRawTag(18);
                output.WriteString(Error);
            }
        }

        [global::System.Diagnostics.DebuggerNonUserCode]
        public int CalculateSize()
        {
            int size = 0;
            if (Guid.Length != 0)
            {
                size += 1 + pb::CodedOutputStream.ComputeStringSize(Guid);
            }
            if (Error.Length != 0)
            {
                size += 1 + pb::CodedOutputStream.ComputeStringSize(Error);
            }
            return size;
        }

        [global::System.Diagnostics.DebuggerNonUserCode]
        public void MergeFrom(PubAck other)
        {
            if (other == null)
            {
                return;
            }
            if (other.Guid.Length != 0)
            {
                Guid = other.Guid;
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
                            Guid = input.ReadString();
                            break;
                        }
                    case 18:
                        {
                            Error = input.ReadString();
                            break;
                        }
                }
            }
        }

    }

}
