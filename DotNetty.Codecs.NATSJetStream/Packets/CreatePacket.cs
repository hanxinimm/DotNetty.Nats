using DotNetty.Buffers;
using DotNetty.Codecs.NATSJetStream.Protocol;
using DotNetty.Codecs.Protocol;
using DotNetty.Common;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace DotNetty.Codecs.NATSJetStream.Packets
{
    [DataContract]
    public class CreatePacket : MessagePacket<NATSJetStreamConfig>
    {
        public CreatePacket(string inboxId, string subject, NATSJetStreamConfig jetStreamConfig)
        {
            Subject = $"{JetStreamProtocolSignatures.JSAPI_STREAM_CREATE}.{jetStreamConfig.Name}";
            ReplyTo = $"{NATSJetStreamInboxs.CreateResponse}{inboxId}.{Guid.NewGuid():N}";
            Message = jetStreamConfig;
        }

        public override NATSJetStreamPacketType PacketType => NATSJetStreamPacketType.Create;
    }
}
