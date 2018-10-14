// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace DotNetty.Codecs.STAN
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using System.Text;
    using DotNetty.Buffers;
    using DotNetty.Codecs.STAN.Packets;
    using DotNetty.Transport.Channels;

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
                        input.SkipBytes(input.ReadableBytes);
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
            for (int i = 0; i < input.WriterIndex; i++)
            {
                switch (input.ReadByte())
                {
                    case STANConstants.FIELDDELIMITER_SPACES:
                    case STANConstants.FIELDDELIMITER_TAB:
                    case STANConstants.NEWLINES_CR:
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
                    case STANConstants.FIELDDELIMITER_SPACES:
                    case STANConstants.FIELDDELIMITER_TAB:
                        return input.GetString(startIndex, i, Encoding.UTF8);
                    case STANConstants.NEWLINES_CR:
                        if (STANConstants.NEWLINES_LF == input.ReadByte()) return string.Empty;
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
                    case STANConstants.FIELDDELIMITER_SPACES:
                    case STANConstants.FIELDDELIMITER_TAB:
                        return Unpooled.Empty;
                    case STANConstants.NEWLINES_CR:
                        if (STANConstants.NEWLINES_LF == input.ReadByte())
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

        static STANPacket DecodePacketInternal(IByteBuffer buffer, string packetSignature, IChannelHandlerContext context)
        {
            switch (packetSignature)
            {
                case STANSignatures.INFO:
                    return DecodeInfoPacket(buffer, context);
                case STANSignatures.ConnectRequest:
                    return DecodeConnectRequestPacket(buffer, context);
                //case Signatures.ConnectResponse:
                //    return DecodeMessagePacket(buffer, context);
                case STANSignatures.OK:
                    return DecodeOKPacket(buffer, context);
                //case Signatures.PING:
                //    return DecodePingPacket(buffer, context);
                //case Signatures.PONG:
                //    return DecodePongPacket(buffer, context);
                //case Signatures.ERR:
                //    return DecodeErrorPacket(buffer, context);
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

        //static Packet DecodeMessagePacket(IByteBuffer buffer, IChannelHandlerContext context)
        //{

        //    var Subject = GetStringFromFieldDelimiters(buffer, buffer.ReaderIndex, buffer.ReadableBytes, Signatures.MSG);

        //    var SubscribeId = GetStringFromFieldDelimiters(buffer, buffer.ReaderIndex, buffer.ReadableBytes, Signatures.MSG);

        //    var ReplyTo = GetStringFromFieldDelimiters(buffer, buffer.ReaderIndex, buffer.ReadableBytes, Signatures.MSG);

        //    var Payload = GetBytesFromNewlines(buffer, buffer.ReaderIndex, buffer.ReadableBytes, Signatures.MSG);

        //    return new MessagePacket(Subject, SubscribeId, Payload, ReplyTo);

        //}

        //static Packet DecodeErrorPacket(IByteBuffer buffer, IChannelHandlerContext context)
        //{
        //    return new ErrorPacket(DecodeStringNew(buffer));
        //}

        static STANPacket DecodeOKPacket(IByteBuffer buffer, IChannelHandlerContext context)
        {
            return new OKPacket();
        }

        //static Packet DecodePingPacket(IByteBuffer buffer, IChannelHandlerContext context)
        //{
        //    return new PingPacket();
        //}

        //static PongPacket DecodePongPacket(IByteBuffer buffer, IChannelHandlerContext context)
        //{
        //    return new PongPacket();
        //}

        static string DecodeString(IByteBuffer buffer) => DecodeString(buffer, 0, 20480);

        static string DecodeString(IByteBuffer buffer, int minBytes, int maxBytes)
        {
            int size = buffer.ReadableBytes - 2;

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

            return buffer.ToString(buffer.ReaderIndex, size, Encoding.UTF8);
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