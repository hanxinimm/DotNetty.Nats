using DotNetty.Codecs.Protocol;
using System;
using System.Collections.Generic;
using System.Text;

namespace DotNetty.Codecs.NATSJetStream
{
    public class NATSJetStreamValidator
    {
        public static string ValidateSubject(string value, bool required)
        {
            return ValidatePrintable(value, "Subject", required);
        }

        public static string ValidateReplyTo(string value, bool required)
        {
            return ValidatePrintableExceptWildGt(value, "Reply To", required);
        }

        public static string ValidateQueueName(string value, bool required)
        {
            return ValidatePrintableExceptWildDotGt(value, "Queue", required);
        }

        public static string ValidateStreamName(string value, bool required)
        {
            return ValidatePrintableExceptWildDotGt(value, "Stream", required);
        }

        public static string ValidateDurable(string value, bool required)
        {
            return ValidatePrintableExceptWildDotGt(value, "Durable", required);
        }

        public static string ValidateJetStreamPrefix(string value)
        {
            return ValidatePrintableExceptWildGtDollar(value, "Prefix", false);
        }

        public static string InternalValidate(string value, string label, bool required, Func<string, string, string> customValidate)
        {
            if (required)
            {
                if (string.IsNullOrEmpty(value))
                {
                    throw new ArgumentNullException($"{label} cannot be null or empty [{value}]");
                }
            }
            else if (string.IsNullOrEmpty(value))
            {
                return null;
            }

            return customValidate(value, label);
        }

        public static string ValidatePrintable(string value, string label, bool required)
        {
            return InternalValidate(value, label, required, (v, l) => {
                if (NotPrintable(value))
                {
                    throw new ArgumentOutOfRangeException($"{l} must be in the printable ASCII range [{v}]");
                }
                return value;
            });
        }

        public static string ValidatePrintableExceptWildDotGt(string value, string label, bool required)
        {
            return InternalValidate(value, label, required, (v, l) => {
                if (NotPrintableOrHasWildGtDot(value))
                {
                    throw new ArgumentOutOfRangeException($"{l} must be in the printable ASCII range and cannot include `*`, `.` or `>` [{v}]");
                }
                return value;
            });
        }

        public static string ValidatePrintableExceptWildGt(string value, string label, bool required)
        {
            return InternalValidate(value, label, required, (v, l) => {
                if (NotPrintableOrHasWildGt(value))
                {
                    throw new ArgumentOutOfRangeException($"{l} must be in the printable ASCII range and cannot include `*`, `>` or `$` [{v}]");
                }
                return v;
            });
        }

        public static string ValidatePrintableExceptWildGtDollar(string value, string label, bool required)
        {
            return InternalValidate(value, label, required, (v, l) =>
            {
                if (NotPrintableOrHasWildGtDollar(v))
                {
                    throw new ArgumentOutOfRangeException($"{l} must be in the printable ASCII range and cannot include `*`, `>` or `$` [{v}]");
                }
                return v;
            });
        }

        public static int ValidatePullBatchSize(int pullBatchSize)
        {
            if (pullBatchSize < 1 || pullBatchSize > NATSJetStreamConstants.MAX_PULL_SIZE)
            {
                throw new ArgumentOutOfRangeException($"Pull Batch Size must be between 1 and {NATSJetStreamConstants.MAX_PULL_SIZE} inclusive [{pullBatchSize}]");
            }
            return pullBatchSize;
        }

        public static long ValidateMaxConsumers(long max)
        {
            return ValidateGtZeroOrMinus1(max, "Max Consumers");
        }

        public static long ValidateMaxMessages(long max)
        {
            return ValidateGtZeroOrMinus1(max, "Max Messages");
        }

        public static long ValidateMaxBytes(long max)
        {
            return ValidateGtZeroOrMinus1(max, "Max Bytes");
        }

        public static long ValidateMaxMessageSize(long max)
        {
            return ValidateGtZeroOrMinus1(max, "Max message size");
        }

        public static int ValidateNumberOfReplicas(int replicas)
        {
            if (replicas < 1 || replicas > 5)
            {
                throw new ArgumentOutOfRangeException("Replicas must be from 1 to 5 inclusive.");
            }
            return replicas;
        }

        public static TimeSpan ValidateTimeSpanNotRequiredGtOrEqZero(TimeSpan? value)
        {
            if (value == null)
            {
                return TimeSpan.Zero;
            }
            if (value <= TimeSpan.Zero)
            {
                throw new ArgumentOutOfRangeException("TimeSpan must be greater than or equal to 0.");
            }
            return value.Value;
        }


        public static long ValidateGtZeroOrMinus1(long value, string label)
        {
            if (ZeroOrLtMinus1(value))
            {
                throw new ArgumentOutOfRangeException($"{label} must be greater than zero or -1 for unlimited");
            }
            return value;
        }

        public static long ValidateNotNegative(long value, string label)
        {
            if (value < 0)
            {
                throw new ArgumentOutOfRangeException($"{label} cannot be negative");
            }
            return value;
        }

        public static bool NotPrintable(string value)
        {
            for (int x = 0; x < value.Length; x++)
            {
                char c = value[x];
                if (c < 33 || c > 126)
                {
                    return true;
                }
            }
            return false;
        }

        public static bool NotPrintableOrHasChars(string value, char[] charsToNotHave)
        {
            for (int x = 0; x < value.Length; x++)
            {
                var c = value[x];
                if (c < 33 || c > 126)
                {
                    return true;
                }
                foreach (char cx in charsToNotHave)
                {
                    if (c == cx)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        static readonly char[] WILD_GT = { '*', '>' };
        static readonly char[] WILD_GT_DOT = { '*', '>', '.' };
        static readonly char[] WILD_GT_DOLLAR = { '*', '>', '$' };

        private static bool NotPrintableOrHasWildGt(string value)
        {
            return NotPrintableOrHasChars(value, WILD_GT);
        }

        private static bool NotPrintableOrHasWildGtDot(string value)
        {
            return NotPrintableOrHasChars(value, WILD_GT_DOT);
        }

        private static bool NotPrintableOrHasWildGtDollar(string value)
        {
            return NotPrintableOrHasChars(value, WILD_GT_DOLLAR);
        }

        public static bool ZeroOrLtMinus1(long value)
        {
            return value == 0 || value < -1;
        }
    }
}
