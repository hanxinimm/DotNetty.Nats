using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Hunter.NATS.Client
{
    // Provides a Channel<T> implementation that only allows a single call to 'get'
    // Used for ping-pong
    // Instance methods are not thread safe, however, this class is not intended
    // to be shared by any more than a single producer and single consumer.
    internal sealed class SingleUseChannel<T>
    {
        static readonly ConcurrentBag<SingleUseChannel<T>> Channels
            = new ConcurrentBag<SingleUseChannel<T>>();

        readonly ManualResetEventSlim e = new ManualResetEventSlim();
        volatile bool hasValue = false;
        T actualValue;

        // Get an existing unused SingleUseChannel from the pool,
        // or create one if none are available.
        // Thread safe.
        public static SingleUseChannel<T> GetOrCreate()
        {
            SingleUseChannel<T> item;
            if (Channels.TryTake(out item))
            {
                return item;
            }

            return new SingleUseChannel<T>();
        }

        // Return a SingleUseChannel to the internal pool.
        // Thread safe.
        public static void Return(SingleUseChannel<T> ch)
        {
            ch.reset();
            if (Channels.Count < 1024) Channels.Add(ch);
        }

        internal T get(int timeout)
        {
            while (!hasValue)
            {
                if (timeout < 0)
                {
                    e.Wait();
                }
                else
                {
                    if (!e.Wait(timeout))
                        throw new NATSTimeoutException();
                }
            }

            return actualValue;
        }

        internal void add(T value)
        {
            actualValue = value;
            hasValue = true;
            e.Set();
        }

        internal void reset()
        {
            hasValue = false;
            actualValue = default(T);
            e.Reset();
        }
    }
}
