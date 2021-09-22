using DotNetty.Buffers;
using DotNetty.Transport.Channels;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;


namespace DotNetty.Codecs.Protocol
{

    public abstract class ZeroAllocationByteDecoder : ReplayingDecoder<ParseState>
    {
        private readonly ILogger _logger;

        public ZeroAllocationByteDecoder(ILogger logger)
            : base(ParseState.Ready)
        {
            _logger = logger;
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

        protected string GetSignature(IByteBuffer input)
        {
            int startIndex = input.ReaderIndex;
            for (int i = 0; input.ReadableBytes > 0; i++)
            {
                switch (input.ReadByte())
                {
                    case ProtocolConstants.FIELD_DELIMITER_SPACES:
                    case ProtocolConstants.FIELD_DELIMITER_TAB:
                        return input.GetString(startIndex, i, Encoding.UTF8);
                    case ProtocolConstants.NEW_LINES_CR:
                        if (ProtocolConstants.NEW_LINES_LF == input.SafeReadByte())
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
                                    _logger.LogWarning($"NATS Newlines is invalid.");
#endif
                                    return string.Empty;
                            }
                        }
#if DEBUG
                        _logger.LogWarning("NATS Newlines is invalid.");
#endif
                        break;
                    default:
                        break;
                }
            }
            return string.Empty;
        }

        protected bool TryGetStringFromFieldDelimiter(IByteBuffer input, string packetSignature, out string value)
        {
            value = null;
            int startIndex = input.ReaderIndex;
            for (int i = 0; input.ReadableBytes > 0; i++)
            {
                switch (input.ReadByte())
                {
                    case ProtocolConstants.FIELD_DELIMITER_SPACES:
                    case ProtocolConstants.FIELD_DELIMITER_TAB:
                        value = input.GetString(startIndex, i, Encoding.UTF8);
                        return true;
                    case ProtocolConstants.NEW_LINES_CR:
                    case ProtocolConstants.NEW_LINES_LF:
#if DEBUG
                        _logger.LogWarning($"[130]NATS protocol name of `{packetSignature}` is invalid. Text = ", input.ReadString(input.ReadableBytes, Encoding.UTF8));
#endif
                        return false;
                    default:
                        break;
                }
            }
            return false;
        }

        protected string GetStringFromFieldDelimiter(IByteBuffer input, string packetSignature)
        {
            int startIndex = input.ReaderIndex;
            for (int i = 0; input.ReadableBytes > 0; i++)
            {
                switch (input.ReadByte())
                {
                    case ProtocolConstants.FIELD_DELIMITER_SPACES:
                    case ProtocolConstants.FIELD_DELIMITER_TAB:
                        return input.GetString(startIndex, i, Encoding.UTF8);
                    case ProtocolConstants.NEW_LINES_CR:
                        if (ProtocolConstants.NEW_LINES_LF == input.SafeReadByte())
                        {
                            input.SetReaderIndex(startIndex);
                            return string.Empty;
                        }
#if DEBUG
                        _logger.LogWarning($"[157]NATS protocol name of `{packetSignature}` is invalid. Text = ", input.ReadString(input.ReadableBytes, Encoding.UTF8));
#endif
                        break;
                    default:
                        break;
                }
            }
            return string.Empty;
        }

        protected bool TryGetStringFromNewlineDelimiter(IByteBuffer input, string packetSignature, out string value)
        {
            value = null;
            int startIndex = input.ReaderIndex;
            for (int i = 0; input.ReadableBytes > 0; i++)
            {
                if (ProtocolConstants.NEW_LINES_CR == input.ReadByte())
                {
                    if (ProtocolConstants.NEW_LINES_LF == input.SafeReadByte())
                    {
                        value = input.GetString(startIndex, i, Encoding.UTF8);
                        return true;
                    }
#if DEBUG
                    _logger.LogWarning($"[181]NATS protocol name of `{packetSignature}` is invalid. Text = ", input.ReadString(input.ReadableBytes, Encoding.UTF8));
#endif
                    break;
                }
            }
            return false;
        }

        protected bool TryGetStringFromColonDelimiter(Span<byte> input, ref int startIndex, out string value)
        {
            value = null;
            for (int i = startIndex; input.Length > 0; i++)
            {
                if (ProtocolConstants.FIELD_DELIMITER_COLON == input[i])
                {
                    value = Encoding.UTF8.GetString(input.Slice(startIndex, i - startIndex).ToArray());

                    startIndex = i;

                    return true;
                }
            }
#if DEBUG
            _logger.LogWarning($"[181]NATS protocol name of `header` is invalid. Text = ", Encoding.UTF8.GetString(input.Slice(startIndex, input.Length - startIndex).ToArray()));
#endif

            return false;
        }

        protected bool TryGetStringFromNewlineDelimiter(Span<byte> input, ref int startIndex, out string value)
        {
            value = null;
            for (int i = startIndex; input.Length > 0; i++)
            {
                if (ProtocolConstants.NEW_LINES_LF == input[i])
                {
                    if (ProtocolConstants.NEW_LINES_CR == input[i - 1])
                    {
                        value = Encoding.UTF8.GetString(input.Slice(startIndex, i - startIndex - 1).ToArray());

                        startIndex = i;

                        return true;
                    }
                    else
                    {
#if DEBUG
                        _logger.LogWarning($"[181]NATS protocol name of `header` is invalid. Text = ", Encoding.UTF8.GetString(input.Slice(i - startIndex, i).ToArray()));
#endif

                        startIndex = i++;

                        break;
                    }
                }
            }
            return false;
        }

        protected bool TryGetBytesFromNewlineDelimiter(IByteBuffer input, int payloadSize, string packetSignature, out byte[] value)
        {
            value = null;

            if (input.ReadableBytes < payloadSize + 2)
            {
                return false;
            }

            if (payloadSize == 0)
            {
                if (ProtocolConstants.NEW_LINES_CR == input.ReadByte() && ProtocolConstants.NEW_LINES_LF == input.ReadByte())
                {
                    value = new byte[0];
                    return true;
                }
#if DEBUG
                _logger.LogWarning($"[206]NATS protocol name of `{packetSignature}` is invalid. Text = ", input.ReadString(input.ReadableBytes, Encoding.UTF8));
#endif
                return false;
            }

            if (ProtocolConstants.NEW_LINES_CR != input.GetByte(input.ReaderIndex + payloadSize) || ProtocolConstants.NEW_LINES_LF != input.GetByte(input.ReaderIndex + payloadSize + 1))
#if DEBUG
                _logger.LogWarning($"[213]NATS protocol name of `{packetSignature}` is invalid. Text = ", input.ReadString(input.ReadableBytes, Encoding.UTF8));
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