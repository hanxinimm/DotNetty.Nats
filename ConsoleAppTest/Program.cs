//using Hunter.NATS.Client;
using Microsoft.Extensions.Options;
using System;
using System.Diagnostics;
using System.Text;

namespace ConsoleAppTest
{
    class Program
    {
        private static IOptionsMonitor<Order> optionsMonitor;
        static void Main(string[] args)
        {

            
            //var testchar = new byte[20];
            //var lg3 = Int32ToBuffer(testchar, 1 , i);

            //var str = "3113123";
            //var val = Encoding.ASCII.GetBytes(str);
            //var testcharstr = new byte[20];
            //WriteStringToBuffer(testcharstr, 0, str);

            Console.WriteLine("Hello World!");
        }

        /// <summary>
        /// Returns a 64-bit signed integer converted from eight bytes at a specified position in a byte array.
        /// </summary>
        /// <param name="value">An array of bytes.</param>
        /// <param name="startIndex">The starting position within value.</param>
        /// <returns>A 64-bit signed integer formed by eight bytes beginning at startIndex.</returns>
        private static long ToInt64(byte[] value, int startIndex)
        {
            return CheckedFromBytes(value, startIndex, 8);
        }

        /// <summary>
        /// Checks the arguments for validity before calling FromBytes
        /// (which can therefore assume the arguments are valid).
        /// </summary>
        /// <param name="value">The bytes to convert after checking</param>
        /// <param name="startIndex">The index of the first byte to convert</param>
        /// <param name="bytesToConvert">The number of bytes to convert</param>
        /// <returns></returns>
        private static long CheckedFromBytes(byte[] value, int startIndex, int bytesToConvert)
        {
            CheckByteArgument(value, startIndex, bytesToConvert);
            return FromBytes(value, startIndex, bytesToConvert);
        }

        /// <summary>
        /// Checks the given argument for validity.
        /// </summary>
        /// <param name="value">The byte array passed in</param>
        /// <param name="startIndex">The start index passed in</param>
        /// <param name="bytesRequired">The number of bytes required</param>
        /// <exception cref="ArgumentNullException">value is a null reference</exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// startIndex is less than zero or greater than the length of value minus bytesRequired.
        /// </exception>
        private static void CheckByteArgument(byte[] value, int startIndex, int bytesRequired)
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }
            if (startIndex < 0 || startIndex > value.Length - bytesRequired)
            {
                throw new ArgumentOutOfRangeException(nameof(startIndex));
            }
        }

        /// <summary>
        /// Returns a value built from the specified number of bytes from the given buffer,
        /// starting at index.
        /// </summary>
        /// <param name="buffer">The data in byte array format</param>
        /// <param name="startIndex">The first index to use</param>
        /// <param name="bytesToConvert">The number of bytes to use</param>
        /// <returns>The value built from the given bytes</returns>
        private static long FromBytes(byte[] buffer, int startIndex, int bytesToConvert)
        {
            long ret = 0;
            for (int i = 0; i < bytesToConvert; i++)
            {
                ret = unchecked((ret << 8) | buffer[startIndex + i]);
            }
            return ret;
        }

        // A simple ToInt64.
        // Assumes: positive integers.
        static long ToInt64(byte[] buffer, int start, int end)
        {
            int length = end - start;
            switch (length)
            {
                case 0:
                    return 0;
                case 1:
                    return buffer[start] - '0';
                case 2:
                    return 10 * (buffer[start] - '0')
                         + (buffer[start + 1] - '0');
                case 3:
                    return 100 * (buffer[start] - '0')
                         + 10 * (buffer[start + 1] - '0')
                         + (buffer[start + 2] - '0');
                case 4:
                    return 1000 * (buffer[start] - '0')
                         + 100 * (buffer[start + 1] - '0')
                         + 10 * (buffer[start + 2] - '0')
                         + (buffer[start + 3] - '0');
                case 5:
                    return 10000 * (buffer[start] - '0')
                         + 1000 * (buffer[start + 1] - '0')
                         + 100 * (buffer[start + 2] - '0')
                         + 10 * (buffer[start + 3] - '0')
                         + (buffer[start + 4] - '0');
                case 6:
                    return 100000 * (buffer[start] - '0')
                         + 10000 * (buffer[start + 1] - '0')
                         + 1000 * (buffer[start + 2] - '0')
                         + 100 * (buffer[start + 3] - '0')
                         + 10 * (buffer[start + 4] - '0')
                         + (buffer[start + 5] - '0');
                default:
                    if (length < 0)
                        throw new ArgumentOutOfRangeException("end");
                    break;
            }

            long value = 0L;

            int i = start;
            while (i < end)
            {
                value *= 10L;
                value += (buffer[i++] - '0');
            }

            return value;
        }

        // Since we know we don't need to decode the protocol string,
        // just copy the chars into bytes.  This increased
        // publish peformance by 30% as compared to Encoding.
        private static int WriteStringToBuffer(byte[] buffer, int offset, string value)
        {
            int length = value.Length;
            int end = offset + length;

            for (int i = 0; i < length; i++)
            {
                buffer[i + offset] = (byte)value[i];
            }

            return end;
        }

        public static int Int32ToBuffer(byte[] buffer, int offset, int value, IFormatProvider formatProvider = null)
        {
            if (buffer == null)
                throw new ArgumentNullException("buffer");
            if (offset < 0 || offset >= buffer.Length)
                throw new ArgumentOutOfRangeException("start");


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

    public class Order
    {
        /// <summary>
        /// 用户ID
        /// </summary>
        public int AccountId { get; set; }
        /// <summary>
        /// 期数编号
        /// </summary>
        public int PeriodNo { get; set; }
        /// <summary>
        /// 订单号
        /// </summary>
        public string OrderNo { get; set; }
        /// <summary>
        /// 单注金额
        /// </summary>
        public decimal Amount { get; set; }
        /// <summary>
        /// 注数
        /// </summary>
        public int BetCount { get; set; }
        /// <summary>
        /// 金额单位（元角分）
        /// </summary>
        public int MoneyUnit { get; set; }
        /// <summary>
        /// 总金额
        /// </summary>
        public decimal TotalAmount { get; set; }
        /// <summary>
        /// 内容
        /// </summary>
        public string Content { get; set; }
        /// <summary>
        /// 游戏ID
        /// </summary>
        public int GameId { get; set; }
        /// <summary>
        /// 玩法ID
        /// </summary>
        public int PlayItemId { get; set; }
        /// <summary>
        /// 订单时间
        /// </summary>
        public DateTime CreateTime { get; set; }
    }


}
