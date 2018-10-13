using System;
using System.Collections.Generic;
using System.Text;
using pb = global::Google.Protobuf;
using pbr = global::Google.Protobuf.Reflection;

namespace DotNetty.Codecs.STAN.Protocol
{
    /// <summary>
    /// Ack will deliver an ack for a delivered msg.
    /// </summary>
    public sealed partial class Ack : pb::IMessage<Ack>
    {
        [global::System.Diagnostics.DebuggerNonUserCode]
        public static pb::MessageParser<Ack> Parser { get; } = new pb::MessageParser<Ack>(() => new Ack());

        [global::System.Diagnostics.DebuggerNonUserCode]
        public static pbr::MessageDescriptor Descriptor
        {
            get { return ProtocolReflection.Descriptor.MessageTypes[3]; }
        }

        [global::System.Diagnostics.DebuggerNonUserCode]
        pbr::MessageDescriptor pb::IMessage.Descriptor
        {
            get { return Descriptor; }
        }

        [global::System.Diagnostics.DebuggerNonUserCode]
        public Ack()
        {
            OnConstruction();
        }

        partial void OnConstruction();

        [global::System.Diagnostics.DebuggerNonUserCode]
        public Ack(Ack other) : this()
        {
            subject_ = other.subject_;
            sequence_ = other.sequence_;
        }

        [global::System.Diagnostics.DebuggerNonUserCode]
        public Ack Clone()
        {
            return new Ack(this);
        }

        /// <summary>Field number for the "subject" field.</summary>
        public const int SubjectFieldNumber = 1;
        private string subject_ = string.Empty;
        /// <summary>
        /// Subject
        /// </summary>
        [global::System.Diagnostics.DebuggerNonUserCode]
        public string Subject
        {
            get { return subject_; }
            set
            {
                subject_ = pb::ProtoPreconditions.CheckNotNull(value, "value");
            }
        }

        /// <summary>Field number for the "sequence" field.</summary>
        public const int SequenceFieldNumber = 2;
        private ulong sequence_;
        /// <summary>
        /// Sequence to acknowledge
        /// </summary>
        [global::System.Diagnostics.DebuggerNonUserCode]
        public ulong Sequence
        {
            get { return sequence_; }
            set
            {
                sequence_ = value;
            }
        }

        [global::System.Diagnostics.DebuggerNonUserCode]
        public override bool Equals(object other)
        {
            return Equals(other as Ack);
        }

        [global::System.Diagnostics.DebuggerNonUserCode]
        public bool Equals(Ack other)
        {
            if (other is null)
            {
                return false;
            }
            if (ReferenceEquals(other, this))
            {
                return true;
            }
            if (Subject != other.Subject) return false;
            if (Sequence != other.Sequence) return false;
            return true;
        }

        [global::System.Diagnostics.DebuggerNonUserCode]
        public override int GetHashCode()
        {
            int hash = 1;
            if (Subject.Length != 0) hash ^= Subject.GetHashCode();
            if (Sequence != 0UL) hash ^= Sequence.GetHashCode();
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
            if (Subject.Length != 0)
            {
                output.WriteRawTag(10);
                output.WriteString(Subject);
            }
            if (Sequence != 0UL)
            {
                output.WriteRawTag(16);
                output.WriteUInt64(Sequence);
            }
        }

        [global::System.Diagnostics.DebuggerNonUserCode]
        public int CalculateSize()
        {
            int size = 0;
            if (Subject.Length != 0)
            {
                size += 1 + pb::CodedOutputStream.ComputeStringSize(Subject);
            }
            if (Sequence != 0UL)
            {
                size += 1 + pb::CodedOutputStream.ComputeUInt64Size(Sequence);
            }
            return size;
        }

        [global::System.Diagnostics.DebuggerNonUserCode]
        public void MergeFrom(Ack other)
        {
            if (other == null)
            {
                return;
            }
            if (other.Subject.Length != 0)
            {
                Subject = other.Subject;
            }
            if (other.Sequence != 0UL)
            {
                Sequence = other.Sequence;
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
                            Subject = input.ReadString();
                            break;
                        }
                    case 16:
                        {
                            Sequence = input.ReadUInt64();
                            break;
                        }
                }
            }
        }

    }
}
