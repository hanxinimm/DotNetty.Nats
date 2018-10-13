using System;
using System.Collections.Generic;
using System.Text;

namespace Hunter.NATS.Client
{
    public class PublishProtocol
    {

        // Prepare protocol messages for efficiency
        public readonly static byte[] PING_P_BYTES ;
        public readonly static int PING_P_BYTES_LEN;

        public readonly static byte[] PONG_P_BYTES;
        public readonly static int PONG_P_BYTES_LEN;

        public readonly static byte[] PUB_P_BYTES;
        public readonly static int PUB_P_BYTES_LEN;

        public readonly static byte[] CRLF_BYTES;
        public readonly static int CRLF_BYTES_LEN;

        static PublishProtocol()
        {
            PING_P_BYTES = Encoding.UTF8.GetBytes(NATSConstants.pingProto);
            PING_P_BYTES_LEN = PING_P_BYTES.Length;

            PONG_P_BYTES = Encoding.UTF8.GetBytes(NATSConstants.pongProto);
            PONG_P_BYTES_LEN = PONG_P_BYTES.Length;

            PUB_P_BYTES = Encoding.UTF8.GetBytes(NATSConstants._PUB_P_);
            PUB_P_BYTES_LEN = PUB_P_BYTES.Length;

            CRLF_BYTES = Encoding.UTF8.GetBytes(NATSConstants._CRLF_);
            CRLF_BYTES_LEN = CRLF_BYTES.Length;
        }

        public static void BuildPublishProtocolBuffer(int size, byte[] publishProtocolBuffer)
        {
            //TODO:优化协议变量
            publishProtocolBuffer = new byte[size];
            Buffer.BlockCopy(PUB_P_BYTES, 0, publishProtocolBuffer, 0, PUB_P_BYTES_LEN);
        }

        // Ensures that pubProtoBuf is appropriately sized for the given
        // subject and reply.
        // Caller must lock.
        public static void EnsurePublishProtocolBuffer(string subject, string reply,byte[] publishProtocolBuffer)
        {
            // Publish protocol buffer sizing:
            //
            // PUB_P_BYTES_LEN (includes trailing space)
            //  + SUBJECT field length
            //  + SIZE field maximum + 1 (= log(2147483647) + 1 = 11)
            //  + (optional) REPLY field length + 1

            int pubProtoBufSize = PUB_P_BYTES_LEN
                                + (1 + subject.Length)
                                + (11)
                                + (reply != null ? reply.Length + 1 : 0);

            // only resize if we're increasing the buffer...
            if (pubProtoBufSize > publishProtocolBuffer.Length)
            {
                // ...and when we increase it up to the next power of 2.
                pubProtoBufSize--;
                pubProtoBufSize |= pubProtoBufSize >> 1;
                pubProtoBufSize |= pubProtoBufSize >> 2;
                pubProtoBufSize |= pubProtoBufSize >> 4;
                pubProtoBufSize |= pubProtoBufSize >> 8;
                pubProtoBufSize |= pubProtoBufSize >> 16;
                pubProtoBufSize++;

                BuildPublishProtocolBuffer(pubProtoBufSize, publishProtocolBuffer);
            }
        }

        // Use low level primitives to build the protocol for the publish
        // message.
        public static int WritePublishProtocolBuffer(string subject, string reply, int msgSize, byte[] publishProtocolBuffer)
        {
            // Skip past the predefined "PUB "
            int index = PUB_P_BYTES_LEN;

            // Subject
            index = WriteStringToBuffer(publishProtocolBuffer, index, subject);

            if (reply != null)
            {
                // " REPLY"
                publishProtocolBuffer[index] = (byte)' ';
                index++;

                index = WriteStringToBuffer(publishProtocolBuffer, index, reply);
            }

            // " "
            publishProtocolBuffer[index] = (byte)' ';
            index++;

            // " Size"
            index = WriteInt32ToBuffer(publishProtocolBuffer, index, msgSize);

            // "\r\n"
            publishProtocolBuffer[index] = CRLF_BYTES[0];
            publishProtocolBuffer[index + 1] = CRLF_BYTES[1];
            if (CRLF_BYTES_LEN > 2)
            {
                for (int i = 2; i < CRLF_BYTES_LEN; ++i)
                    publishProtocolBuffer[index + i] = CRLF_BYTES[i];
            }
            index += CRLF_BYTES_LEN;

            return index;
        }

        // Since we know we don't need to decode the protocol string,
        // just copy the chars into bytes.  This increased
        // publish peformance by 30% as compared to Encoding.
        static int WriteStringToBuffer(byte[] buffer, int offset, string value)
        {
            //TODO:优化减少逻辑判断
            //if(buffer == null)
            //    throw new ArgumentNullException("buffer");
            //if (offset < 0 || offset >= buffer.Length)
            //    throw new ArgumentOutOfRangeException("offset");

            for (int i = 0; i < value.Length; i++)
            {
                buffer[i + offset] = (byte)value[i];
            }
            return offset + value.Length;
        }

        static int WriteInt32ToBuffer(byte[] buffer, int offset, int value)
        {
            //TODO:优化减少逻辑判断
            //if(buffer == null)
            //    throw new ArgumentNullException("buffer");
            //if (offset < 0 || offset >= buffer.Length)
            //    throw new ArgumentOutOfRangeException("offset");


            const int ZERO = (ushort)'0';

            var idx = offset;
            var neg = value < 0;
            // Take care of sign
            var uvalue = neg ? (uint)(-value) : (uint)value;
            // Conversion. Number is reversed.
            do buffer[idx++] = (byte)(ZERO + (uvalue % 10)); while ((uvalue /= 10) != 0);
            if (neg) buffer[idx++] = (byte)'-';

            var length = idx - offset;
            // Reverse string
            Array.Reverse(buffer, offset, length);

            return offset + length;
        }
    }
}
