// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace DotNetty.Codecs.NATSJetStream
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using DotNetty.Buffers;
    using DotNetty.Codecs;
    using DotNetty.Codecs.NATS;
    using DotNetty.Codecs.NATS.Packets;
    using DotNetty.Codecs.NATSJetStream.Packets;
    using DotNetty.Codecs.NATSJetStream.Protocol;
    using DotNetty.Common;
    using DotNetty.Common.Utilities;
    using DotNetty.Transport.Channels;
    using Newtonsoft.Json;

    public sealed class NATSJetStreamEncoder : NATSEncoder
    {
        public static new NATSJetStreamEncoder Instance => new NATSJetStreamEncoder();

        protected override bool DoEncode(IByteBufferAllocator bufferAllocator, NATSPacket packet, List<object> output)
        {
            if (!base.DoEncode(bufferAllocator, packet, output))
            {
                switch (packet.PacketType)
                {
                    case NATSPacketType.STREAM_INBOX:
                        EncodeSubscribeMessage(bufferAllocator, (SubscribePacket)packet, output);
                        break;
                    case NATSPacketType.STREAM_CREATE:
                        EncodePublishMessage(bufferAllocator, (CreatePacket)packet, output);
                        break;
                    case NATSPacketType.STREAM_INFO:
                        EncodePublishMessage(bufferAllocator, (Packets.InfoPacket)packet, output);
                        break;
                    default:
                        return false;
                }
            }
            return true;
        }
    }
}