// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace DotNetty.Codecs.STAN
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using DotNetty.Buffers;
    using DotNetty.Codecs.NATS;
    using DotNetty.Codecs.STAN.Packets;
    using DotNetty.Codecs.STAN.Protocol;
    using DotNetty.Common;
    using DotNetty.Common.Utilities;
    using DotNetty.Transport.Channels;
    using Google.Protobuf;

    public sealed class STANEncoder : MessageToMessageEncoder<STANPacket>
    {
        public static readonly STANEncoder Instance = new STANEncoder();

        public static readonly byte[] EMPTY_BYTES;
        public static readonly byte[] SPACES_BYTES;
        public static readonly byte[] CRLF_BYTES;

        public static readonly byte[] CONNECT_BYTES;
        public static readonly byte[] PUB_BYTES;
        public static readonly byte[] SUB_BYTES;
        public static readonly byte[] UNSUB_BYTES;
        public static readonly byte[] PING_BYTES;
        public static readonly byte[] PONG_BYTES;

        static STANEncoder()
        {
            EMPTY_BYTES = new byte[0];
            SPACES_BYTES = Encoding.UTF8.GetBytes(ProtocolSignatures.SPACES);
            CRLF_BYTES = Encoding.UTF8.GetBytes(ProtocolSignatures.CRLF);

            CONNECT_BYTES = Encoding.UTF8.GetBytes(ProtocolSignatures.CONNECT);
            PUB_BYTES = Encoding.UTF8.GetBytes(ProtocolSignatures.PUB);
            SUB_BYTES = Encoding.UTF8.GetBytes(ProtocolSignatures.SUB);
            UNSUB_BYTES = Encoding.UTF8.GetBytes(ProtocolSignatures.UNSUB);
            PING_BYTES = Encoding.UTF8.GetBytes(ProtocolSignatures.PING);
            PONG_BYTES = Encoding.UTF8.GetBytes(ProtocolSignatures.PONG);
        }

        protected override void Encode(IChannelHandlerContext context, STANPacket message, List<object> output) => DoEncode(context.Allocator, message, output);

        public override bool IsSharable => true;

        internal static void DoEncode(IByteBufferAllocator bufferAllocator, STANPacket packet, List<object> output)
        {
            switch (packet.PacketType)
            {
                case STANPacketType.PubMsg:
                    EncodePublishMessage(bufferAllocator, (PubMsgPacket)packet, output);
                    break;
                case STANPacketType.MultiplePubMsg:
                    EncodePublishMessages(bufferAllocator, (PubMultipleMsgPacket)packet, output);
                    break;
                case STANPacketType.Ack:
                    EncodePublishMessage(bufferAllocator, (AckPacket)packet, output);
                    break;
                case STANPacketType.HeartbeatInbox:
                    EncodeSubscribeMessage(bufferAllocator, (HeartbeatInboxPacket)packet, output);
                    break;
                case STANPacketType.HeartbeatAck:
                    EncodeHeartbeatAck(bufferAllocator, (HeartbeatAckPacket)packet, output);
                    break;
                case STANPacketType.SubscriptionRequest:
                    EncodePublishMessage(bufferAllocator, (SubscriptionRequestPacket)packet, output);
                    break;
                case STANPacketType.UnsubscribeRequest:
                    EncodePublishMessage(bufferAllocator, (UnsubscribeRequestPacket)packet, output);
                    break;
                case STANPacketType.SUB:
                    EncodeSubscribeMessage(bufferAllocator, (SubscribePacket)packet, output);
                    break;
                case STANPacketType.INBOX:
                    EncodeSubscribeMessage(bufferAllocator, (InboxPacket)packet, output);
                    break;
                case STANPacketType.ConnectRequest:
                    EncodePublishMessage(bufferAllocator, (ConnectRequestPacket)packet, output);
                    break;
                case STANPacketType.CloseRequest:
                    EncodePublishMessage(bufferAllocator, (CloseRequestPacket)packet, output);
                    break;
                case STANPacketType.PING:
                    EncodePingMessage(bufferAllocator, (PingPacket)packet, output);
                    break;
                case STANPacketType.PONG:
                    EncodePongMessage(bufferAllocator, (PongPacket)packet, output);
                    break;
                default:
                    throw new ArgumentException("Unknown packet type: " + packet.PacketType, nameof(packet));
            }
        }

        static void EncodeHeartbeatAck(IByteBufferAllocator bufferAllocator, HeartbeatAckPacket packet, List<object> output)
        {
            byte[] SubjectNameBytes = EncodeStringInUtf8(packet.Subject);

            int variablePartSize = SubjectNameBytes.Length + SPACES_BYTES.Length;

            byte[] PayloadSize = EncodeStringInUtf8("0");

            variablePartSize += PayloadSize.Length + CRLF_BYTES.Length;
            variablePartSize += CRLF_BYTES.Length;

            int fixedHeaderBufferSize = PUB_BYTES.Length + SPACES_BYTES.Length;

            IByteBuffer buf = null;
            try
            {
                buf = bufferAllocator.Buffer(fixedHeaderBufferSize + variablePartSize);
                buf.WriteBytes(PUB_BYTES);
                buf.WriteBytes(SPACES_BYTES);
                buf.WriteBytes(SubjectNameBytes);
                buf.WriteBytes(SPACES_BYTES);

                buf.WriteBytes(PayloadSize);
                buf.WriteBytes(CRLF_BYTES);

                buf.WriteBytes(CRLF_BYTES);

                output.Add(buf);
                buf = null;
            }
            finally
            {
                buf?.SafeRelease();
            }
        }

        static void EncodePublishMessage<TMessage>(IByteBufferAllocator bufferAllocator, MessagePacket<TMessage> packet, List<object> output)
            where TMessage : IMessage
        {
            byte[] SubjectNameBytes = EncodeStringInUtf8(packet.Subject);
            byte[] ReplyToBytes = EncodeStringInUtf8(packet.ReplyTo);

            int variablePartSize = SubjectNameBytes.Length + SPACES_BYTES.Length;
            variablePartSize += (ReplyToBytes.Length > 0 ? ReplyToBytes.Length + SPACES_BYTES.Length : 0);

            IByteBuffer Payload = Unpooled.WrappedBuffer(packet.Message.ToByteArray());

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

        static void EncodePublishMessages(IByteBufferAllocator bufferAllocator, PubMultipleMsgPacket packet, List<object> output)
        {
            foreach (var msgPacket in packet.MessagePackets)
            {
                EncodePublishMessage(bufferAllocator, msgPacket, output);
            }
        }

        static void EncodeSubscribeMessage(IByteBufferAllocator bufferAllocator, STANSubscribePacket packet, List<object> output)
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


        static byte[] EncodeStringInUtf8(string s)
        {
            if (string.IsNullOrEmpty(s)) return EMPTY_BYTES;
            return Encoding.UTF8.GetBytes(s);
        }
    }
}