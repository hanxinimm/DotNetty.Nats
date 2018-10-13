using System;
using System.Collections.Generic;
using System.Text;
using pb = global::Google.Protobuf;
using pbr = global::Google.Protobuf.Reflection;

namespace DotNetty.Codecs.STAN.Protocol
{
    /// <summary>
    /// Protocol for a client to subscribe
    /// </summary>
    public sealed partial class SubscriptionRequest : pb::IMessage<SubscriptionRequest>
    {
        [global::System.Diagnostics.DebuggerNonUserCode]
        public static pb::MessageParser<SubscriptionRequest> Parser { get; } = new pb::MessageParser<SubscriptionRequest>(() => new SubscriptionRequest());

        [global::System.Diagnostics.DebuggerNonUserCode]
        public static pbr::MessageDescriptor Descriptor
        {
            get { return ProtocolReflection.Descriptor.MessageTypes[6]; }
        }

        [global::System.Diagnostics.DebuggerNonUserCode]
        pbr::MessageDescriptor pb::IMessage.Descriptor
        {
            get { return Descriptor; }
        }

        [global::System.Diagnostics.DebuggerNonUserCode]
        public SubscriptionRequest()
        {
            OnConstruction();
        }

        partial void OnConstruction();

        [global::System.Diagnostics.DebuggerNonUserCode]
        public SubscriptionRequest(SubscriptionRequest other) : this()
        {
            clientID_ = other.clientID_;
            subject_ = other.subject_;
            qGroup_ = other.qGroup_;
            inbox_ = other.inbox_;
            maxInFlight_ = other.maxInFlight_;
            ackWaitInSecs_ = other.ackWaitInSecs_;
            durableName_ = other.durableName_;
            StartPosition = other.StartPosition;
            startSequence_ = other.startSequence_;
            startTimeDelta_ = other.startTimeDelta_;
        }

        [global::System.Diagnostics.DebuggerNonUserCode]
        public SubscriptionRequest Clone()
        {
            return new SubscriptionRequest(this);
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

        /// <summary>Field number for the "subject" field.</summary>
        public const int SubjectFieldNumber = 2;
        private string subject_ = string.Empty;
        /// <summary>
        /// Formal subject to subscribe to, e.g. foo.bar
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

        /// <summary>Field number for the "qGroup" field.</summary>
        public const int QGroupFieldNumber = 3;
        private string qGroup_ = string.Empty;
        /// <summary>
        /// Optional queue group
        /// </summary>
        [global::System.Diagnostics.DebuggerNonUserCode]
        public string QGroup
        {
            get { return qGroup_; }
            set
            {
                qGroup_ = pb::ProtoPreconditions.CheckNotNull(value, "value");
            }
        }

        /// <summary>Field number for the "inbox" field.</summary>
        public const int InboxFieldNumber = 4;
        private string inbox_ = string.Empty;
        /// <summary>
        /// Inbox subject to deliver messages on
        /// </summary>
        [global::System.Diagnostics.DebuggerNonUserCode]
        public string Inbox
        {
            get { return inbox_; }
            set
            {
                inbox_ = pb::ProtoPreconditions.CheckNotNull(value, "value");
            }
        }

        /// <summary>Field number for the "maxInFlight" field.</summary>
        public const int MaxInFlightFieldNumber = 5;
        private int maxInFlight_;
        /// <summary>
        /// Maximum inflight messages without an ack allowed
        /// </summary>
        [global::System.Diagnostics.DebuggerNonUserCode]
        public int MaxInFlight
        {
            get { return maxInFlight_; }
            set
            {
                maxInFlight_ = value;
            }
        }

        /// <summary>Field number for the "ackWaitInSecs" field.</summary>
        public const int AckWaitInSecsFieldNumber = 6;
        private int ackWaitInSecs_;
        /// <summary>
        /// Timeout for receiving an ack from the client
        /// </summary>
        [global::System.Diagnostics.DebuggerNonUserCode]
        public int AckWaitInSecs
        {
            get { return ackWaitInSecs_; }
            set
            {
                ackWaitInSecs_ = value;
            }
        }

        /// <summary>Field number for the "durableName" field.</summary>
        public const int DurableNameFieldNumber = 7;
        private string durableName_ = string.Empty;
        /// <summary>
        /// Optional durable name which survives client restarts
        /// </summary>
        [global::System.Diagnostics.DebuggerNonUserCode]
        public string DurableName
        {
            get { return durableName_; }
            set
            {
                durableName_ = pb::ProtoPreconditions.CheckNotNull(value, "value");
            }
        }

        /// <summary>Field number for the "startPosition" field.</summary>
        public const int StartPositionFieldNumber = 10;
        /// <summary>
        /// Start position
        /// </summary>
        [global::System.Diagnostics.DebuggerNonUserCode]
        public StartPosition StartPosition { get; set; } = 0;

        /// <summary>Field number for the "startSequence" field.</summary>
        public const int StartSequenceFieldNumber = 11;
        private ulong startSequence_;
        /// <summary>
        /// Optional start sequence number
        /// </summary>
        [global::System.Diagnostics.DebuggerNonUserCode]
        public ulong StartSequence
        {
            get { return startSequence_; }
            set
            {
                startSequence_ = value;
            }
        }

        /// <summary>Field number for the "startTimeDelta" field.</summary>
        public const int StartTimeDeltaFieldNumber = 12;
        private long startTimeDelta_;
        /// <summary>
        /// Optional start time
        /// </summary>
        [global::System.Diagnostics.DebuggerNonUserCode]
        public long StartTimeDelta
        {
            get { return startTimeDelta_; }
            set
            {
                startTimeDelta_ = value;
            }
        }

        [global::System.Diagnostics.DebuggerNonUserCode]
        public override bool Equals(object other)
        {
            return Equals(other as SubscriptionRequest);
        }

        [global::System.Diagnostics.DebuggerNonUserCode]
        public bool Equals(SubscriptionRequest other)
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
            if (Subject != other.Subject) return false;
            if (QGroup != other.QGroup) return false;
            if (Inbox != other.Inbox) return false;
            if (MaxInFlight != other.MaxInFlight) return false;
            if (AckWaitInSecs != other.AckWaitInSecs) return false;
            if (DurableName != other.DurableName) return false;
            if (StartPosition != other.StartPosition) return false;
            if (StartSequence != other.StartSequence) return false;
            if (StartTimeDelta != other.StartTimeDelta) return false;
            return true;
        }

        [global::System.Diagnostics.DebuggerNonUserCode]
        public override int GetHashCode()
        {
            int hash = 1;
            if (ClientID.Length != 0) hash ^= ClientID.GetHashCode();
            if (Subject.Length != 0) hash ^= Subject.GetHashCode();
            if (QGroup.Length != 0) hash ^= QGroup.GetHashCode();
            if (Inbox.Length != 0) hash ^= Inbox.GetHashCode();
            if (MaxInFlight != 0) hash ^= MaxInFlight.GetHashCode();
            if (AckWaitInSecs != 0) hash ^= AckWaitInSecs.GetHashCode();
            if (DurableName.Length != 0) hash ^= DurableName.GetHashCode();
            if (StartPosition != 0) hash ^= StartPosition.GetHashCode();
            if (StartSequence != 0UL) hash ^= StartSequence.GetHashCode();
            if (StartTimeDelta != 0L) hash ^= StartTimeDelta.GetHashCode();
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
            if (Subject.Length != 0)
            {
                output.WriteRawTag(18);
                output.WriteString(Subject);
            }
            if (QGroup.Length != 0)
            {
                output.WriteRawTag(26);
                output.WriteString(QGroup);
            }
            if (Inbox.Length != 0)
            {
                output.WriteRawTag(34);
                output.WriteString(Inbox);
            }
            if (MaxInFlight != 0)
            {
                output.WriteRawTag(40);
                output.WriteInt32(MaxInFlight);
            }
            if (AckWaitInSecs != 0)
            {
                output.WriteRawTag(48);
                output.WriteInt32(AckWaitInSecs);
            }
            if (DurableName.Length != 0)
            {
                output.WriteRawTag(58);
                output.WriteString(DurableName);
            }
            if (StartPosition != 0)
            {
                output.WriteRawTag(80);
                output.WriteEnum((int)StartPosition);
            }
            if (StartSequence != 0UL)
            {
                output.WriteRawTag(88);
                output.WriteUInt64(StartSequence);
            }
            if (StartTimeDelta != 0L)
            {
                output.WriteRawTag(96);
                output.WriteInt64(StartTimeDelta);
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
            if (Subject.Length != 0)
            {
                size += 1 + pb::CodedOutputStream.ComputeStringSize(Subject);
            }
            if (QGroup.Length != 0)
            {
                size += 1 + pb::CodedOutputStream.ComputeStringSize(QGroup);
            }
            if (Inbox.Length != 0)
            {
                size += 1 + pb::CodedOutputStream.ComputeStringSize(Inbox);
            }
            if (MaxInFlight != 0)
            {
                size += 1 + pb::CodedOutputStream.ComputeInt32Size(MaxInFlight);
            }
            if (AckWaitInSecs != 0)
            {
                size += 1 + pb::CodedOutputStream.ComputeInt32Size(AckWaitInSecs);
            }
            if (DurableName.Length != 0)
            {
                size += 1 + pb::CodedOutputStream.ComputeStringSize(DurableName);
            }
            if (StartPosition != 0)
            {
                size += 1 + pb::CodedOutputStream.ComputeEnumSize((int)StartPosition);
            }
            if (StartSequence != 0UL)
            {
                size += 1 + pb::CodedOutputStream.ComputeUInt64Size(StartSequence);
            }
            if (StartTimeDelta != 0L)
            {
                size += 1 + pb::CodedOutputStream.ComputeInt64Size(StartTimeDelta);
            }
            return size;
        }

        [global::System.Diagnostics.DebuggerNonUserCode]
        public void MergeFrom(SubscriptionRequest other)
        {
            if (other == null)
            {
                return;
            }
            if (other.ClientID.Length != 0)
            {
                ClientID = other.ClientID;
            }
            if (other.Subject.Length != 0)
            {
                Subject = other.Subject;
            }
            if (other.QGroup.Length != 0)
            {
                QGroup = other.QGroup;
            }
            if (other.Inbox.Length != 0)
            {
                Inbox = other.Inbox;
            }
            if (other.MaxInFlight != 0)
            {
                MaxInFlight = other.MaxInFlight;
            }
            if (other.AckWaitInSecs != 0)
            {
                AckWaitInSecs = other.AckWaitInSecs;
            }
            if (other.DurableName.Length != 0)
            {
                DurableName = other.DurableName;
            }
            if (other.StartPosition != 0)
            {
                StartPosition = other.StartPosition;
            }
            if (other.StartSequence != 0UL)
            {
                StartSequence = other.StartSequence;
            }
            if (other.StartTimeDelta != 0L)
            {
                StartTimeDelta = other.StartTimeDelta;
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
                            Subject = input.ReadString();
                            break;
                        }
                    case 26:
                        {
                            QGroup = input.ReadString();
                            break;
                        }
                    case 34:
                        {
                            Inbox = input.ReadString();
                            break;
                        }
                    case 40:
                        {
                            MaxInFlight = input.ReadInt32();
                            break;
                        }
                    case 48:
                        {
                            AckWaitInSecs = input.ReadInt32();
                            break;
                        }
                    case 58:
                        {
                            DurableName = input.ReadString();
                            break;
                        }
                    case 80:
                        {
                            StartPosition = (StartPosition)input.ReadEnum();
                            break;
                        }
                    case 88:
                        {
                            StartSequence = input.ReadUInt64();
                            break;
                        }
                    case 96:
                        {
                            StartTimeDelta = input.ReadInt64();
                            break;
                        }
                }
            }
        }

    }
}
