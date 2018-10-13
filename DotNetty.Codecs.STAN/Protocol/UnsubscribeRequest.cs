using System;
using System.Collections.Generic;
using System.Text;
using pb = global::Google.Protobuf;
using pbr = global::Google.Protobuf.Reflection;

namespace DotNetty.Codecs.STAN.Protocol
{
    /// <summary>
    /// Protocol for a clients to unsubscribe. Will return a SubscriptionResponse
    /// </summary>
    public sealed partial class UnsubscribeRequest : pb::IMessage<UnsubscribeRequest>
    {
        [global::System.Diagnostics.DebuggerNonUserCode]
        public static pb::MessageParser<UnsubscribeRequest> Parser { get; } = new pb::MessageParser<UnsubscribeRequest>(() => new UnsubscribeRequest());

        [global::System.Diagnostics.DebuggerNonUserCode]
        public static pbr::MessageDescriptor Descriptor
        {
            get { return ProtocolReflection.Descriptor.MessageTypes[8]; }
        }

        [global::System.Diagnostics.DebuggerNonUserCode]
        pbr::MessageDescriptor pb::IMessage.Descriptor
        {
            get { return Descriptor; }
        }

        [global::System.Diagnostics.DebuggerNonUserCode]
        public UnsubscribeRequest()
        {
            OnConstruction();
        }

        partial void OnConstruction();

        [global::System.Diagnostics.DebuggerNonUserCode]
        public UnsubscribeRequest(UnsubscribeRequest other) : this()
        {
            clientID_ = other.clientID_;
            subject_ = other.subject_;
            inbox_ = other.inbox_;
            durableName_ = other.durableName_;
        }

        [global::System.Diagnostics.DebuggerNonUserCode]
        public UnsubscribeRequest Clone()
        {
            return new UnsubscribeRequest(this);
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
        /// subject for the subscription
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

        /// <summary>Field number for the "inbox" field.</summary>
        public const int InboxFieldNumber = 3;
        private string inbox_ = string.Empty;
        /// <summary>
        /// Inbox subject to identify subscription
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

        /// <summary>Field number for the "durableName" field.</summary>
        public const int DurableNameFieldNumber = 4;
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

        [global::System.Diagnostics.DebuggerNonUserCode]
        public override bool Equals(object other)
        {
            return Equals(other as UnsubscribeRequest);
        }

        [global::System.Diagnostics.DebuggerNonUserCode]
        public bool Equals(UnsubscribeRequest other)
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
            if (Inbox != other.Inbox) return false;
            if (DurableName != other.DurableName) return false;
            return true;
        }

        [global::System.Diagnostics.DebuggerNonUserCode]
        public override int GetHashCode()
        {
            int hash = 1;
            if (ClientID.Length != 0) hash ^= ClientID.GetHashCode();
            if (Subject.Length != 0) hash ^= Subject.GetHashCode();
            if (Inbox.Length != 0) hash ^= Inbox.GetHashCode();
            if (DurableName.Length != 0) hash ^= DurableName.GetHashCode();
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
            if (Inbox.Length != 0)
            {
                output.WriteRawTag(26);
                output.WriteString(Inbox);
            }
            if (DurableName.Length != 0)
            {
                output.WriteRawTag(34);
                output.WriteString(DurableName);
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
            if (Inbox.Length != 0)
            {
                size += 1 + pb::CodedOutputStream.ComputeStringSize(Inbox);
            }
            if (DurableName.Length != 0)
            {
                size += 1 + pb::CodedOutputStream.ComputeStringSize(DurableName);
            }
            return size;
        }

        [global::System.Diagnostics.DebuggerNonUserCode]
        public void MergeFrom(UnsubscribeRequest other)
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
            if (other.Inbox.Length != 0)
            {
                Inbox = other.Inbox;
            }
            if (other.DurableName.Length != 0)
            {
                DurableName = other.DurableName;
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
                            Inbox = input.ReadString();
                            break;
                        }
                    case 34:
                        {
                            DurableName = input.ReadString();
                            break;
                        }
                }
            }
        }

    }

}
