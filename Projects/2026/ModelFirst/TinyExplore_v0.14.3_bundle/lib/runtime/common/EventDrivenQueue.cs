// written by Roman Kiss, January 1, 2026
// version: 1.0.0.0  01/01/2026  initial version
//
//

using System;
using System.Collections;
using System.Diagnostics;
using System.Threading;

namespace NFAppLib
{
    /// <summary>
    /// Thread-safe, event-driven queue for .NET nanoFramework.
    /// Enqueue signals a worker thread, which drains items and raises OnDequeue.
    /// </summary>
    public class EventDrivenQueue : Queue, IDisposable
    {
        public enum QueueMode { AutoDrain, Manual }

        private readonly AutoResetEvent _signal = new AutoResetEvent(false);
        private readonly Thread _worker;
        private readonly QueueMode _mode;
        private bool _stopping;

        /// <summary> Raised when a new item is retrieved from the queue (worker thread). </summary>
        public event GetItemHandler OnGetItem;

        /// <summary> Raised if OnGetItem handler throws an exception. </summary>
        public event ErrorHandler OnError;

        /// <summary> Raised when queue count changes (after enqueue or dequeue). </summary>
        public event CountChangedHandler OnCountChanged;


        public string Name { get; private set; }

        // Delegetes
        public delegate void GetItemHandler(object source, object item, int count);
        public delegate void ErrorHandler(object source, object item, Exception ex);
        public delegate void CountChangedHandler(object source, int count);

        public EventDrivenQueue(string name = "Queue", QueueMode mode = QueueMode.AutoDrain)
        {
            Name = name ?? "Queue";
            _mode = mode;
            _worker = new Thread(WorkerLoop) { Priority = ThreadPriority.Normal };
            _worker.Start();
        }

        public int Enqueue(object item, string prompt = "")
        {
            if (_stopping)
                throw new ObjectDisposedException(nameof(EventDrivenQueue));

            int countAfter;
            lock (SyncRoot)
            {
                base.Enqueue(item);
                countAfter = base.Count;
                Debug.WriteLine($"📨 [{Name}] {prompt}Enqueued, wfq={countAfter}");
            }

            OnCountChanged?.Invoke(this, countAfter);

            if (_mode == QueueMode.AutoDrain)
            {
                _signal.Set(); // RX mode: start worker immediately
            }

            return countAfter;
        }

        public int EnqueueRange(object[] items, string prompt = "")
        {
            if (_stopping)
                throw new ObjectDisposedException(nameof(EventDrivenQueue));

            int countAfter;

            lock (SyncRoot)
            {
                for (int i = 0; i < items.Length; i++)
                {
                    base.Enqueue(items[i]);
                }
                countAfter = base.Count;
                Debug.WriteLine($"📨… [{Name}] {prompt}Batch enqueued, wfq={base.Count}");
            }

            OnCountChanged?.Invoke(this, countAfter);

            if (_mode == QueueMode.AutoDrain)
            {
                _signal.Set();
            }

            return countAfter;
        }

        /// <summary>
        /// Manual trigger (TX mode). Returns true if signal was set.
        /// </summary>
        public bool FireOneIf(bool bCondition = true) => _mode == QueueMode.Manual && Count > 0 && bCondition && _signal.Set();

        public bool WaitForFire() => _mode == QueueMode.Manual && _signal.Reset();

        private void WorkerLoop()
        {
            while (!_stopping)
            {
                _signal.WaitOne();
                if (_stopping) break;

                do
                {
                    object item = null;
                    int countAfter = 0;

                    lock (SyncRoot)
                    {
                        if (Count > 0)
                        {
                            item = base.Dequeue();
                            countAfter = base.Count;
                        }
                        else
                        {
                            break; // nothing to process
                        }
                    }

                    if (item != null)
                    {
                        try
                        {
                            // Notify count change first
                            OnCountChanged?.Invoke(this, countAfter);
                        }
                        catch { }
                        
                        try
                        {
                            // Process the item
                            Debug.WriteLine($"📤 [{Name}] Got item, wfq={countAfter}");
                            OnGetItem?.Invoke(this, item, countAfter);
                        }
                        catch (Exception ex)
                        {
                            OnError?.Invoke(this, item, ex);
                        }
                    }

                    // TX mode: process only 1 item per FireOne()
                    if (_mode == QueueMode.Manual)
                        break;

                } while (Count > 0);
            }
        }

        public void Stop()
        {
            if (_stopping) return;
            _stopping = true;
            _signal.Set(); // wake the worker so it can exit
        }

        public void Dispose()
        {
            Stop();
            // nanoFramework doesn't guarantee Thread.Join(); if available, you can call it.
            // Otherwise, give the worker a moment to exit.
            Thread.Sleep(10);
        }
    }
}
