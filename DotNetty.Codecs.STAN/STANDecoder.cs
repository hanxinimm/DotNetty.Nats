// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace DotNetty.Codecs.STAN
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Runtime.CompilerServices;
    using System.Text;
    using DotNetty.Buffers;
    using DotNetty.Codecs.STAN.Packets;
    using DotNetty.Codecs.STAN.Protocol;
    using DotNetty.Transport.Channels;
    using Google.Protobuf;

    public sealed class STANDecoder : ReplayingDecoder<ParseState>
    {
        public STANDecoder()
            : base(ParseState.Ready)
        {

        }

        protected override void Decode(IChannelHandlerContext context, IByteBuffer input, List<object> output)
        {
            try
            {
                switch (this.State)
                {
                    case ParseState.Ready:
                        if (!TryDecodePacket(input, context, out STANPacket packet))
                        {
                            this.RequestReplay();
                            return;
                        }
                        output.Add(packet);
                        if (input.ReadableBytes > 0)
                        {

                        }
                        this.Checkpoint(ParseState.Ready);
                        break;
                    case ParseState.Failed:
                        // read out data until connection is closed
                        input.SkipBytes(input.ReadableBytes);
                        return;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            catch (DecoderException)
            {
                input.SkipBytes(input.ReadableBytes);
                this.Checkpoint(ParseState.Failed);
                throw;
            }
        }

        

        static bool TryDecodePacket(IByteBuffer buffer, IChannelHandlerContext context, out STANPacket packet)
        {
            try
            {

                if (buffer.ReadableBytes == 0)
                {
                    packet = null;
                    return false;
                }

                string signature = GetSignature(buffer);

                DebugLogger.LogSignature(signature);

                packet = DecodePacketInternal(buffer, signature, context);

                return packet != null;
            }
            catch (Exception ex)
            {
                packet = null;
                return false;
            }
        }

        static string GetSignature(IByteBuffer input)
        {
            int startIndex = input.ReaderIndex;
            for (int i = 0; input.ReadableBytes > 0; i++)
            {
                switch (input.ReadByte())
                {
                    case STANConstants.FIELDDELIMITER_SPACES:
                    case STANConstants.FIELDDELIMITER_TAB:
                        return input.GetString(startIndex, i, Encoding.UTF8);
                    case STANConstants.NEWLINES_CR:
                        if (STANConstants.NEWLINES_LF == input.ReadByte()) return input.GetString(startIndex, i, Encoding.UTF8);
                        throw new FormatException($"NATS Newlines is invalid.");
                    default:
                        break;
                }
            }
            return string.Empty;
        }

        static string GetStringFromFieldDelimiters(IByteBuffer input, string packetSignature)
        {
            int startIndex = input.ReaderIndex;
            for (int i = 0; input.ReadableBytes > 0; i++)
            {
                switch (input.ReadByte())
                {
                    case STANConstants.FIELDDELIMITER_SPACES:
                    case STANConstants.FIELDDELIMITER_TAB:
                        return input.GetString(startIndex, i, Encoding.UTF8);
                    case STANConstants.NEWLINES_CR:
                        if (STANConstants.NEWLINES_LF == input.ReadByte())
                        {
                            input.SetReaderIndex(startIndex);
                            return string.Empty;
                        }
                        throw new FormatException($"NATS protocol name of `{packetSignature}` is invalid.");
                    default:
                        break;
                }
            }
            return string.Empty;
        }

        static string GetStringFromNewlineDelimiters(IByteBuffer input, string packetSignature)
        {
            int startIndex = input.ReaderIndex;
            for (int i = 0; input.ReadableBytes > 0; i++)
            {
                if (input.ReadByte() == STANConstants.NEWLINES_CR)
                {
                    if (STANConstants.NEWLINES_LF == input.ReadByte()) return input.GetString(startIndex, i, Encoding.UTF8);
                    throw new FormatException($"NATS protocol name of `{packetSignature}` is invalid.");
                }
            }
            return string.Empty;
        }

        static byte[] GetBytesFromNewlineDelimiter(IByteBuffer input, int payloadSize, string packetSignature)
        {
            if (payloadSize == 0)
            {
                if (input.ReadByte() == STANConstants.NEWLINES_CR && input.ReadByte() == STANConstants.NEWLINES_LF) return new byte[0];
                throw new FormatException($"NATS protocol name of `{packetSignature}` is invalid.");
            }


            var payload = new byte[payloadSize];
            for (int i = 0; input.ReadableBytes > 0; i++)
            {
                var currentByte = input.ReadByte();
                if (currentByte == STANConstants.NEWLINES_CR)
                {
                    if (input.ReadByte() == STANConstants.NEWLINES_LF) break;
                    throw new FormatException($"NATS protocol name of `{packetSignature}` is invalid.");
                }
                else
                {
                    payload[i] = currentByte;
                }
            }
            return payload;
        }

        static string GetInbox(string subject)
        {
            if (subject.Length > 12)
            {
                return subject.Substring(0, 12);
            }
            return string.Empty;
        }

        static STANPacket DecodePacketInternal(IByteBuffer buffer, string packetSignature, IChannelHandlerContext context)
        {
            switch (packetSignature)
            {
                case STANSignatures.INFO:
                    return DecodeInfoPacket(buffer, context);
                case STANSignatures.MSG:
                    return DecodeMessagePacket(buffer, context);
                case STANSignatures.OK:
                    return DecodeOKPacket(buffer, context);
                case STANSignatures.PING:
                    return DecodePingPacket(buffer, context);
                case STANSignatures.PONG:
                    return DecodePongPacket(buffer, context);
                case STANSignatures.ERR:
                    return DecodeErrorPacket(buffer, context);
                default:
                    Console.WriteLine("--|{0}|--", packetSignature);
                    return null;
                    //throw new DecoderException($"NATS protocol operation name of `{packetSignature}` is invalid.");
            }
        }

        static STANPacket DecodeInfoPacket(IByteBuffer buffer, IChannelHandlerContext context)
        {
            return InfoPacket.CreateFromJson(DecodeString(buffer));
        }

        static STANPacket DecodeMessagePacket(IByteBuffer buffer, IChannelHandlerContext context)
        {

            var Subject = GetStringFromFieldDelimiters(buffer, STANSignatures.MSG);
            var SubscribeId = GetStringFromFieldDelimiters(buffer, STANSignatures.MSG);
            var ReplyTo = GetStringFromFieldDelimiters(buffer, STANSignatures.MSG);

            if (int.TryParse(GetStringFromNewlineDelimiters(buffer, STANSignatures.MSG), out int payloadSize))
            {
                var Payload = GetBytesFromNewlineDelimiter(buffer, payloadSize, STANSignatures.MSG);

                return DecodeMessagePacket(Subject, SubscribeId, ReplyTo, payloadSize, Payload);
            }

            return null;
        }

        static STANPacket DecodeMessagePacket(string subject, string subscribeId, string replyTo, int payloadSize, byte[] payload)
        {
            switch (GetInbox(subject))
            {
                case STANInboxs.ConnectResponse:
                    return GetMessagePacket<ConnectResponsePacket, ConnectResponse>(subject, subscribeId, replyTo, payloadSize, payload);
                case STANInboxs.SubscriptionResponse:
                    return GetMessagePacket<SubscriptionResponsePacket, SubscriptionResponse>(subject, subscribeId, replyTo, payloadSize, payload);
                case STANInboxs.PubAck:
                    return GetMessagePacket<PubAckPacket, PubAck>(subject, subscribeId, replyTo, payloadSize, payload);
                case STANInboxs.MsgProto:
                    return GetMessagePacket<MsgProtoPacket, MsgProto>(subject, subscribeId, replyTo, payloadSize, payload);
                case STANInboxs.CloseResponse:
                    return GetMessagePacket<CloseResponsePacket, CloseResponse>(subject, subscribeId, replyTo, payloadSize, payload);
                default:
                    return null;
            }
        }

        static STANPacket GetMessagePacket<TMessagePacket, TMessage>(string subject, string subscribeId, string replyTo, int payloadSize, byte[] payload)
            where TMessagePacket : MessagePacket<TMessage>, new()
            where TMessage : IMessage, new()
        {
            var Packet = new TMessagePacket();
            Packet.Subject = subject;
            Packet.SubscribeId = subscribeId;
            Packet.ReplyTo = replyTo;
            Packet.PayloadSize = payloadSize;

            var Message = new TMessage();
            Message.MergeFrom(payload);
            Packet.Message = Message;

            return Packet;
        }

        static STANPacket DecodeErrorPacket(IByteBuffer buffer, IChannelHandlerContext context)
        {
            return new ErrorPacket(DecodeStringNew(buffer));
        }

        static STANPacket DecodeOKPacket(IByteBuffer buffer, IChannelHandlerContext context)
        { 
            return new OKPacket();
        }

        static STANPacket DecodePingPacket(IByteBuffer buffer, IChannelHandlerContext context)
        {
            return new PingPacket();
        }

        static STANPacket DecodePongPacket(IByteBuffer buffer, IChannelHandlerContext context)
        {
            return new PongPacket();
        }

        static string DecodeString(IByteBuffer buffer) => DecodeString(buffer, 0, 20480);

        static string DecodeString(IByteBuffer buffer, int minBytes, int maxBytes)
        {
            int size = buffer.ReadableBytes;

            if (size < minBytes)
            {
                throw new DecoderException($"String value is shorter than minimum allowed {minBytes}. Advertised length: {size}");
            }
            if (size > maxBytes)
            {
                throw new DecoderException($"String value is longer than maximum allowed {maxBytes}. Advertised length: {size}");
            }

            if (size <= 0)
            {
                return string.Empty;
            }

            return buffer.ReadBytes(size).ToString(Encoding.UTF8);
        }
        static string DecodeStringNew(IByteBuffer buffer)
        {
            int size = buffer.ReadableBytes;

            if (size <= 0)
            {
                return string.Empty;
            }

            return buffer.ReadBytes(size).ToString(Encoding.UTF8);
        }
    }
}