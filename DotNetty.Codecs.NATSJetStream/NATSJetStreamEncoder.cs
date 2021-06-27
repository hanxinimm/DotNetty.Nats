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

        public static readonly byte[] STREAM_CREATE_BYTES;


        static NATSJetStreamEncoder()
        {
            STREAM_CREATE_BYTES = Encoding.UTF8.GetBytes(Protocol.ProtocolSignatures.JSAPI_STREAM_CREATE);
        }

        protected override bool DoEncode(IByteBufferAllocator bufferAllocator, NATSPacket packet, List<object> output)
        {
            switch (packet.PacketType)
            {
                case NATSPacketType.CREATE:
                    EncodeJsonMessage(bufferAllocator, (CreatePacket)packet, output);
                    break;
                default:
                    return false;
            }
            return true;
        }

        static void EncodeJsonMessage<TMessage>(IByteBufferAllocator bufferAllocator, MessagePacket<TMessage> packet, List<object> output)
        {
            byte[] SubjectNameBytes = EncodeStringInUtf8(packet.Subject);
            byte[] ReplyToBytes = EncodeStringInUtf8(packet.ReplyTo);

            int variablePartSize = SubjectNameBytes.Length + SPACES_BYTES.Length;
            variablePartSize += (ReplyToBytes.Length > 0 ? ReplyToBytes.Length + SPACES_BYTES.Length : 0);

            var MessageJson = JsonConvert.SerializeObject(packet.Message);

            IByteBuffer Payload = Unpooled.WrappedBuffer(Encoding.UTF8.GetBytes(MessageJson));

            byte[] PayloadSize = EncodeStringInUtf8(Payload.ReadableBytes.ToString());

            variablePartSize += PayloadSize.Length + CRLF_BYTES.Length;
            variablePartSize += Payload.ReadableBytes + CRLF_BYTES.Length;

            int fixedHeaderBufferSize = PUB_BYTES.Length + SPACES_BYTES.Length;

            IByteBuffer buf = null;
            try
            {
                buf = bufferAllocator.Buffer(fixedHeaderBufferSize + variablePartSize);
                buf.WriteBytes(PUB_BYTES);
                buf.WriteBytes(SPACES_BYTES);
                buf.WriteBytes(SubjectNameBytes);
                buf.WriteBytes(SPACES_BYTES);
                if (!string.IsNullOrEmpty(packet.ReplyTo))
                {
                    buf.WriteBytes(ReplyToBytes);
                    buf.WriteBytes(SPACES_BYTES);
                }
                buf.WriteBytes(PayloadSize);
                buf.WriteBytes(CRLF_BYTES);

                buf.WriteBytes(Payload);

                buf.WriteBytes(CRLF_BYTES);

                output.Add(buf);
                buf = null;
            }
            finally
            {
                buf?.SafeRelease();
            }
        }
    }
}