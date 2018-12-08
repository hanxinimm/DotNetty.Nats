// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace DotNetty.Codecs.STAN
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using DotNetty.Buffers;
    using DotNetty.Common.Utilities;
    using DotNetty.Transport.Channels;

    /// <summary>
    ///     A decoder that splits the received <see cref="DotNetty.Buffers.IByteBuffer" /> by one or more
    ///     delimiters.It is particularly useful for decoding the frames which ends
    ///     with a delimiter such as <see cref="DotNetty.Codecs.Delimiters.NullDelimiter" /> or
    ///     <see cref="DotNetty.Codecs.Delimiters.LineDelimiter" />
    ///     <h3>Specifying more than one delimiter </h3>
    ///     <see cref="DotNetty.Codecs.Delimiters.NullDelimiter" /> allows you to specify more than one
    ///     delimiter.  If more than one delimiter is found in the buffer, it chooses
    ///     the delimiter which produces the shortest frame.  For example, if you have
    ///     the following data in the buffer:
    ///     +--------------+
    ///     | ABC\nDEF\r\n |
    ///     +--------------+
    ///     a <see cref="DotNetty.Codecs.Delimiters.LineDelimiter" /> will choose '\n' as the first delimiter and produce two
    ///     frames:
    ///     +-----+-----+
    ///     | ABC | DEF |
    ///     +-----+-----+
    ///     rather than incorrectly choosing '\r\n' as the first delimiter:
    ///     +----------+
    ///     | ABC\nDEF |
    ///     +----------+
    /// </summary>
    /// <remarks>
    /// NATS uses CR followed by LF (CR+LF, \r\n, 0x0D0A) to terminate protocol messages. 
    /// This newline sequence is also used to mark the end of the message payload in a PUB or MSG protocol message.
    /// </remarks>
    public class STANDelimiterBasedFrameDecoder : ByteToMessageDecoder
    {
        /// Maximum length of a frame we're willing to decode.  
		private readonly int maxLength;

        /// Whether or not to throw an exception as soon as we exceed maxLength. 
        private readonly bool failFast;

        private readonly bool stripDelimiter;

        /// True if we're discarding input because we're already over maxLength.  
        private bool discarding;

        private int discardedBytes;

        /// <summary>
        ///     Initializes a new instance of the <see cref="T:DotNetty.Codecs.LineBasedFrameDecoder" /> class.
        /// </summary>
        /// <param name="maxLength">
        ///     the maximum length of the decoded frame.
        ///     A {@link TooLongFrameException} is thrown if
        ///     the length of the frame exceeds this value.
        /// </param>
        public STANDelimiterBasedFrameDecoder(int maxLength)
            : this(maxLength, false, false)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="T:DotNetty.Codecs.LineBasedFrameDecoder" /> class.
        /// </summary>
        /// <param name="maxLength">
        ///     the maximum length of the decoded frame.
        ///     A {@link TooLongFrameException} is thrown if
        ///     the length of the frame exceeds this value.
        /// </param>
        /// <param name="stripDelimiter">
        ///     whether the decoded frame should strip out the
        ///     delimiter or not
        /// </param>
        /// <param name="failFast">
        ///     If <tt>true</tt>, a {@link TooLongFrameException} is
        ///     thrown as soon as the decoder notices the length of the
        ///     frame will exceed <tt>maxFrameLength</tt> regardless of
        ///     whether the entire frame has been read.
        ///     If <tt>false</tt>, a {@link TooLongFrameException} is
        ///     thrown after the entire frame that exceeds
        ///     <tt>maxFrameLength</tt> has been read.
        /// </param>
        public STANDelimiterBasedFrameDecoder(int maxLength, bool stripDelimiter, bool failFast)
        {
            this.maxLength = maxLength;
            this.failFast = failFast;
            this.stripDelimiter = stripDelimiter;
        }

        protected override void Decode(IChannelHandlerContext context, IByteBuffer input, List<object> output)
        {
            try
            {

                DebugLogger.LogBaseMSG(input.GetString(input.ReaderIndex, input.ReadableBytes, System.Text.Encoding.UTF8));
                object obj = Decode(context, input);
                if (obj != null)
                {
                    DebugLogger.LogMSG(((IByteBuffer)obj).GetString(0, ((IByteBuffer)obj).ReadableBytes, System.Text.Encoding.UTF8));
                    output.Add(obj);
                }
            }
            catch (Exception ex)
            {

            }
        }

        /// <summary>
        ///     Create a frame out of the {@link ByteBuf} and return it.
        /// </summary>
        /// <param name="ctx">the {@link ChannelHandlerContext} which this {@link ByteToMessageDecoder} belongs to</param>
        /// <param name="buffer">the {@link ByteBuf} from which to read data</param>
        protected internal virtual object Decode(IChannelHandlerContext ctx, IByteBuffer buffer)
        {
            int num = FindEndOfLine(buffer);
            if (!discarding)
            {
                if (num >= 0)
                {
                    int num2 = num - buffer.ReaderIndex;
                    if (num2 > maxLength)
                    {
                        buffer.SetReaderIndex(num + 2);
                        Fail(ctx, num2);
                        return null;
                    }
                    IByteBuffer byteBuffer;
                    if (stripDelimiter)
                    {
                        byteBuffer = buffer.ReadSlice(num2);
                        buffer.SkipBytes(2);
                    }
                    else
                    {
                        byteBuffer = buffer.ReadSlice(num2 + 2);
                    }
                    return byteBuffer.Retain();
                }
                int readableBytes = buffer.ReadableBytes;
                if (readableBytes > maxLength)
                {
                    discardedBytes = readableBytes;
                    buffer.SetReaderIndex(buffer.WriterIndex);
                    discarding = true;
                    if (failFast)
                    {
                        Fail(ctx, string.Concat((object)"over ", (object)discardedBytes));
                    }
                }
                return null;
            }
            if (num >= 0)
            {
                int length = discardedBytes + num - buffer.ReaderIndex;
                int num4 = (buffer.GetByte(num) != 13) ? 1 : 2;
                buffer.SetReaderIndex(num + num4);
                discardedBytes = 0;
                discarding = false;
                if (!failFast)
                {
                    Fail(ctx, length);
                }
            }
            else
            {
                discardedBytes += buffer.ReadableBytes;
                buffer.SetReaderIndex(buffer.WriterIndex);
            }
            return null;
        }

        private void Fail(IChannelHandlerContext ctx, int length)
        {
            Fail(ctx, length.ToString());
        }

        private void Fail(IChannelHandlerContext ctx, string length)
        {
            ctx.FireExceptionCaught(new TooLongFrameException(string.Format("frame length ({0}) exceeds the allowed maximum ({1})", (object)length, (object)maxLength)));
        }

        private int FindEndOfLine(IByteBuffer buffer)
        {
            int num = buffer.ForEachByte(ByteProcessor.FindLF);
            if (num > 0 && buffer.GetByte(num - 1) == 13)
            {
                if (buffer.GetByte(buffer.ReaderIndex) == 77)
                {
                    return FindEndOfLine(buffer, num + 2, buffer.WriterIndex - (num + 2));
                }
                else
                {
                    return --num;
                }
            }
            return -1;
        }

        private int FindEndOfLine(IByteBuffer buffer, int startIndex, int length)
        {
            int num = buffer.ForEachByte(startIndex, length, ByteProcessor.FindLF);
            if (num > 0 && buffer.GetByte(num - 1) == 13)
            {
                return --num;
            }
            if (length > 0)
            {
                return FindEndOfLine(buffer, num + 1, buffer.WriterIndex - (num + 1));
            }
            return -1;
        }
    }
}