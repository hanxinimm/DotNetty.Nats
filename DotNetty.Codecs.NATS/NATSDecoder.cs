// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace DotNetty.Codecs.NATS
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using System.Text;
    using DotNetty.Buffers;
    using DotNetty.Codecs.NATS.Packets;
    using DotNetty.Transport.Channels;

    public sealed class NATSDecoder : ReplayingDecoder<ParseState>
    {
        public NATSDecoder()
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
                        if (!TryDecodePacket(input, context, out NATSPacket packet))
                        {
                            this.RequestReplay();
                            return;
                        }
                        output.Add(packet);
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

        

        static bool TryDecodePacket(IByteBuffer buffer, IChannelHandlerContext context, out NATSPacket packet)
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
                    case NATSConstants.FIELDDELIMITER_SPACES:
                    case NATSConstants.FIELDDELIMITER_TAB:
                        return input.GetString(startIndex, i, Encoding.UTF8);
                    case NATSConstants.NEWLINES_CR:
                        if (NATSConstants.NEWLINES_LF == input.ReadByte())
                        {
                            //只有OK,PING,PONG支持换行符结尾
                            switch (input.GetString(startIndex, i, Encoding.UTF8))
                            {
                                case NATSSignatures.OK:
                                    return NATSSignatures.OK;
                                case NATSSignatures.PING:
                                    return NATSSignatures.PING;
                                case NATSSignatures.PONG:
                                    return NATSSignatures.PONG;
                                default:
#if DEBUG
                                    throw new FormatException($"NATS Newlines is invalid.");
#else
                                    return string.Empty;
#endif
                            }
                        }
#if DEBUG
                        throw new FormatException($"NATS Newlines is invalid.");
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
                    case NATSConstants.FIELDDELIMITER_SPACES:
                    case NATSConstants.FIELDDELIMITER_TAB:
                        value = input.GetString(startIndex, i, Encoding.UTF8);
                        return true;
                    case NATSConstants.NEWLINES_CR:
                    case NATSConstants.NEWLINES_LF:
#if DEBUG
                        throw new FormatException($"NATS protocol name of `{packetSignature}` is invalid.");
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
                    case NATSConstants.FIELDDELIMITER_SPACES:
                    case NATSConstants.FIELDDELIMITER_TAB:
                        return input.GetString(startIndex, i, Encoding.UTF8);
                    case NATSConstants.NEWLINES_CR:
                        if (NATSConstants.NEWLINES_LF == input.ReadByte())
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

        static bool TryGetStringFromNewlineDelimiter(IByteBuffer input, string packetSignature, out string value)
        {
            value = null;
            int startIndex = input.ReaderIndex;
            for (int i = 0; input.ReadableBytes > 0; i++)
            {
                if (input.ReadByte() == NATSConstants.NEWLINES_CR)
                {
                    if (NATSConstants.NEWLINES_LF == input.ReadByte())
                    {
                        value = input.GetString(startIndex, i, Encoding.UTF8);
                        return true;
                    }
#if DEBUG
                    throw new FormatException($"NATS protocol name of `{packetSignature}` is invalid.");
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
                if (input.ReadByte() == NATSConstants.NEWLINES_CR && input.ReadByte() == NATSConstants.NEWLINES_LF)
                {
                    value = new byte[0];
                    return true;
                }
#if DEBUG
                throw new FormatException($"NATS protocol name of `{packetSignature}` is invalid.");
#endif
            }

            if (input.GetByte(input.ReaderIndex + payloadSize) != NATSConstants.NEWLINES_CR || input.GetByte(input.ReaderIndex + payloadSize + 1) != NATSConstants.NEWLINES_LF)
#if DEBUG
                throw new FormatException($"NATS protocol name of `{packetSignature}` is invalid.");
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

        static NATSPacket DecodePacketInternal(IByteBuffer buffer, string packetSignature, IChannelHandlerContext context)
        {
            switch (packetSignature)
            {
                case NATSSignatures.INFO:
                    return DecodeInfoPacket(buffer, context);
                case NATSSignatures.MSG:
                    return DecodeMessagePacket(buffer, context);
                case NATSSignatures.OK:
                    return DecodeOKPacket(buffer, context);
                case NATSSignatures.PING:
                    return DecodePingPacket(buffer, context);
                case NATSSignatures.PONG:
                    return DecodePongPacket(buffer, context);
                case NATSSignatures.ERR:
                    return DecodeErrorPacket(buffer, context);
                default:
                    Console.WriteLine("--|{0}|--", packetSignature);
                    return null;
                    //throw new DecoderException($"NATS protocol operation name of `{packetSignature}` is invalid.");
            }
        }

        static NATSPacket DecodeInfoPacket(IByteBuffer buffer, IChannelHandlerContext context)
        {
            if (TryGetStringFromNewlineDelimiter(buffer, NATSSignatures.INFO, out var infoJson))
            {
                return InfoPacket.CreateFromJson(infoJson);
            }
            return null;
        }

        static NATSPacket DecodeMessagePacket(IByteBuffer buffer, IChannelHandlerContext context)
        {

            if (TryGetStringFromFieldDelimiter(buffer, NATSSignatures.MSG, out var subject))
            {
                if (TryGetStringFromFieldDelimiter(buffer, NATSSignatures.MSG, out var subscribeId))
                {
                    var ReplyTo = GetStringFromFieldDelimiter(buffer, NATSSignatures.MSG);

                    if (TryGetStringFromNewlineDelimiter(buffer, NATSSignatures.MSG, out var payloadSizeString))
                    {

                        if (int.TryParse(payloadSizeString, out int payloadSize))
                        {
                            if (TryGetBytesFromNewlineDelimiter(buffer, payloadSize, NATSSignatures.MSG, out var payload))
                            {

                                return new MessagePacket
                                {
                                    Subject = subject,
                                    SubscribeId = subscribeId,
                                    ReplyTo = ReplyTo,
                                    PayloadSize = payloadSize,
                                    Payload = payload
                                };
                            }
                        }
                    }
                }
            }

            return null;
        }

        static NATSPacket DecodeErrorPacket(IByteBuffer buffer, IChannelHandlerContext context)
        {
            if (TryGetStringFromNewlineDelimiter(buffer, NATSSignatures.ERR, out var error))
            {
                return new ErrorPacket(error);
            }
            return null;
        }

        static NATSPacket DecodeOKPacket(IByteBuffer buffer, IChannelHandlerContext context)
        {
            return new OKPacket();
        }

        static NATSPacket DecodePingPacket(IByteBuffer buffer, IChannelHandlerContext context)
        {
            return new PingPacket();
        }

        static NATSPacket DecodePongPacket(IByteBuffer buffer, IChannelHandlerContext context)
        {
            return new PongPacket();
        }
    }
}