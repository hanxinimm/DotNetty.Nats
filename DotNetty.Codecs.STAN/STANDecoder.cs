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
                        break;
                    //TODO:待分析什么情况下是错误的
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

            if (buffer.ReadableBytes == 0)
            {
                packet = null;
                return false;
            }

            string signature = GetSignature(buffer);

            if (string.IsNullOrWhiteSpace(signature))
            {
                packet = null;
                return false;
            }

            DebugLogger.LogSignature(signature);

            packet = DecodePacketInternal(buffer, signature, context);

            return packet != null;
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
                        if (STANConstants.NEWLINES_LF == input.ReadByte())
                        {
                            //只有OK,PING,PONG支持换行符结尾
                            switch (input.GetString(startIndex, i, Encoding.UTF8))
                            {
                                case STANSignatures.OK:
                                    return STANSignatures.OK;
                                case STANSignatures.PING:
                                    return STANSignatures.PING;
                                case STANSignatures.PONG:
                                    return STANSignatures.PONG;
                                default:
#if DEBUG
                                    throw new FormatException($"STAN Newlines is invalid.");
#else
                                    return string.Empty;;
#endif

                            }
                        }
#if DEBUG
                        throw new FormatException($"STAN Newlines is invalid.");
#else
                    break;
#endif
                    default:
                        break;
                }
            }
            return string.Empty;
        }

        static bool TryGetStringFromFieldDelimiter(IByteBuffer input, string packetSignature, out string value)
        {
            value = null;
            int startIndex = input.ReaderIndex;
            for (int i = 0; input.ReadableBytes > 0; i++)
            {
                switch (input.ReadByte())
                {
                    case STANConstants.FIELDDELIMITER_SPACES:
                    case STANConstants.FIELDDELIMITER_TAB:
                        value = input.GetString(startIndex, i, Encoding.UTF8);
                        return true;
                    case STANConstants.NEWLINES_CR:
                    case STANConstants.NEWLINES_LF:
#if DEBUG
                        throw new FormatException($"STAN protocol name of `{packetSignature}` is invalid.");
#else
                        return false;
#endif
                    default:
                        break;
                }
            }
            return false;
        }

        static string GetStringFromFieldDelimiter(IByteBuffer input, string packetSignature)
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
                        throw new FormatException($"STAN protocol name of `{packetSignature}` is invalid.");
                    default:
                        break;
                }
            }
            return string.Empty;
        }

        static bool TryGetStringFromNewlineDelimiter(IByteBuffer input, string packetSignature, out string value)
        {
            value = null;
            int startIndex = input.ReaderIndex;
            for (int i = 0; input.ReadableBytes > 0; i++)
            {
                if (input.ReadByte() == STANConstants.NEWLINES_CR)
                {
                    if (STANConstants.NEWLINES_LF == input.ReadByte())
                    {
                        value = input.GetString(startIndex, i, Encoding.UTF8);
                        return true;
                    }
#if DEBUG
                    throw new FormatException($"STAN protocol name of `{packetSignature}` is invalid.");
#else
                    break;
#endif
                }
            }
            return false;
        }

        static bool TryGetBytesFromNewlineDelimiter(IByteBuffer input, int payloadSize, string packetSignature, out byte[] value)
        {
            value = null;

            if (input.ReadableBytes < payloadSize + 2)
            {
                return false;
            }

            if (payloadSize == 0)
            {
                if (input.ReadByte() == STANConstants.NEWLINES_CR && input.ReadByte() == STANConstants.NEWLINES_LF)
                {
                    value = new byte[0];
                    return true;
                }
#if DEBUG
                throw new FormatException($"STAN protocol name of `{packetSignature}` is invalid.");
#endif
            }

            if (input.GetByte(input.ReaderIndex + payloadSize) != STANConstants.NEWLINES_CR || input.GetByte(input.ReaderIndex + payloadSize + 1) != STANConstants.NEWLINES_LF)
#if DEBUG
                throw new FormatException($"STAN protocol name of `{packetSignature}` is invalid.");
#else
                    return false;
#endif

            value = new byte[payloadSize];
            for (int i = 0; payloadSize > i; i++)
            {
                value[i] = input.ReadByte();
            }

            //跳过消息结尾的NEWLINES_CR和NEWLINES_LF字符
            input.SkipBytes(2);
            return true;
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
#if DEBUG
                    Console.WriteLine("--|{0}|--", packetSignature);
                    throw new DecoderException($"NATS protocol operation name of `{packetSignature}` is invalid.");
#else
                    return null;
#endif
            }
        }

        static STANPacket DecodeInfoPacket(IByteBuffer buffer, IChannelHandlerContext context)
        {
            if (TryGetStringFromNewlineDelimiter(buffer, STANSignatures.INFO, out var infoJson))
            {
                return InfoPacket.CreateFromJson(infoJson);
            }
            return null;
        }

        static STANPacket DecodeMessagePacket(IByteBuffer buffer, IChannelHandlerContext context)
        {

            if (TryGetStringFromFieldDelimiter(buffer, STANSignatures.MSG, out var subject))
            {
                var ReplyTo = GetStringFromFieldDelimiter(buffer, STANSignatures.MSG);

                if (TryGetStringFromNewlineDelimiter(buffer, STANSignatures.MSG, out var payloadSizeString))
                {

                    if (int.TryParse(payloadSizeString, out int payloadSize))
                    {
                        if (TryGetBytesFromNewlineDelimiter(buffer, payloadSize, STANSignatures.MSG, out var payload))
                        {

                            return DecodeMessagePacket(subject, ReplyTo, payloadSize, payload);
                        }
                    }
                }
            }

            return null;
        }

        static STANPacket DecodeMessagePacket(string subject, string replyTo, int payloadSize, byte[] payload)
        {
            switch (GetInbox(subject))
            {
                case STANInboxs.ConnectResponse:
                    return GetMessagePacket<ConnectResponsePacket, ConnectResponse>(subject, replyTo, payloadSize, payload);
                case STANInboxs.SubscriptionResponse:
                    return GetMessagePacket<SubscriptionResponsePacket, SubscriptionResponse>(subject, replyTo, payloadSize, payload);
                case STANInboxs.PubAck:
                    return GetMessagePacket<PubAckPacket, PubAck>(subject, replyTo, payloadSize, payload);
                case STANInboxs.MsgProto:
                    return GetMessagePacket<MsgProtoPacket, MsgProto>(subject, replyTo, payloadSize, payload);
                case STANInboxs.CloseResponse:
                    return GetMessagePacket<CloseResponsePacket, CloseResponse>(subject, replyTo, payloadSize, payload);
                default:
                    return null;
            }
        }

        static STANPacket GetMessagePacket<TMessagePacket, TMessage>(string subject, string replyTo, int payloadSize, byte[] payload)
            where TMessagePacket : MessagePacket<TMessage>, new()
            where TMessage : IMessage, new()
        {
            var Packet = new TMessagePacket
            {
                Subject = subject,
                ReplyTo = replyTo,
                PayloadSize = payloadSize
            };

            var Message = new TMessage();
            Message.MergeFrom(payload);
            Packet.Message = Message;

            return Packet;
        }

        static STANPacket DecodeErrorPacket(IByteBuffer buffer, IChannelHandlerContext context)
        {
            if (TryGetStringFromNewlineDelimiter(buffer, STANSignatures.ERR, out var error))
            {
                return new ErrorPacket(error);
            }
            return null;
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
    }
}