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
                switch (input.ReadByte())
                {
                    case STANConstants.NEWLINES_CR:
                        if (STANConstants.NEWLINES_LF == input.ReadByte()) return input.GetString(startIndex, i, Encoding.UTF8);
                        throw new FormatException($"NATS protocol name of `{packetSignature}` is invalid.");
                    default:
                        break;
                }
            }
            return string.Empty;
        }

        static byte[] GetBytesFromNewlineDelimiter(IByteBuffer input, int payloadSize, string packetSignature)
        {
            var payload = new byte[payloadSize];
            for (int i = 0; input.ReadableBytes > 0; i++)
            {
                var currentByte = input.ReadByte();
                switch (currentByte)
                {
                    case STANConstants.NEWLINES_CR:
                        if (STANConstants.NEWLINES_LF == input.ReadByte()) break;
                        throw new FormatException($"NATS protocol name of `{packetSignature}` is invalid.");
                    default:
                        payload[i] = currentByte;
                        break;
                }
            }
            return payload;
        }

        static STANPacket DecodePacketInternal(IByteBuffer buffer, string packetSignature, IChannelHandlerContext context)
        {
            switch (packetSignature)
            {
                case STANSignatures.INFO:
                    return DecodeInfoPacket(buffer, context);
                case STANSignatures.ConnectResponse:
                    return DecodeConnectRequestPacket(buffer, context);
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
        static STANPacket DecodeConnectRequestPacket(IByteBuffer buffer, IChannelHandlerContext context)
        {
            return null;
            //return ConnectResponsePacket.CreateFromJson(DecodeString(buffer));
        }

        static STANPacket DecodeMessagePacket(IByteBuffer buffer, IChannelHandlerContext context)
        {
            var MSGPacket = new MessagePacket
            {
                Subject = GetStringFromFieldDelimiters(buffer, STANSignatures.MSG),
                SubscribeId = GetStringFromFieldDelimiters(buffer, STANSignatures.MSG),
                ReplyTo = GetStringFromFieldDelimiters(buffer, STANSignatures.MSG)
            };

            if (int.TryParse(GetStringFromNewlineDelimiters(buffer, STANSignatures.MSG), out int payloadSize))
            {
                MSGPacket.PayloadSize = payloadSize;
            }

            MSGPacket.Payload = GetBytesFromNewlineDelimiter(buffer, payloadSize, STANSignatures.MSG);

            return MSGPacket;
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