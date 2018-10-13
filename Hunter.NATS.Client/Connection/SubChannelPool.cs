using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Hunter.NATS.Client
{
    /// <summary>
    /// The SubChannelPool class is used when the application
    /// has specified async subscribers will share channels and associated
    /// processing threads in the connection.  It simply returns a channel 
    /// that already has a long running task (thread) processing it.  
    /// Async subscribers use this channel in lieu of their own channel and
    /// message processing task.
    /// </summary>
    internal sealed class SubChannelPool : IDisposable
    {
        /// <summary>
        /// SubChannelProcessor creates a channel and a task to process
        /// messages on that channel.
        /// </summary>
        // TODO:  Investigate reuse of this class in async sub.
        private sealed class SubChannelProcessor : IDisposable
        {
            Channel<Message> channel = new Channel<Message>();
            NATSConnection connection = null;
            Task channelTask = null;

            internal SubChannelProcessor(NATSConnection c)
            {
                connection = c;

                channel.Name = "SubChannelProcessor " + this.GetHashCode();

                channelTask = new Task(() => {
                    //TODO:传递消息
                    //connection.deliverMsgs(channel);
                }, TaskCreationOptions.LongRunning);

                channelTask.Start();
            }

            internal Channel<Message> Channel
            {
                get { return channel; }
            }

            public void Dispose()
            {
                // closing the channel will end the task, but cap the
                // wait just in case things are slow.  
                // See Connection.deliverMsgs
                channel.close();
                channelTask.Wait(500);
#if NET45
                    channelTask.Dispose();
#endif
                channelTask = null;
            }
        }

        object pLock = new object();
        List<SubChannelProcessor> pList = new List<SubChannelProcessor>();

        int current = 0;
        int maxTasks = 0;

        NATSConnection connection = null;

        internal SubChannelPool(NATSConnection c, int numTasks)
        {
            maxTasks = numTasks;
            connection = c;
        }

        /// <summary>
        /// Gets a message channel for use with an async subscriber.
        /// </summary>
        /// <returns>
        /// A channel, already setup with a task processing messages.
        /// </returns>
        internal Channel<Message> getChannel()
        {
            // simple round robin, adding Channels/Tasks as necessary.
            lock (pLock)
            {
                if (current == maxTasks)
                    current = 0;

                if (pList.Count == current)
                    pList.Add(new SubChannelProcessor(connection));

                return pList[current++].Channel;
            }
        }

        public void Dispose()
        {
            lock (pLock)
            {
                pList.ForEach((p) => { p.Dispose(); });
                pList.Clear();
            }
        }
    }
}
