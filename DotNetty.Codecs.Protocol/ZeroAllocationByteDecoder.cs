using DotNetty.Buffers;
using DotNetty.Transport.Channels;
using System;
using System.Collections.Generic;
using System.Text;


namespace DotNetty.Codecs.Protocol
{

    public abstract class ZeroAllocationByteDecoder : ReplayingDecoder<ParseState>
    {
        public ZeroAllocationByteDecoder()
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
                        if (!TryDecodePacket(input, context, out ProtocolPacket packet))
                        {
                            this.RequestReplay();
                            return;
                        }
                        output.Add(packet);
                        break;
                    case ParseState.Failed:
                        //可以将Failed解释为当消息体内容不符合正确的字节解析流时，跳出当前解析
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



        private bool TryDecodePacket(IByteBuffer buffer, IChannelHandlerContext context, out ProtocolPacket packet)
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

            packet = DecodePacket(buffer, signature, context);

            return packet != null;
        }

        protected static string GetSignature(IByteBuffer input)
        {
            int startIndex = input.ReaderIndex;
            for (int i = 0; input.ReadableBytes > 0; i++)
            {
                switch (input.ReadByte())
                {
                    case ProtocolConstants.FIELDDELIMITER_SPACES:
                    case ProtocolConstants.FIELDDELIMITER_TAB:
                        return input.GetString(startIndex, i, Encoding.UTF8);
                    case ProtocolConstants.NEWLINES_CR:
                        if (ProtocolConstants.NEWLINES_LF == input.SafeReadByte())
                        {
                            //只有OK,PING,PONG支持换行符结尾
                            switch (input.GetString(startIndex, i, Encoding.UTF8))
                            {
                                case ProtocolSignatures.OK:
                                    return ProtocolSignatures.OK;
                                case ProtocolSignatures.PING:
                                    return ProtocolSignatures.PING;
                                case ProtocolSignatures.PONG:
                                    return ProtocolSignatures.PONG;
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

        protected static bool TryGetStringFromFieldDelimiter(IByteBuffer input, string packetSignature, out string value)
        {
            value = null;
            int startIndex = input.ReaderIndex;
            for (int i = 0; input.ReadableBytes > 0; i++)
            {
                switch (input.ReadByte())
                {
                    case ProtocolConstants.FIELDDELIMITER_SPACES:
                    case ProtocolConstants.FIELDDELIMITER_TAB:
                        value = input.GetString(startIndex, i, Encoding.UTF8);
                        return true;
                    case ProtocolConstants.NEWLINES_CR:
                    case ProtocolConstants.NEWLINES_LF:
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

        protected static string GetStringFromFieldDelimiter(IByteBuffer input, string packetSignature)
        {
            int startIndex = input.ReaderIndex;
            for (int i = 0; input.ReadableBytes > 0; i++)
            {
                switch (input.ReadByte())
                {
                    case ProtocolConstants.FIELDDELIMITER_SPACES:
                    case ProtocolConstants.FIELDDELIMITER_TAB:
                        return input.GetString(startIndex, i, Encoding.UTF8);
                    case ProtocolConstants.NEWLINES_CR:
                        if (ProtocolConstants.NEWLINES_LF == input.SafeReadByte())
                        {
                            input.SetReaderIndex(startIndex);
                            return string.Empty;
                        }
#if DEBUG
                        throw new FormatException($"NATS protocol name of `{packetSignature}` is invalid.");
#else
                    break;
#endif
                    default:
                        break;
                }
            }
            return string.Empty;
        }

        protected static bool TryGetStringFromNewlineDelimiter(IByteBuffer input, string packetSignature, out string value)
        {
            value = null;
            int startIndex = input.ReaderIndex;
            for (int i = 0; input.ReadableBytes > 0; i++)
            {
                if (ProtocolConstants.NEWLINES_CR == input.ReadByte())
                {
                    if (ProtocolConstants.NEWLINES_LF == input.SafeReadByte())
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

        protected static bool TryGetBytesFromNewlineDelimiter(IByteBuffer input, int payloadSize, string packetSignature, out byte[] value)
        {
            value = null;

            if (input.ReadableBytes < payloadSize + 2)
            {
                return false;
            }

            if (payloadSize == 0)
            {
                if (ProtocolConstants.NEWLINES_CR == input.ReadByte() && ProtocolConstants.NEWLINES_LF == input.ReadByte())
                {
                    value = new byte[0];
                    return true;
                }
#if DEBUG
                throw new FormatException($"NATS protocol name of `{packetSignature}` is invalid.");
#else
                return false;
#endif
            }

            if (ProtocolConstants.NEWLINES_CR != input.GetByte(input.ReaderIndex + payloadSize) || ProtocolConstants.NEWLINES_LF != input.GetByte(input.ReaderIndex + payloadSize + 1))
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

        protected abstract ProtocolPacket DecodePacket(IByteBuffer buffer, string packetSignature, IChannelHandlerContext context);

    }

}