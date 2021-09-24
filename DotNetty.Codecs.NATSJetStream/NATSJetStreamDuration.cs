using System;

namespace DotNetty.Codecs.NATSJetStream
{
    public sealed class NATSJetStreamDuration
    {
        const long NanosPerMilli = 1_000_000L;
        const long NanosPerSecond = 1_000_000_000L;
        const long NanosPerMinute = NanosPerSecond * 60L;
        const long NanosPerHour = NanosPerMinute * 60L;
        const long NanosPerDay = NanosPerHour * 24L;

        public static readonly NATSJetStreamDuration Zero = new NATSJetStreamDuration(0L);
        public static readonly NATSJetStreamDuration One = new NATSJetStreamDuration(1L);

        /// <summary>
        /// Gets the value of the duration in nanoseconds
        /// </summary>
        public long Nanos { get; }

        /// <summary>
        /// Gets the value of the duration in milliseconds, truncating any nano portion
        /// </summary>
        public int Millis => Convert.ToInt32(Nanos / NanosPerMilli);

        private NATSJetStreamDuration(long nanos)
        {
            Nanos = nanos;
        }

        /// <summary>
        /// Create a Duration from nanoseconds
        /// </summary>
        public static NATSJetStreamDuration OfNanos(long nanos)
        {
            return new NATSJetStreamDuration(nanos);
        } 

        /// <summary>
        /// Create a Duration from milliseconds
        /// </summary>
        public static NATSJetStreamDuration OfMillis(long millis)
        {
            return new NATSJetStreamDuration(millis * NanosPerMilli);
        } 

        /// <summary>
        /// Create a Duration from seconds
        /// </summary>
        public static NATSJetStreamDuration OfSeconds(long seconds)
        {
            return new NATSJetStreamDuration(seconds * NanosPerSecond);
        }

        /// <summary>
        /// Create a Duration from minutes
        /// </summary>
        public static NATSJetStreamDuration OfMinutes(long minutes)
        {
            return new NATSJetStreamDuration(minutes * NanosPerMinute);
        }

        /// <summary>
        /// Create a Duration from hours
        /// </summary>
        public static NATSJetStreamDuration OfHours(long hours)
        {
            return new NATSJetStreamDuration(hours * NanosPerHour);
        }

        /// <summary>
        /// Create a Duration from days
        /// </summary>
        public static NATSJetStreamDuration OfDays(long days)
        {
            return new NATSJetStreamDuration(days * NanosPerDay);
        }

        /// <summary>
        /// Is the value equal to 0
        /// </summary>
        /// <returns>true if value is 0</returns>
        public bool IsZero()
        {
            return Nanos == 0;
        }

        /// <summary>
        /// Is the value negative (less than zero)
        /// </summary>
        /// <returns>true if value is negative</returns>
        public bool IsNegative()
        {
            return Nanos < 0;
        }

        /// <summary>
        /// Is the value positive (greater than zero)
        /// </summary>
        /// <returns>true if value is positive</returns>
        public bool IsPositive()
        {
            return Nanos > 0;
        }
        
        public override bool Equals(object obj)
        {
            return Equals(obj as NATSJetStreamDuration);
        }

        private bool Equals(NATSJetStreamDuration other)
        {
            return other != null && Nanos == other.Nanos;
        }

        public override int GetHashCode()
        {
            return Nanos.GetHashCode();
        }

        public override string ToString()
        {
            return Nanos.ToString();
        }
    }
}