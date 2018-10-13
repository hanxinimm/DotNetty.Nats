using System;
using System.Collections.Generic;
using System.Text;
using pb = global::Google.Protobuf;
using pbr = global::Google.Protobuf.Reflection;

namespace DotNetty.Codecs.STAN.Protocol
{
    /// <summary>
    /// Msg struct. Sequence is assigned for global ordering by
    /// the cluster after the publisher has been acknowledged.
    /// </summary>
    public sealed partial class MsgProto : pb::IMessage<MsgProto>
    {
        [global::System.Diagnostics.DebuggerNonUserCode]
        public static pb::MessageParser<MsgProto> Parser { get; } = new pb::MessageParser<MsgProto>(() => new MsgProto());

        [global::System.Diagnostics.DebuggerNonUserCode]
        public static pbr::MessageDescriptor Descriptor
        {
            get { return ProtocolReflection.Descriptor.MessageTypes[2]; }
        }

        [global::System.Diagnostics.DebuggerNonUserCode]
        pbr::MessageDescriptor pb::IMessage.Descriptor
        {
            get { return Descriptor; }
        }

        [global::System.Diagnostics.DebuggerNonUserCode]
        public MsgProto()
        {
            OnConstruction();
        }

        partial void OnConstruction();

        [global::System.Diagnostics.DebuggerNonUserCode]
        public MsgProto(MsgProto other) : this()
        {
            sequence_ = other.sequence_;
            subject_ = other.subject_;
            reply_ = other.reply_;
            data_ = other.data_;
            timestamp_ = other.timestamp_;
            redelivered_ = other.redelivered_;
            cRC32_ = other.cRC32_;
        }

        [global::System.Diagnostics.DebuggerNonUserCode]
        public MsgProto Clone()
        {
            return new MsgProto(this);
        }

        /// <summary>Field number for the "sequence" field.</summary>
        public const int SequenceFieldNumber = 1;
        private ulong sequence_;
        /// <summary>
        /// globally ordered sequence number for the subject's channel
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

        /// <summary>Field number for the "subject" field.</summary>
        public const int SubjectFieldNumber = 2;
        private string subject_ = string.Empty;
        /// <summary>
        /// subject
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

        /// <summary>Field number for the "reply" field.</summary>
        public const int ReplyFieldNumber = 3;
        private string reply_ = string.Empty;
        /// <summary>
        /// optional reply
        /// </summary>
        [global::System.Diagnostics.DebuggerNonUserCode]
        public string Reply
        {
            get { return reply_; }
            set
            {
                reply_ = pb::ProtoPreconditions.CheckNotNull(value, "value");
            }
        }

        /// <summary>Field number for the "data" field.</summary>
        public const int DataFieldNumber = 4;
        private pb::ByteString data_ = pb::ByteString.Empty;
        /// <summary>
        /// payload
        /// </summary>
        [global::System.Diagnostics.DebuggerNonUserCode]
        public pb::ByteString Data
        {
            get { return data_; }
            set
            {
                data_ = pb::ProtoPreconditions.CheckNotNull(value, "value");
            }
        }

        /// <summary>Field number for the "timestamp" field.</summary>
        public const int TimestampFieldNumber = 5;
        private long timestamp_;
        /// <summary>
        /// received timestamp
        /// </summary>
        [global::System.Diagnostics.DebuggerNonUserCode]
        public long Timestamp
        {
            get { return timestamp_; }
            set
            {
                timestamp_ = value;
            }
        }

        /// <summary>Field number for the "redelivered" field.</summary>
        public const int RedeliveredFieldNumber = 6;
        private bool redelivered_;
        /// <summary>
        /// Flag specifying if the message is being redelivered
        /// </summary>
        [global::System.Diagnostics.DebuggerNonUserCode]
        public bool Redelivered
        {
            get { return redelivered_; }
            set
            {
                redelivered_ = value;
            }
        }

        /// <summary>Field number for the "CRC32" field.</summary>
        public const int CRC32FieldNumber = 10;
        private uint cRC32_;
        /// <summary>
        /// optional IEEE CRC32
        /// </summary>
        [global::System.Diagnostics.DebuggerNonUserCode]
        public uint CRC32
        {
            get { return cRC32_; }
            set
            {
                cRC32_ = value;
            }
        }

        [global::System.Diagnostics.DebuggerNonUserCode]
        public override bool Equals(object other)
        {
            return Equals(other as MsgProto);
        }

        [global::System.Diagnostics.DebuggerNonUserCode]
        public bool Equals(MsgProto other)
        {
            if (other is null)
            {
                return false;
            }
            if (ReferenceEquals(other, this))
            {
                return true;
            }
            if (Sequence != other.Sequence) return false;
            if (Subject != other.Subject) return false;
            if (Reply != other.Reply) return false;
            if (Data != other.Data) return false;
            if (Timestamp != other.Timestamp) return false;
            if (Redelivered != other.Redelivered) return false;
            if (CRC32 != other.CRC32) return false;
            return true;
        }

        [global::System.Diagnostics.DebuggerNonUserCode]
        public override int GetHashCode()
        {
            int hash = 1;
            if (Sequence != 0UL) hash ^= Sequence.GetHashCode();
            if (Subject.Length != 0) hash ^= Subject.GetHashCode();
            if (Reply.Length != 0) hash ^= Reply.GetHashCode();
            if (Data.Length != 0) hash ^= Data.GetHashCode();
            if (Timestamp != 0L) hash ^= Timestamp.GetHashCode();
            if (Redelivered != false) hash ^= Redelivered.GetHashCode();
            if (CRC32 != 0) hash ^= CRC32.GetHashCode();
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
            if (Sequence != 0UL)
            {
                output.WriteRawTag(8);
                output.WriteUInt64(Sequence);
            }
            if (Subject.Length != 0)
            {
                output.WriteRawTag(18);
                output.WriteString(Subject);
            }
            if (Reply.Length != 0)
            {
                output.WriteRawTag(26);
                output.WriteString(Reply);
            }
            if (Data.Length != 0)
            {
                output.WriteRawTag(34);
                output.WriteBytes(Data);
            }
            if (Timestamp != 0L)
            {
                output.WriteRawTag(40);
                output.WriteInt64(Timestamp);
            }
            if (Redelivered != false)
            {
                output.WriteRawTag(48);
                output.WriteBool(Redelivered);
            }
            if (CRC32 != 0)
            {
                output.WriteRawTag(80);
                output.WriteUInt32(CRC32);
            }
        }

        [global::System.Diagnostics.DebuggerNonUserCode]
        public int CalculateSize()
        {
            int size = 0;
            if (Sequence != 0UL)
            {
                size += 1 + pb::CodedOutputStream.ComputeUInt64Size(Sequence);
            }
            if (Subject.Length != 0)
            {
                size += 1 + pb::CodedOutputStream.ComputeStringSize(Subject);
            }
            if (Reply.Length != 0)
            {
                size += 1 + pb::CodedOutputStream.ComputeStringSize(Reply);
            }
            if (Data.Length != 0)
            {
                size += 1 + pb::CodedOutputStream.ComputeBytesSize(Data);
            }
            if (Timestamp != 0L)
            {
                size += 1 + pb::CodedOutputStream.ComputeInt64Size(Timestamp);
            }
            if (Redelivered != false)
            {
                size += 1 + 1;
            }
            if (CRC32 != 0)
            {
                size += 1 + pb::CodedOutputStream.ComputeUInt32Size(CRC32);
            }
            return size;
        }

        [global::System.Diagnostics.DebuggerNonUserCode]
        public void MergeFrom(MsgProto other)
        {
            if (other == null)
            {
                return;
            }
            if (other.Sequence != 0UL)
            {
                Sequence = other.Sequence;
            }
            if (other.Subject.Length != 0)
            {
                Subject = other.Subject;
            }
            if (other.Reply.Length != 0)
            {
                Reply = other.Reply;
            }
            if (other.Data.Length != 0)
            {
                Data = other.Data;
            }
            if (other.Timestamp != 0L)
            {
                Timestamp = other.Timestamp;
            }
            if (other.Redelivered != false)
            {
                Redelivered = other.Redelivered;
            }
            if (other.CRC32 != 0)
            {
                CRC32 = other.CRC32;
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
                    case 8:
                        {
                            Sequence = input.ReadUInt64();
                            break;
                        }
                    case 18:
                        {
                            Subject = input.ReadString();
                            break;
                        }
                    case 26:
                        {
                            Reply = input.ReadString();
                            break;
                        }
                    case 34:
                        {
                            Data = input.ReadBytes();
                            break;
                        }
                    case 40:
                        {
                            Timestamp = input.ReadInt64();
                            break;
                        }
                    case 48:
                        {
                            Redelivered = input.ReadBool();
                            break;
                        }
                    case 80:
                        {
                            CRC32 = input.ReadUInt32();
                            break;
                        }
                }
            }
        }

    }
}
