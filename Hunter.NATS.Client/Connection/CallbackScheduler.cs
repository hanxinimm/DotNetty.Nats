using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Hunter.NATS.Client
{
    // One could use a task scheduler, but this is simpler and will
    // likely be easier to port to .NET core.
    internal class CallbackScheduler : IDisposable
    {
        Channel<Task> tasks = new Channel<Task>() { Name = "Tasks" };
        Task executorTask = null;
        object runningLock = new object();
        bool schedulerRunning = false;

        private bool Running
        {
            get
            {
                lock (runningLock)
                {
                    return schedulerRunning;
                }
            }

            set
            {
                lock (runningLock)
                {
                    schedulerRunning = value;
                }
            }
        }

        private void process()
        {
            while (Running)
            {
                Task t = tasks.get(-1);
                try
                {
                    t.RunSynchronously();
                }
                catch (Exception) { }
            }
        }

        internal void Start()
        {
            lock (runningLock)
            {
                schedulerRunning = true;
                executorTask = new Task(() => { process(); });
                executorTask.Start();
            }
        }

        internal void Add(Task t)
        {
            lock (runningLock)
            {
                if (schedulerRunning)
                    tasks.add(t);
            }
        }

        internal void ScheduleStop()
        {
            Add(new Task(() =>
            {
                Running = false;
                tasks.close();
            }));
        }

        internal void WaitForCompletion()
        {
            try
            {
                executorTask.Wait(5000);
            }
            catch (Exception) { }
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
#if NET45
                    if (executorTask != null)
                        executorTask.Dispose();
#endif

                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }
        #endregion
    }
}
