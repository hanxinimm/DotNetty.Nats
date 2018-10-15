﻿// Copyright (c) Microsoft. All rights reserved.
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
        readonly bool isServer;
        readonly int maxMessageSize;

        public NATSDecoder(bool isServer, int maxMessageSize)
            : base(ParseState.Ready)
        {
            this.isServer = isServer;
            this.maxMessageSize = maxMessageSize;
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

        

        static bool TryDecodePacket(IByteBuffer buffer, IChannelHandlerContext context, out NATSPacket packet)
        {
            if (buffer.ReadableBytes == 0)
            {
                packet = null;
                return false;
            }

            string signature = GetSignature(buffer);

            packet = DecodePacketInternal(buffer, signature, context);

            return packet != null;
        }

        static string GetSignature(IByteBuffer input)
        {
            for (int i = 0; i < input.WriterIndex; i++)
            {
                switch (input.ReadByte())
                {
                    case NATSConstants.FIELDDELIMITER_SPACES:
                    case NATSConstants.FIELDDELIMITER_TAB:
                    case NATSConstants.NEWLINES_CR:
                        return input.GetString(0, i, Encoding.UTF8);
                    default:
                        break;
                }
            }
            return input.GetString(0, input.WriterIndex, Encoding.UTF8);
        }

        static string GetStringFromFieldDelimiters(IByteBuffer input, int startIndex, int readableBytes, string packetSignature)
        {
            for (int i = 0; i < readableBytes; i++)
            {
                switch (input.ReadByte())
                {
                    case NATSConstants.FIELDDELIMITER_SPACES:
                    case NATSConstants.FIELDDELIMITER_TAB:
                        return input.GetString(startIndex, i, Encoding.UTF8);
                    case NATSConstants.NEWLINES_CR:
                        if (NATSConstants.NEWLINES_LF == input.ReadByte()) return string.Empty;
                        throw new FormatException($"NATS protocol name of `{packetSignature}` is invalid.");
                    default:
                        break;
                }
            }
            return input.GetString(startIndex, readableBytes, Encoding.UTF8);
        }

        static IByteBuffer GetBytesFromNewlines(IByteBuffer input, int startIndex, int readableBytes, string packetSignature)
        {
            for (int i = 0; i < readableBytes; i++)
            {
                switch (input.ReadByte())
                {
                    case NATSConstants.FIELDDELIMITER_SPACES:
                    case NATSConstants.FIELDDELIMITER_TAB:
                        return Unpooled.Empty;
                    case NATSConstants.NEWLINES_CR:
                        if (NATSConstants.NEWLINES_LF == input.ReadByte())
                        {
                            input.SetReaderIndex(startIndex);
                            return input.ReadBytes(i);
                        }
                        throw new FormatException($"NATS protocol name of `{packetSignature}` is invalid.");
                    default:
                        break;
                }
            }
            return input.GetBytes(startIndex, input);
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
            return InfoPacket.CreateFromJson(DecodeString(buffer));
        }

        static NATSPacket DecodeMessagePacket(IByteBuffer buffer, IChannelHandlerContext context)
        {

            var Subject = GetStringFromFieldDelimiters(buffer, buffer.ReaderIndex, buffer.ReadableBytes, NATSSignatures.MSG);

            var SubscribeId = GetStringFromFieldDelimiters(buffer, buffer.ReaderIndex, buffer.ReadableBytes, NATSSignatures.MSG);

            //TODO;待核验消息格式:迁移另外一个项目代码过来
            //var ReplyTo = GetStringFromFieldDelimiters(buffer, buffer.ReaderIndex, buffer.ReadableBytes, NATSSignatures.MSG);

            var PayloadSize = GetStringFromFieldDelimiters(buffer, buffer.ReaderIndex, buffer.ReadableBytes, NATSSignatures.MSG);

            var Payload = GetBytesFromNewlines(buffer, buffer.ReaderIndex, buffer.ReadableBytes, NATSSignatures.MSG);

            return new MessagePacket(Subject, SubscribeId, string.Empty, int.Parse(PayloadSize), Payload);

        }

        static NATSPacket DecodeErrorPacket(IByteBuffer buffer, IChannelHandlerContext context)
        {
            return new ErrorPacket(DecodeStringNew(buffer));
        }

        static NATSPacket DecodeOKPacket(IByteBuffer buffer, IChannelHandlerContext context)
        {
            return new OKPacket();
        }

        static NATSPacket DecodePingPacket(IByteBuffer buffer, IChannelHandlerContext context)
        {
            return new PingPacket();
        }

        static PongPacket DecodePongPacket(IByteBuffer buffer, IChannelHandlerContext context)
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

            return buffer.ToString(buffer.ReaderIndex, size, Encoding.UTF8);
        }
    }
}