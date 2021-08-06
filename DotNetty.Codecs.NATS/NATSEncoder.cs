// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace DotNetty.Codecs.NATS
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using DotNetty.Buffers;
    using DotNetty.Codecs.NATS.Packets;
    using DotNetty.Common;
    using DotNetty.Common.Utilities;
    using DotNetty.Transport.Channels;

    public class NATSEncoder : MessageToMessageEncoder<NATSPacket>
    {
        public static NATSEncoder Instance => new NATSEncoder();

        public static readonly byte[] EMPTY_BYTES;
        public static readonly byte[] SPACES_BYTES;
        public static readonly byte[] CRLF_BYTES;

        public static readonly byte[] HEADER_VERSION_BYTES;

        public static readonly byte[] CONNECT_BYTES;
        public static readonly byte[] PUB_BYTES;
        public static readonly byte[] HPUB_BYTES;
        public static readonly byte[] SUB_BYTES;
        public static readonly byte[] UNSUB_BYTES;
        public static readonly byte[] PING_BYTES;
        public static readonly byte[] PONG_BYTES;

        public static readonly byte[] ACK_ACK_BYTES;
	    public static readonly byte[] ACK_NAK_BYTES;
        public static readonly byte[] ACK_PROGRESS_BYTES;
        public static readonly byte[] ACK_NEXT_BYTES;
        public static readonly byte[] ACK_TERM_BYTES;

        static NATSEncoder()
        {
            EMPTY_BYTES = Array.Empty<byte>();
            SPACES_BYTES = Encoding.UTF8.GetBytes(NATSSignatures.SPACES);
            CRLF_BYTES = Encoding.UTF8.GetBytes(NATSSignatures.CRLF);

            HEADER_VERSION_BYTES = Encoding.UTF8.GetBytes(NATSSignatures.HEADER_VERSION);

            CONNECT_BYTES = Encoding.UTF8.GetBytes(NATSSignatures.CONNECT);
            PUB_BYTES = Encoding.UTF8.GetBytes(NATSSignatures.PUB);
            HPUB_BYTES = Encoding.UTF8.GetBytes(NATSSignatures.HPUB);
            SUB_BYTES = Encoding.UTF8.GetBytes(NATSSignatures.SUB);
            UNSUB_BYTES = Encoding.UTF8.GetBytes(NATSSignatures.UNSUB);
            PING_BYTES = Encoding.UTF8.GetBytes(NATSSignatures.PING);
            PONG_BYTES = Encoding.UTF8.GetBytes(NATSSignatures.PONG);

            ACK_ACK_BYTES = Encoding.UTF8.GetBytes(NATSSignatures.AckAck);
            ACK_NAK_BYTES = Encoding.UTF8.GetBytes(NATSSignatures.AckNak);
            ACK_PROGRESS_BYTES = Encoding.UTF8.GetBytes(NATSSignatures.AckProgress);
            ACK_NEXT_BYTES = Encoding.UTF8.GetBytes(NATSSignatures.AckNext);
            ACK_TERM_BYTES = Encoding.UTF8.GetBytes(NATSSignatures.AckTerm);
        }

        public override bool IsSharable => true;


        protected override void Encode(IChannelHandlerContext context, NATSPacket packet, List<object> output)
        {
            if (!DoEncode(context.Allocator, packet, output))
                throw new ArgumentException("Unknown packet type: " + packet.PacketType, nameof(packet));
        }

        protected virtual bool DoHighFrequencyEncode(IByteBufferAllocator bufferAllocator, NATSPacket packet, List<object> output)
        {
            switch (packet.PacketType)
            {
                case NATSPacketType.PUB:
                    EncodePublishMessage(bufferAllocator, (PublishPacket)packet, output);
                    break;
                case NATSPacketType.PING:
                    EncodePingMessage(bufferAllocator, (PingPacket)packet, output);
                    break;
                case NATSPacketType.PONG:
                    EncodePongMessage(bufferAllocator, (PongPacket)packet, output);
                    break;
                default:
                    return false;
            }
            return true;
        }

        protected virtual bool DoEncode(IByteBufferAllocator bufferAllocator, NATSPacket packet, List<object> output)
        {
            if (DoHighFrequencyEncode(bufferAllocator, packet, output)) return true;

            switch (packet.PacketType)
            {
                case NATSPacketType.CONNECT:
                    EncodeConnectMessage(bufferAllocator, (ConnectPacket)packet, output);
                    break;
                case NATSPacketType.SUB:
                    EncodeSubscribeMessage(bufferAllocator, (SubscribePacket)packet, output);
                    break;
                case NATSPacketType.UNSUB:
                    EncodeUnsubscribeMessage(bufferAllocator, (UnSubscribePacket)packet, output);
                    break;
                default:
                    return false;
            }
            return true;
        }

        static void EncodeConnectMessage(IByteBufferAllocator bufferAllocator, ConnectPacket packet, List<object> output)
        {
            byte[] ConnectOptionBytes = EncodeStringInUtf8(packet.Content);
            IByteBuffer buf = null;
            try
            {
                buf = bufferAllocator.Buffer(CONNECT_BYTES.Length + SPACES_BYTES.Length + ConnectOptionBytes.Length + CRLF_BYTES.Length);
                buf.WriteBytes(CONNECT_BYTES);
                buf.WriteBytes(SPACES_BYTES);
                buf.WriteBytes(ConnectOptionBytes);
                buf.WriteBytes(CRLF_BYTES);

                output.Add(buf);
                buf = null;
            }
            finally
            {
                buf?.SafeRelease();
            }
        }

        protected static void EncodePublishMessage(IByteBufferAllocator bufferAllocator, PublishPacket packet, List<object> output)
        {
            byte[] SubjectNameBytes = EncodeStringInUtf8(packet.Subject);
            byte[] ReplyToBytes = EncodeStringInUtf8(packet.ReplyTo);

            int variablePartSize = SubjectNameBytes.Length + SPACES_BYTES.Length;
            variablePartSize += (ReplyToBytes.Length > 0 ? ReplyToBytes.Length + SPACES_BYTES.Length : 0);

            byte[] PayloadSize = EncodeStringInUtf8(packet.PayloadLength.ToString());

            variablePartSize += PayloadSize.Length + CRLF_BYTES.Length;
            variablePartSize += packet.PayloadLength + CRLF_BYTES.Length;

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
                if (packet.Payload != null)
                {
                    buf.WriteBytes(packet.Payload);
                }
                buf.WriteBytes(CRLF_BYTES);

                output.Add(buf);
                buf = null;
            }
            finally
            {
                buf?.SafeRelease();
            }
        }


        protected static void EncodeSubscribeMessage(IByteBufferAllocator bufferAllocator, SubscribePacket packet, List<object> output)
        {
            byte[] IdBytes = EncodeStringInUtf8(packet.Id);
            byte[] SubjectNameBytes = EncodeStringInUtf8(packet.Subject);
            byte[] GroupBytes = EncodeStringInUtf8(packet.Group);

            int variablePartSize = IdBytes.Length + CRLF_BYTES.Length;
            variablePartSize += SubjectNameBytes.Length + SPACES_BYTES.Length;

            if (GroupBytes.Length > 0)
            {
                variablePartSize += GroupBytes.Length + SPACES_BYTES.Length;
            }

            int fixedHeaderBufferSize = SUB_BYTES.Length + SPACES_BYTES.Length;

            IByteBuffer buf = null;
            try
            {
                buf = bufferAllocator.Buffer(fixedHeaderBufferSize + variablePartSize);
                buf.WriteBytes(SUB_BYTES);
                buf.WriteBytes(SPACES_BYTES);
                buf.WriteBytes(SubjectNameBytes);
                buf.WriteBytes(SPACES_BYTES);
                if (GroupBytes.Length > 0)
                {
                    buf.WriteBytes(GroupBytes);
                    buf.WriteBytes(SPACES_BYTES);
                }
                buf.WriteBytes(IdBytes);
                buf.WriteBytes(CRLF_BYTES);

                output.Add(buf);
                buf = null;
            }
            finally
            {
                buf?.SafeRelease();
            }
        }

        protected static void EncodeUnsubscribeMessage(IByteBufferAllocator bufferAllocator, UnSubscribePacket packet, List<object> output)
        {
            byte[] IdBytes = EncodeStringInUtf8(packet.Id);
            byte[] WaitMessagesBytes = EncodeStringInUtf8(packet.WaitMessages?.ToString() ?? string.Empty);

            int variablePartSize = IdBytes.Length + CRLF_BYTES.Length;

            if (WaitMessagesBytes.Length > 0)
            {
                variablePartSize += SPACES_BYTES.Length + WaitMessagesBytes.Length;
            }

            int fixedHeaderBufferSize = UNSUB_BYTES.Length + SPACES_BYTES.Length;

            IByteBuffer buf = null;
            try
            {
                buf = bufferAllocator.Buffer(fixedHeaderBufferSize + variablePartSize);
                buf.WriteBytes(UNSUB_BYTES);
                buf.WriteBytes(SPACES_BYTES);
                buf.WriteBytes(IdBytes);
                if (WaitMessagesBytes.Length > 0)
                {
                    buf.WriteBytes(SPACES_BYTES);
                    buf.WriteBytes(WaitMessagesBytes);
                }
                buf.WriteBytes(CRLF_BYTES);

                output.Add(buf);
                buf = null;
            }
            finally
            {
                buf?.SafeRelease();
            }
        }

        static void EncodePingMessage(IByteBufferAllocator bufferAllocator, PingPacket packet, List<object> output)
        {
            IByteBuffer buf = null;
            try
            {
                buf = bufferAllocator.Buffer(PING_BYTES.Length + CRLF_BYTES.Length);
                buf.WriteBytes(PING_BYTES);
                buf.WriteBytes(CRLF_BYTES);

                output.Add(buf);
                buf = null;
            }
            finally
            {
                buf?.SafeRelease();
            }
        }

        static void EncodePongMessage(IByteBufferAllocator bufferAllocator, PongPacket packet, List<object> output)
        {
            IByteBuffer buf = null;
            try
            {
                buf = bufferAllocator.Buffer(PONG_BYTES.Length + CRLF_BYTES.Length);
                buf.WriteBytes(PONG_BYTES);
                buf.WriteBytes(CRLF_BYTES);

                output.Add(buf);
                buf = null;
            }
            finally
            {
                buf?.SafeRelease();
            }
        }

        protected static byte[] EncodeStringInUtf8(string s)
        {
            if (string.IsNullOrEmpty(s)) return EMPTY_BYTES;
            return Encoding.UTF8.GetBytes(s);
        }
    }
}