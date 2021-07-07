using System;
using System.Collections.Generic;
using System.Text;

namespace DotNetty.Codecs.NATSJetStream
{
    public class NATSJetStreamConvertTimeUnits
    {
        private static DateTime BaseTime = TimeZoneInfo.ConvertTimeToUtc(new DateTime(1970, 1, 1));

        /// <summary>   
        /// 将UnixTime转换为.NET的DateTime   
        /// </summary>   
        /// <param name="nanoseconds">秒数</param>   
        /// <returns>转换后的DateTime</returns>   
        public static DateTime ConvertToDateTime(long nanoseconds)
        {
            return new DateTime((nanoseconds / 100 + 8 * 60 * 60) + BaseTime.Ticks);
        }

        public static double ConvertMillisecondsToNanoseconds(double milliseconds)
        {
            // One million nanoseconds in one nanosecond.
            return milliseconds * 1000000;
        }
        public static double ConvertMicrosecondsToNanoseconds(double microseconds)
        {
            // One thousand microseconds in one nanosecond.
            return microseconds * 0.001;
        }

        public static double ConvertMillisecondsToMicroseconds(double milliseconds)
        {
            // One thousand milliseconds in one microsecond.
            return milliseconds * 1000;
        }

        public static double ConvertNanosecondsToMilliseconds(double nanoseconds)
        {
            // One million nanoseconds in one millisecond.
            return nanoseconds * 0.000001;
        }

        public static double ConvertMicrosecondsToMilliseconds(double microseconds)
        {
            // One thousand microseconds in one millisecond.
            return microseconds * 1000;
        }

        public static double ConvertNanosecondsToMicroseconds(double nanoseconds)
        {
            // One thousand nanoseconds in one microsecond.
            return nanoseconds * 1000;
        }
    }
}
