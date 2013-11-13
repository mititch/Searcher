//
// <copyright company="Softerra">
//    Copyright (c) Softerra, Ltd. All rights reserved.
// </copyright>
//
// <summary>
//    Instance can read lines from stream
//    and generate dictionary which contains existing lines
// </summary>
//
// <author email="mititch@softerra.com">Alex Mitin</author>
//
namespace Lines
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using System.IO;
    using System.Collections;

    public class FileLinesCheckerWithQueue : FileLinesCheckerBase, IDisposable
    {

        // Track whether Dispose has been called.
        private Boolean disposed = false;
        
        // Helper object for threads locks which needed in taskQueue access
        private object taskQueueLocker = new object();

        // Async requests queue
        Queue<AsyncRequest> tasksQueue = new Queue<AsyncRequest>();
        
        /// <summary>
        /// Creates an instance 
        /// </summary>
        /// <param name="fileName">Name of file</param>
        public FileLinesCheckerWithQueue(String fileName) : base(fileName)
        {
            // Create and run new worker thread
            ThreadPool.QueueUserWorkItem(ProcessRequests);
            
        }

        ~FileLinesCheckerWithQueue()
        {
            Dispose(false);
        }


        #region IDispose implementation
        //
        // IDispose implementation
        //

        ///<summary>
        ///Implementation of the IDisposable interface
        ///</summary>
        public void Dispose()
        {
            // Call internal Dispose(bool)
            this.Dispose(true);
            // Prevent the destructor from being called
            GC.SuppressFinalize(this);
        }

        #endregion



        /// <summary>
        /// Check line exist async
        /// </summary>
        /// <param name="line">Line to check</param>
        /// <param name="onSuccess"></param>
        /// <param name="onFailure"></param>
        public override void ContainsAsync(string line,
                                           Action<bool> onSuccess,
                                           Action<string> onFailure)
        {
            // Add task to the execution queue
            EnqueueTask(new AsyncRequest(line, onSuccess, onFailure));
        }


        ///<summary>
        /// Central method for cleaning up resources
        ///</summary>
        protected virtual void Dispose(Boolean disposing)
        {
            // Check to see if Dispose has already been called.
            if (!this.disposed)
            {
                // If disposing is true, then this method was called through the
                // public Dispose()
                if (disposing)
                {
                    // Set all request as canceled, stop data loading
                    Cancel();

                    // Notify worker thread for stop working
                    lock (taskQueueLocker) 
                    {
                        Monitor.Pulse(taskQueueLocker);
                    }
                }
                // Always release or cleanup (any) unmanaged resources
            }

            // This effect worker loop
            this.disposed = true;
            //Monitor.Pulse(taskQueueLocker);
        }


        /// <summary>
        /// Add new task to async requests queue
        /// </summary>
        /// <param name="task">Async request parameters</param>
        private void EnqueueTask(AsyncRequest task)
        {
            lock (taskQueueLocker)
            {
                // Add task to queue
                tasksQueue.Enqueue(task);
                
                // Notify worker about new request
                Monitor.Pulse(taskQueueLocker);
            }

        }

        /// <summary>
        /// Execute request callcack in new thread
        /// </summary>
        /// <param name="request"></param>
        private void ProcessAsyncRequest(AsyncRequest request)
        {
            lock (dataLocker)
            {
                // If instance waits for new data
                if (this.state == FileLinesCheckerState.Pending)
                {
                    //Wait new data
                    Monitor.Wait(dataLocker);
                }

                // If instance can return the answer
                if (this.state == FileLinesCheckerState.Ready)
                {
                    // Request callback execution
                    ThreadPool.QueueUserWorkItem((result) => request.SuccessCallback((Boolean)result),
                        this.data.Contains(request.Line));
                }
                else
                {
                    // Request callback execution
                    ThreadPool.QueueUserWorkItem((result) => request.FailureCallback((String)result),
                        this.state.ToString());
                }
            }
        }

        /// <summary>
        /// Checks is queue contains request and process it
        /// </summary>
        private void ProcessRequests(Object notUsed)
        {
            // Execute while object is not disposed
            while (!this.disposed) 
            {
                AsyncRequest task = null;

                // Lock task queue
                lock (taskQueueLocker)
                {
                    // If no requests is queue wait for task
                    if (tasksQueue.Count == 0)
                    {
                        Monitor.Wait(taskQueueLocker);

                        // If object was disposed release thread
                        if (this.disposed)
                        {
                            return;
                        }
                    
                    }

                    // Get task from queue
                    task = tasksQueue.Dequeue();
                }

                // If task exist process him
                if (task != null)
                {
                    ProcessAsyncRequest(task);
                }

            }

        }

    }

}
