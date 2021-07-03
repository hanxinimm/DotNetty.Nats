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
    public class ConsumerPullMessagePacket : PublishPacket
    {
        public ConsumerPullMessagePacket(string inboxId, string subject, string durableName, byte[] payload)
        {
            Subject = $"{ProtocolSignatures.JSAPI_CONSUMER_MSG_NEXT}.{subject}.{durableName}";
            ReplyTo = $"{NATSJetStreamInboxs.ConsumerPullMessageResponse}{inboxId}.{Guid.NewGuid():N}";
            Payload = payload;
        }

        public override NATSPacketType PacketType => NATSPacketType.CONSUMER_PULL_MSG;
    }
}
