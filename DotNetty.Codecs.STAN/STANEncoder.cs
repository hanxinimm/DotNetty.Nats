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
            SPACES_BYTES = Encoding.UTF8.GetBytes(NATSSignatures.SPACES);
            CRLF_BYTES = Encoding.UTF8.GetBytes(NATSSignatures.CRLF);

            CONNECT_BYTES = Encoding.UTF8.GetBytes(NATSSignatures.CONNECT);
            PUB_BYTES = Encoding.UTF8.GetBytes(NATSSignatures.PUB);
            SUB_BYTES = Encoding.UTF8.GetBytes(NATSSignatures.SUB);
            UNSUB_BYTES = Encoding.UTF8.GetBytes(NATSSignatures.UNSUB);
            PING_BYTES = Encoding.UTF8.GetBytes(NATSSignatures.PING);
            PONG_BYTES = Encoding.UTF8.GetBytes(NATSSignatures.PONG);
        }

        protected override void Encode(IChannelHandlerContext context, STANPacket message, List<object> output) => DoEncode(context.Allocator, message, output);

        public override bool IsSharable => true;

        /// <summary>
        ///     This is the main encoding method.
        ///     It's only visible for testing.
        ///     @param bufferAllocator Allocates ByteBuf
        ///     @param packet MQTT packet to encode
        ///     @return ByteBuf with encoded bytes
        /// </summary>
        internal static void DoEncode(IByteBufferAllocator bufferAllocator, STANPacket packet, List<object> output)
        {
            switch (packet.PacketType)
            {
                case STANPacketType.Heartbeat:
                    EncodeSubscribeMessage(bufferAllocator, (HeartbeatInboxPacket)packet, output);
                    break;
                case STANPacketType.ConnectRequest:
                    EncodePublishMessage(bufferAllocator, (ConnectRequestPacket)packet, output);
                    break;
                case STANPacketType.SubscriptionRequest:
                    EncodePublishMessage(bufferAllocator, (SubscriptionRequestPacket)packet, output);
                    break;

                //case PacketType.UNSUB:
                //    EncodeUnsubscribeMessage(bufferAllocator, (UnSubscribePacket)packet, output);
                //    break;
                //case PacketType.PING:
                //    EncodePingMessage(bufferAllocator, (PingPacket)packet, output);
                //    break;
                //case PacketType.PONG:
                //    EncodePongMessage(bufferAllocator, (PongPacket)packet, output);
                //    break;
                //case PacketType.CONNACK:
                //    EncodeConnAckMessage(bufferAllocator, (ConnAckPacket)packet, output);
                //    break;
                //case PacketType.PUBACK:
                //case PacketType.PUBREC:
                //case PacketType.PUBREL:
                //case PacketType.PUBCOMP:
                //case PacketType.UNSUBACK:
                //    EncodePacketWithIdOnly(bufferAllocator, (PacketWithId)packet, output);
                //    break;

                //case PacketType.SUBACK:
                //    EncodeSubAckMessage(bufferAllocator, (SubAckPacket)packet, output);
                //    break;

                //case PacketType.PINGREQ:
                //case PacketType.PINGRESP:
                //case PacketType.DISCONNECT:
                //    EncodePacketWithFixedHeaderOnly(bufferAllocator, packet, output);
                //    break;
                default:
                    throw new ArgumentException("Unknown packet type: " + packet.PacketType, nameof(packet));
            }
        }

        static void EncodePublishMessage<TMessage>(IByteBufferAllocator bufferAllocator, STANPublishPacket<TMessage> packet, List<object> output)
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



        static void EncodeSubscribeMessage(IByteBufferAllocator bufferAllocator, STANSubscribePacket packet, List<object> output)
        {
            byte[] IdBytes = EncodeStringInUtf8(packet.Id);
            byte[] SubjectNameBytes = EncodeStringInUtf8(packet.Subject);
            byte[] GroupBytes = EncodeStringInUtf8(packet.Group);

            int variablePartSize = IdBytes.Length + SPACES_BYTES.Length;
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

        //static void EncodeUnsubscribeMessage(IByteBufferAllocator bufferAllocator, UnSubscribePacket packet, List<object> output)
        //{
        //    byte[] IdBytes = EncodeStringInUtf8(packet.Id);
        //    byte[] WaitMessagesBytes = EncodeStringInUtf8(packet.WaitMessages?.ToString() ?? string.Empty);

        //    int variablePartSize = IdBytes.Length + SPACES_BYTES.Length;

        //    if (WaitMessagesBytes.Length > 0)
        //    {
        //        variablePartSize += WaitMessagesBytes.Length + SPACES_BYTES.Length;
        //    }

        //    int fixedHeaderBufferSize = UNSUB_BYTES.Length + SPACES_BYTES.Length;

        //    IByteBuffer buf = null;
        //    try
        //    {
        //        buf = bufferAllocator.Buffer(fixedHeaderBufferSize + variablePartSize);
        //        buf.WriteBytes(UNSUB_BYTES);
        //        buf.WriteBytes(SPACES_BYTES);
        //        buf.WriteBytes(IdBytes);
        //        if (WaitMessagesBytes.Length > 0)
        //        {
        //            buf.WriteBytes(SPACES_BYTES);
        //            buf.WriteBytes(WaitMessagesBytes);
        //        }
        //        buf.WriteBytes(CRLF_BYTES);

        //        output.Add(buf);
        //        buf = null;
        //    }
        //    finally
        //    {
        //        buf?.SafeRelease();
        //    }
        //}

        //static void EncodePingMessage(IByteBufferAllocator bufferAllocator, PingPacket packet, List<object> output)
        //{
        //    IByteBuffer buf = null;
        //    try
        //    {
        //        buf = bufferAllocator.Buffer(PING_BYTES.Length);
        //        buf.WriteBytes(PING_BYTES);
        //        buf.WriteBytes(CRLF_BYTES);

        //        output.Add(buf);
        //        buf = null;
        //    }
        //    finally
        //    {
        //        buf?.SafeRelease();
        //    }
        //}

        //static void EncodePongMessage(IByteBufferAllocator bufferAllocator, PongPacket packet, List<object> output)
        //{
        //    IByteBuffer buf = null;
        //    try
        //    {
        //        buf = bufferAllocator.Buffer(PONG_BYTES.Length);
        //        buf.WriteBytes(PONG_BYTES);
        //        buf.WriteBytes(CRLF_BYTES);

        //        output.Add(buf);
        //        buf = null;
        //    }
        //    finally
        //    {
        //        buf?.SafeRelease();
        //    }
        //}

        //static void EncodeConnAckMessage(IByteBufferAllocator bufferAllocator, ConnAckPacket message, List<object> output)
        //{
        //    IByteBuffer buffer = null;
        //    try
        //    {
        //        buffer = bufferAllocator.Buffer(4);
        //        buffer.WriteByte(CalculateFirstByteOfFixedHeader(message));
        //        buffer.WriteByte(2); // remaining length
        //        if (message.SessionPresent)
        //        {
        //            buffer.WriteByte(1); // 7 reserved 0-bits and SP = 1
        //        }
        //        else
        //        {
        //            buffer.WriteByte(0); // 7 reserved 0-bits and SP = 0
        //        }
        //        buffer.WriteByte((byte)message.ReturnCode);


        //        output.Add(buffer);
        //        buffer = null;
        //    }
        //    finally
        //    {
        //        buffer?.SafeRelease();
        //    }
        //}

        //static void EncodePublishMessage(IByteBufferAllocator bufferAllocator, PublishPacket packet, List<object> output)
        //{
        //    IByteBuffer payload = packet.Payload ?? Unpooled.Empty;

        //    string topicName = packet.TopicName;
        //    Util.ValidateTopicName(topicName);
        //    byte[] topicNameBytes = EncodeStringInUtf8(topicName);

        //    int variableHeaderBufferSize = StringSizeLength + topicNameBytes.Length +
        //        (packet.QualityOfService > QualityOfService.AtMostOnce ? PacketIdLength : 0);
        //    int payloadBufferSize = payload.ReadableBytes;
        //    int variablePartSize = variableHeaderBufferSize + payloadBufferSize;
        //    int fixedHeaderBufferSize = 1 + MaxVariableLength;

        //    IByteBuffer buf = null;
        //    try
        //    {
        //        buf = bufferAllocator.Buffer(fixedHeaderBufferSize + variablePartSize);
        //        buf.WriteByte(CalculateFirstByteOfFixedHeader(packet));
        //        WriteVariableLengthInt(buf, variablePartSize);
        //        buf.WriteShort(topicNameBytes.Length);
        //        buf.WriteBytes(topicNameBytes);
        //        if (packet.QualityOfService > QualityOfService.AtMostOnce)
        //        {
        //            buf.WriteShort(packet.PacketId);
        //        }

        //        output.Add(buf);
        //        buf = null;
        //    }
        //    finally
        //    {
        //        buf?.SafeRelease();
        //    }

        //    if (payload.IsReadable())
        //    {
        //        output.Add(payload.Retain());
        //    }
        //}

        //static void EncodePacketWithIdOnly(IByteBufferAllocator bufferAllocator, PacketWithId packet, List<object> output)
        //{
        //    int msgId = packet.PacketId;

        //    const int VariableHeaderBufferSize = PacketIdLength; // variable part only has a packet id
        //    int fixedHeaderBufferSize = 1 + MaxVariableLength;
        //    IByteBuffer buffer = null;
        //    try
        //    {
        //        buffer = bufferAllocator.Buffer(fixedHeaderBufferSize + VariableHeaderBufferSize);
        //        buffer.WriteByte(CalculateFirstByteOfFixedHeader(packet));
        //        WriteVariableLengthInt(buffer, VariableHeaderBufferSize);
        //        buffer.WriteShort(msgId);

        //        output.Add(buffer);
        //        buffer = null;
        //    }
        //    finally
        //    {
        //        buffer?.SafeRelease();
        //    }
        //}

        //static void EncodeSubscribeMessage(IByteBufferAllocator bufferAllocator, SubscribePacket packet, List<object> output)
        //{
        //    const int VariableHeaderSize = PacketIdLength;
        //    int payloadBufferSize = 0;

        //    ThreadLocalObjectList encodedTopicFilters = ThreadLocalObjectList.NewInstance();

        //    IByteBuffer buf = null;
        //    try
        //    {
        //        foreach (SubscriptionRequest topic in packet.Requests)
        //        {
        //            byte[] topicFilterBytes = EncodeStringInUtf8(topic.TopicFilter);
        //            payloadBufferSize += StringSizeLength + topicFilterBytes.Length + 1; // length, value, QoS
        //            encodedTopicFilters.Add(topicFilterBytes);
        //        }

        //        int variablePartSize = VariableHeaderSize + payloadBufferSize;
        //        int fixedHeaderBufferSize = 1 + MaxVariableLength;

        //        buf = bufferAllocator.Buffer(fixedHeaderBufferSize + variablePartSize);
        //        buf.WriteByte(CalculateFirstByteOfFixedHeader(packet));
        //        WriteVariableLengthInt(buf, variablePartSize);

        //        // Variable Header
        //        buf.WriteShort(packet.PacketId); // todo: review: validate?

        //        // Payload
        //        for (int i = 0; i < encodedTopicFilters.Count; i++)
        //        {
        //            var topicFilterBytes = (byte[])encodedTopicFilters[i];
        //            buf.WriteShort(topicFilterBytes.Length);
        //            buf.WriteBytes(topicFilterBytes, 0, topicFilterBytes.Length);
        //            buf.WriteByte((int)packet.Requests[i].QualityOfService);
        //        }

        //        output.Add(buf);
        //        buf = null;
        //    }
        //    finally
        //    {
        //        buf?.SafeRelease();
        //        encodedTopicFilters.Return();
        //    }
        //}

        //static void EncodeSubAckMessage(IByteBufferAllocator bufferAllocator, SubAckPacket message, List<object> output)
        //{
        //    int payloadBufferSize = message.ReturnCodes.Count;
        //    int variablePartSize = PacketIdLength + payloadBufferSize;
        //    int fixedHeaderBufferSize = 1 + MaxVariableLength;
        //    IByteBuffer buf = null;
        //    try
        //    {
        //        buf = bufferAllocator.Buffer(fixedHeaderBufferSize + variablePartSize);
        //        buf.WriteByte(CalculateFirstByteOfFixedHeader(message));
        //        WriteVariableLengthInt(buf, variablePartSize);
        //        buf.WriteShort(message.PacketId);
        //        foreach (QualityOfService qos in message.ReturnCodes)
        //        {
        //            buf.WriteByte((byte)qos);
        //        }

        //        output.Add(buf);
        //        buf = null;

        //    }
        //    finally
        //    {
        //        buf?.SafeRelease();
        //    }
        //}

        //static void EncodeUnsubscribeMessage(IByteBufferAllocator bufferAllocator, UnsubscribePacket packet, List<object> output)
        //{
        //    const int VariableHeaderSize = 2;
        //    int payloadBufferSize = 0;

        //    ThreadLocalObjectList encodedTopicFilters = ThreadLocalObjectList.NewInstance();

        //    IByteBuffer buf = null;
        //    try
        //    {
        //        foreach (string topic in packet.TopicFilters)
        //        {
        //            byte[] topicFilterBytes = EncodeStringInUtf8(topic);
        //            payloadBufferSize += StringSizeLength + topicFilterBytes.Length; // length, value
        //            encodedTopicFilters.Add(topicFilterBytes);
        //        }

        //        int variablePartSize = VariableHeaderSize + payloadBufferSize;
        //        int fixedHeaderBufferSize = 1 + MaxVariableLength;

        //        buf = bufferAllocator.Buffer(fixedHeaderBufferSize + variablePartSize);
        //        buf.WriteByte(CalculateFirstByteOfFixedHeader(packet));
        //        WriteVariableLengthInt(buf, variablePartSize);

        //        // Variable Header
        //        buf.WriteShort(packet.PacketId); // todo: review: validate?

        //        // Payload
        //        for (int i = 0; i < encodedTopicFilters.Count; i++)
        //        {
        //            var topicFilterBytes = (byte[])encodedTopicFilters[i];
        //            buf.WriteShort(topicFilterBytes.Length);
        //            buf.WriteBytes(topicFilterBytes, 0, topicFilterBytes.Length);
        //        }

        //        output.Add(buf);
        //        buf = null;
        //    }
        //    finally
        //    {
        //        buf?.SafeRelease();
        //        encodedTopicFilters.Return();
        //    }
        //}

        //static void EncodePacketWithFixedHeaderOnly(IByteBufferAllocator bufferAllocator, Packet packet, List<object> output)
        //{
        //    IByteBuffer buffer = null;
        //    try
        //    {
        //        buffer = bufferAllocator.Buffer(2);
        //        buffer.WriteByte(CalculateFirstByteOfFixedHeader(packet));
        //        buffer.WriteByte(0);

        //        output.Add(buffer);
        //        buffer = null;
        //    }
        //    finally
        //    {
        //        buffer?.SafeRelease();
        //    }
        //}

        static byte[] EncodeStringInUtf8(string s)
        {
            if (string.IsNullOrEmpty(s)) return EMPTY_BYTES;
            return Encoding.UTF8.GetBytes(s);
        }
    }
}