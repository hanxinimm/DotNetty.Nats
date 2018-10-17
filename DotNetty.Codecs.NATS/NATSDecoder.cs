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
            int startIndex = input.ReaderIndex;
            for (int i = 0; input.ReadableBytes > 0; i++)
            {
                switch (input.ReadByte())
                {
                    case NATSConstants.FIELDDELIMITER_SPACES:
                    case NATSConstants.FIELDDELIMITER_TAB:
                        return input.GetString(startIndex, i, Encoding.UTF8);
                    case NATSConstants.NEWLINES_CR:
                        if (NATSConstants.NEWLINES_LF == input.ReadByte()) return input.GetString(startIndex, i, Encoding.UTF8);
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

        static string GetStringFromNewlineDelimiters(IByteBuffer input, string packetSignature)
        {
            int startIndex = input.ReaderIndex;
            for (int i = 0; input.ReadableBytes > 0; i++)
            {
                if (input.ReadByte() == NATSConstants.NEWLINES_CR)
                {
                    if (NATSConstants.NEWLINES_LF == input.ReadByte()) return input.GetString(startIndex, i, Encoding.UTF8);
                    throw new FormatException($"NATS protocol name of `{packetSignature}` is invalid.");
                }
            }
            return string.Empty;
        }

        static byte[] GetBytesFromNewlineDelimiter(IByteBuffer input, int payloadSize, string packetSignature)
        {
            if (payloadSize == 0)
            {
                if (input.ReadByte() == NATSConstants.NEWLINES_CR && input.ReadByte() == NATSConstants.NEWLINES_LF) return new byte[0];
                throw new FormatException($"NATS protocol name of `{packetSignature}` is invalid.");
            }


            var payload = new byte[payloadSize];
            for (int i = 0; input.ReadableBytes > 0; i++)
            {
                var currentByte = input.ReadByte();
                if (currentByte == NATSConstants.NEWLINES_CR)
                {
                    if (input.ReadByte() == NATSConstants.NEWLINES_LF) break;
                    throw new FormatException($"NATS protocol name of `{packetSignature}` is invalid.");
                }
                else
                {
                    payload[i] = currentByte;
                }
            }
            return payload;
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

            var MSGPacket = new MessagePacket
            {
                Subject = GetStringFromFieldDelimiters(buffer, NATSSignatures.MSG),
                SubscribeId = GetStringFromFieldDelimiters(buffer, NATSSignatures.MSG),
                ReplyTo = GetStringFromFieldDelimiters(buffer, NATSSignatures.MSG)
            };

            if (int.TryParse(GetStringFromNewlineDelimiters(buffer, NATSSignatures.MSG), out int payloadSize))
            {
                MSGPacket.Payload = GetBytesFromNewlineDelimiter(buffer, payloadSize, NATSSignatures.MSG);

                return MSGPacket;
            }

            return null;

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

            return buffer.ReadBytes(size).ToString(Encoding.UTF8);
        }
    }
}