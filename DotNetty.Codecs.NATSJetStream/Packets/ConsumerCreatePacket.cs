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
    public class ConsumerCreatePacket : PublishPacket
    {
        public ConsumerCreatePacket(string inboxId, string subject, string durableName, byte[] payload)
        {
            Subject = string.IsNullOrEmpty(durableName) ? $"{NATSJetStreamSignatures.JSAPI_CONSUMER_CREATE}.{subject}" : $"{NATSJetStreamSignatures.JSAPI_DURABLE_CREATE}.{subject}.{durableName}";
            ReplyTo = $"{NATSJetStreamInboxs.ConsumerCreateResponse}{inboxId}.{Guid.NewGuid():N}";
            Payload = payload;
        }

        public override NATSPacketType PacketType => NATSPacketType.CONSUMER_CREATE;
    }
}
