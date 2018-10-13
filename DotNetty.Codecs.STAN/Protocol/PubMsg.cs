using System;
using System.Collections.Generic;
using System.Text;
using pb = global::Google.Protobuf;
using pbr = global::Google.Protobuf.Reflection;

namespace DotNetty.Codecs.STAN.Protocol
{
    /// <summary>
    /// How messages are delivered to the STAN cluster
    /// </summary>
    public sealed partial class PubMsg : pb::IMessage<PubMsg>
    {
        [global::System.Diagnostics.DebuggerNonUserCode]
        public static pb::MessageParser<PubMsg> Parser { get; } = new pb::MessageParser<PubMsg>(() => new PubMsg());

        [global::System.Diagnostics.DebuggerNonUserCode]
        public static pbr::MessageDescriptor Descriptor
        {
            get { return ProtocolReflection.Descriptor.MessageTypes[0]; }
        }

        [global::System.Diagnostics.DebuggerNonUserCode]
        pbr::MessageDescriptor pb::IMessage.Descriptor
        {
            get { return Descriptor; }
        }

        [global::System.Diagnostics.DebuggerNonUserCode]
        public PubMsg()
        {
            OnConstruction();
        }

        partial void OnConstruction();

        [global::System.Diagnostics.DebuggerNonUserCode]
        public PubMsg(PubMsg other) : this()
        {
            clientID_ = other.clientID_;
            guid_ = other.guid_;
            subject_ = other.subject_;
            reply_ = other.reply_;
            data_ = other.data_;
            sha256_ = other.sha256_;
        }

        [global::System.Diagnostics.DebuggerNonUserCode]
        public PubMsg Clone()
        {
            return new PubMsg(this);
        }

        /// <summary>Field number for the "clientID" field.</summary>
        public const int ClientIDFieldNumber = 1;
        private string clientID_ = string.Empty;
        /// <summary>
        /// ClientID
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

        /// <summary>Field number for the "guid" field.</summary>
        public const int GuidFieldNumber = 2;
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

        /// <summary>Field number for the "subject" field.</summary>
        public const int SubjectFieldNumber = 3;
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
        public const int ReplyFieldNumber = 4;
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
        public const int DataFieldNumber = 5;
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

        /// <summary>Field number for the "sha256" field.</summary>
        public const int Sha256FieldNumber = 10;
        private pb::ByteString sha256_ = pb::ByteString.Empty;
        /// <summary>
        /// optional sha256 of data
        /// </summary>
        [global::System.Diagnostics.DebuggerNonUserCode]
        public pb::ByteString Sha256
        {
            get { return sha256_; }
            set
            {
                sha256_ = pb::ProtoPreconditions.CheckNotNull(value, "value");
            }
        }

        [global::System.Diagnostics.DebuggerNonUserCode]
        public override bool Equals(object other)
        {
            return Equals(other as PubMsg);
        }

        [global::System.Diagnostics.DebuggerNonUserCode]
        public bool Equals(PubMsg other)
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
            if (Guid != other.Guid) return false;
            if (Subject != other.Subject) return false;
            if (Reply != other.Reply) return false;
            if (Data != other.Data) return false;
            if (Sha256 != other.Sha256) return false;
            return true;
        }

        [global::System.Diagnostics.DebuggerNonUserCode]
        public override int GetHashCode()
        {
            int hash = 1;
            if (ClientID.Length != 0) hash ^= ClientID.GetHashCode();
            if (Guid.Length != 0) hash ^= Guid.GetHashCode();
            if (Subject.Length != 0) hash ^= Subject.GetHashCode();
            if (Reply.Length != 0) hash ^= Reply.GetHashCode();
            if (Data.Length != 0) hash ^= Data.GetHashCode();
            if (Sha256.Length != 0) hash ^= Sha256.GetHashCode();
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
            if (Guid.Length != 0)
            {
                output.WriteRawTag(18);
                output.WriteString(Guid);
            }
            if (Subject.Length != 0)
            {
                output.WriteRawTag(26);
                output.WriteString(Subject);
            }
            if (Reply.Length != 0)
            {
                output.WriteRawTag(34);
                output.WriteString(Reply);
            }
            if (Data.Length != 0)
            {
                output.WriteRawTag(42);
                output.WriteBytes(Data);
            }
            if (Sha256.Length != 0)
            {
                output.WriteRawTag(82);
                output.WriteBytes(Sha256);
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
            if (Guid.Length != 0)
            {
                size += 1 + pb::CodedOutputStream.ComputeStringSize(Guid);
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
            if (Sha256.Length != 0)
            {
                size += 1 + pb::CodedOutputStream.ComputeBytesSize(Sha256);
            }
            return size;
        }

        [global::System.Diagnostics.DebuggerNonUserCode]
        public void MergeFrom(PubMsg other)
        {
            if (other == null)
            {
                return;
            }
            if (other.ClientID.Length != 0)
            {
                ClientID = other.ClientID;
            }
            if (other.Guid.Length != 0)
            {
                Guid = other.Guid;
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
            if (other.Sha256.Length != 0)
            {
                Sha256 = other.Sha256;
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
                            Guid = input.ReadString();
                            break;
                        }
                    case 26:
                        {
                            Subject = input.ReadString();
                            break;
                        }
                    case 34:
                        {
                            Reply = input.ReadString();
                            break;
                        }
                    case 42:
                        {
                            Data = input.ReadBytes();
                            break;
                        }
                    case 82:
                        {
                            Sha256 = input.ReadBytes();
                            break;
                        }
                }
            }
        }

    }
}
