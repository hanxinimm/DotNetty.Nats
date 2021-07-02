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
            Subject = string.IsNullOrEmpty(durableName) ? $"{ProtocolSignatures.JSAPI_CONSUMER_CREATE}.{subject}" : $"{ProtocolSignatures.JSAPI_DURABLE_CREATE}.{subject}.{durableName}";
            ReplyTo = $"{NATSJetStreamInboxs.ConsumerCreateResponse}{inboxId}.{Guid.NewGuid():N}";
            Payload = payload;
        }

        public override NATSPacketType PacketType => NATSPacketType.CONSUMER_CREATE;
    }
}
