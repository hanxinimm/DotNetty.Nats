// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using DotNetty.Codecs.NATS;

namespace DotNetty.Codecs.NATSJetStream
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using DotNetty.Buffers;
    using DotNetty.Codecs;
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

        public static readonly byte[] COLON_BYTES;

        static NATSJetStreamEncoder()
        {
            COLON_BYTES = Encoding.UTF8.GetBytes(NATSJetStreamSignatures.COLON);
        }

        protected override bool DoHighFrequencyEncode(IByteBufferAllocator bufferAllocator, NATSPacket packet, List<object> output)
        {
            if (base.DoHighFrequencyEncode(bufferAllocator, packet, output)) return true;

            switch (packet.PacketType)
            {
                case NATSPacketType.HPUB:
                    EncodePublishHigherMessage(bufferAllocator, (PublishHigherPacket)packet, output);
                    break;

                case NATSPacketType.ACK_ACK:
                case NATSPacketType.ACK_NAK:
                case NATSPacketType.ACK_PROGRESS:
                case NATSPacketType.ACK_NEXT:
                case NATSPacketType.ACK_TERM:
                    EncodeAckMessage(bufferAllocator, (AckPacket)packet, output);
                    break;

                default:
                    return false;
            }

            return true;
        }



        protected override bool DoEncode(IByteBufferAllocator bufferAllocator, NATSPacket packet, List<object> output)
        {
            if (!base.DoEncode(bufferAllocator, packet, output)) return true;

            switch (packet.PacketType)
            {
                case NATSPacketType.STREAM_INBOX:
                    EncodeSubscribeMessage(bufferAllocator, (SubscribePacket)packet, output);
                    break;
                case NATSPacketType.STREAM_MSG_GET:
                    EncodePublishMessage(bufferAllocator, (GetMessagePacket)packet, output);
                    break;
                case NATSPacketType.STREAM_CREATE:
                    EncodePublishMessage(bufferAllocator, (CreatePacket)packet, output);
                    break;
                case NATSPacketType.STREAM_UPDATE:
                    EncodePublishMessage(bufferAllocator, (UpdatePacket)packet, output);
                    break;
                case NATSPacketType.STREAM_NAMES:
                    EncodePublishMessage(bufferAllocator, (NamesPacket)packet, output);
                    break;
                case NATSPacketType.STREAM_LIST:
                    EncodePublishMessage(bufferAllocator, (ListPacket)packet, output);
                    break;
                case NATSPacketType.STREAM_DELETE:
                    EncodePublishMessage(bufferAllocator, (DeletePacket)packet, output);
                    break;
                case NATSPacketType.STREAM_PURGE:
                    EncodePublishMessage(bufferAllocator, (PurgePacket)packet, output);
                    break;
                case NATSPacketType.STREAM_SNAPSHOT:
                    EncodePublishMessage(bufferAllocator, (SnapshotPacket)packet, output);
                    break;
                case NATSPacketType.STREAM_MSG_DELETE:
                    EncodePublishMessage(bufferAllocator, (DeleteMessagePacket)packet, output);
                    break;
                case NATSPacketType.STREAM_REMOVE_PEERE:
                    EncodePublishMessage(bufferAllocator, (RemovePeerPacket)packet, output);
                    break;
                case NATSPacketType.STREAM_LEADER_STEPDOWN:
                    EncodePublishMessage(bufferAllocator, (LeaderStepDownPacket)packet, output);
                    break;
                case NATSPacketType.CONSUMER_CREATE:
                    EncodePublishMessage(bufferAllocator, (ConsumerCreatePacket)packet, output);
                    break;
                case NATSPacketType.CONSUMER_NAMES:
                    EncodePublishMessage(bufferAllocator, (ConsumerNamesPacket)packet, output);
                    break;
                case NATSPacketType.CONSUMER_LIST:
                    EncodePublishMessage(bufferAllocator, (ConsumerListPacket)packet, output);
                    break;
                case NATSPacketType.STREAM_INFO:
                    EncodePublishMessage(bufferAllocator, (Packets.InfoPacket)packet, output);
                    break;
                default:
                    return false;
            }

            return true;
        }

        static void EncodePublishHigherMessage(IByteBufferAllocator bufferAllocator, PublishHigherPacket packet, List<object> output)
        {
            byte[] SubjectNameBytes = EncodeStringInUtf8(packet.Subject);
            byte[] ReplyToBytes = EncodeStringInUtf8(packet.ReplyTo);

            int variablePartSize = SubjectNameBytes.Length + SPACES_BYTES.Length;
            variablePartSize += (ReplyToBytes.Length > 0 ? ReplyToBytes.Length + SPACES_BYTES.Length : 0);

            byte[] HeadersBytes = Array.Empty<byte>();
            if (packet.Headers != null && packet.Headers.Count > 0)
            {
                variablePartSize += HEADER_VERSION_BYTES_PLUS_CRLF.Length;
                StringBuilder headersBuilder = new();
                foreach (var packetHeaders in packet.Headers)
                {
                    headersBuilder.Append(packetHeaders.Key);
                    headersBuilder.Append(NATSJetStreamSignatures.COLON);
                    headersBuilder.Append(packetHeaders.Value);
                    headersBuilder.Append(NATSSignatures.CRLF);
                }
                HeadersBytes = EncodeStringInUtf8(headersBuilder.ToString());
                variablePartSize += HeadersBytes.Length;

                variablePartSize += CRLF_BYTES.Length;
            }

            byte[] PayloadSize = EncodeStringInUtf8(packet.PayloadLength.ToString());

            variablePartSize += PayloadSize.Length + CRLF_BYTES.Length;
            variablePartSize += packet.PayloadLength + CRLF_BYTES.Length;

            int fixedHeaderBufferSize = HPUB_BYTES.Length + SPACES_BYTES.Length;

            IByteBuffer buf = null;
            try
            {
                buf = bufferAllocator.Buffer(fixedHeaderBufferSize + variablePartSize);
                buf.WriteBytes(HPUB_BYTES);
                buf.WriteBytes(SPACES_BYTES);
                buf.WriteBytes(SubjectNameBytes);
                buf.WriteBytes(SPACES_BYTES);
                if (!string.IsNullOrEmpty(packet.ReplyTo))
                {
                    buf.WriteBytes(ReplyToBytes);
                    buf.WriteBytes(SPACES_BYTES);
                }
                if (HeadersBytes.Length > 0)
                {
                    buf.WriteBytes(HEADER_VERSION_BYTES_PLUS_CRLF);

                    buf.WriteBytes(HeadersBytes);

                    buf.WriteBytes(CRLF_BYTES);
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


        static void EncodeAckMessage(IByteBufferAllocator bufferAllocator, AckPacket packet, List<object> output)
        {

            byte[] packetPayload = null;

            switch (packet.PacketType)
            {
                case NATSPacketType.ACK_ACK:
                    packetPayload = ACK_ACK_BYTES;
                    break;
                case NATSPacketType.ACK_NAK:
                    packetPayload = ACK_NAK_BYTES;
                    break;
                case NATSPacketType.ACK_PROGRESS:
                    packetPayload = ACK_PROGRESS_BYTES;
                    break;
                case NATSPacketType.ACK_NEXT:
                    packetPayload = ACK_NEXT_BYTES;
                    break;
                case NATSPacketType.ACK_TERM:
                    packetPayload = ACK_TERM_BYTES;
                    break;
                default:
                    packetPayload = ACK_ACK_BYTES;
                    break;
            }


            byte[] SubjectNameBytes = EncodeStringInUtf8(packet.Subject);

            int variablePartSize = SubjectNameBytes.Length + SPACES_BYTES.Length;

            byte[] PayloadSize = EncodeStringInUtf8(packetPayload.Length.ToString());

            variablePartSize += PayloadSize.Length + CRLF_BYTES.Length;
            variablePartSize += packetPayload.Length + CRLF_BYTES.Length;

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
                if (packetPayload != null)
                {
                    buf.WriteBytes(packetPayload);
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



    }
}