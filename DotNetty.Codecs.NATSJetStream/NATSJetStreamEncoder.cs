// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace DotNetty.Codecs.NATS
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using DotNetty.Buffers;
    using DotNetty.Codecs;
    using DotNetty.Codecs.NATSJetStream.Packets;
    using DotNetty.Codecs.Protocol;
    using DotNetty.Common;
    using DotNetty.Common.Utilities;
    using DotNetty.Transport.Channels;
    using Newtonsoft.Json;

    public sealed class NATSJetStreamEncoder : MessageToMessageEncoder<NATSJetStreamPacket>
    {
        public static NATSJetStreamEncoder Instance => new NATSJetStreamEncoder();

        public static readonly byte[] EMPTY_BYTES;
        public static readonly byte[] SPACES_BYTES;
        public static readonly byte[] CRLF_BYTES;

        public static readonly byte[] STREAM_CREATE_BYTES;
        public static readonly byte[] PUB_BYTES;
        public static readonly byte[] SUB_BYTES;
        public static readonly byte[] UNSUB_BYTES;
        public static readonly byte[] PING_BYTES;
        public static readonly byte[] PONG_BYTES;

        static NATSJetStreamEncoder()
        {
            EMPTY_BYTES = new byte[0];
            SPACES_BYTES = Encoding.UTF8.GetBytes(ProtocolSignatures.SPACES);
            CRLF_BYTES = Encoding.UTF8.GetBytes(ProtocolSignatures.CRLF);

            STREAM_CREATE_BYTES = Encoding.UTF8.GetBytes(JetStreamProtocolSignatures.JSAPI_STREAM_CREATE);
            PUB_BYTES = Encoding.UTF8.GetBytes(ProtocolSignatures.PUB);
            SUB_BYTES = Encoding.UTF8.GetBytes(ProtocolSignatures.SUB);
            UNSUB_BYTES = Encoding.UTF8.GetBytes(ProtocolSignatures.UNSUB);
            PING_BYTES = Encoding.UTF8.GetBytes(ProtocolSignatures.PING);
            PONG_BYTES = Encoding.UTF8.GetBytes(ProtocolSignatures.PONG);
        }

        public override bool IsSharable => true;


        protected override void Encode(IChannelHandlerContext context, NATSJetStreamPacket message, List<object> output) => DoEncode(context.Allocator, message, output);


        internal static void DoEncode(IByteBufferAllocator bufferAllocator, NATSJetStreamPacket packet, List<object> output)
        {
            switch (packet.PacketType)
            {
                case NATSJetStreamPacketType.Create:
                    EncodeCreateMessage(bufferAllocator, (CreatePacket)packet, output);
                    break;
                default:
                    throw new ArgumentException("Unknown packet type: " + packet.PacketType, nameof(packet));
            }
        }

        static void EncodeCreateMessage<TMessage>(IByteBufferAllocator bufferAllocator, MessagePacket<TMessage> packet, List<object> output)
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


        static byte[] EncodeStringInUtf8(string s)
        {
            if (string.IsNullOrEmpty(s)) return EMPTY_BYTES;
            return Encoding.UTF8.GetBytes(s);
        }
    }
}