using DotNetty.Buffers;
using System;
using System.Collections.Generic;
using System.Text;

namespace DotNetty.Codecs.Protocol
{
    public static class ByteBufferExtensions
    {
        public static byte SafeReadByte(this IByteBuffer byteBuffer)
        {
            if (byteBuffer.IsReadable()) return byteBuffer.ReadByte();
            return byte.MinValue;
        }
    }
}
