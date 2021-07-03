using DotNetty.Buffers;
using DotNetty.Codecs.NATS.Packets;
using DotNetty.Codecs.NATSJetStream.Protocol;
using DotNetty.Common;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace DotNetty.Codecs.NATSJetStream.Packets
{
    [DataContract]
    public class LeaderStepDownPacket : DirectivePacket
    {
        public LeaderStepDownPacket(string inboxId, string subject)
        {
            Subject = $"{ProtocolSignatures.JSAPI_STREAM_LEADER_STEPDOWN}.{subject}";
            ReplyTo = $"{NATSJetStreamInboxs.LeaderStepDownResponse}{inboxId}.{Guid.NewGuid():N}";
        }

        public override NATSPacketType PacketType => NATSPacketType.STREAM_LEADER_STEPDOWN;
    }
}
