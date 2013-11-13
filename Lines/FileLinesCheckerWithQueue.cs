//
// <copyright company="Softerra">
//    Copyright (c) Softerra, Ltd. All rights reserved.
// </copyright>
//
// <summary>
//    Makes search for the line in file
//    Uses queue as the async requests storage    
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
            Dispose(true);
            // Prevent the destructor from being called
            GC.SuppressFinalize(this);
        }

        #endregion

        /// <summary>
        /// Check is line contains in the file asynchronously
        /// </summary>
        /// <param name="line">Line for check</param>
        /// <param name="onSuccess">Executed after success check<</param>
        /// <param name="onFailure">Executed if check can not be processed</param>
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

                    // Empty task instruct worker for stop working 
                    EnqueueTask(null);
                }
                // Always release or cleanup (any) unmanaged resources
            }

            this.disposed = true;
        }

        /// <summary>
        /// Add new task to async requests queue
        /// </summary>
        /// <param name="task">Async request parameters</param>
        private void EnqueueTask(AsyncRequest task)
        {
            lock (this.taskQueueLocker)
            {
                // Add task to queue
                this.tasksQueue.Enqueue(task);
                
                // Notify worker about new request
                Monitor.Pulse(this.taskQueueLocker);
            }

        }

        /// <summary>
        ///  Executes a request callcack in the new thread
        /// </summary>
        /// <param name="request"></param>
        private void ProcessAsyncRequest(AsyncRequest request)
        {
            lock (this.dataLocker)
            {
                // If instance is waits for new data
                if (this.state == FileLinesCheckerState.Pending)
                {
                    Monitor.Wait(this.dataLocker);
                }

                // If instance can return the answer
                if (this.state == FileLinesCheckerState.Ready)
                {
                    // Request success callback execution
                    ThreadPool.QueueUserWorkItem((result) => request.SuccessCallback((Boolean)result),
                        this.data.Contains(request.Line));
                }
                else
                {
                    // Request failure callback execution
                    ThreadPool.QueueUserWorkItem((result) => request.FailureCallback((String)result),
                        this.state.ToString());
                }
            }
        }

        /// <summary>
        /// Checks is queue contains request and process him
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
                    }

                    // Get task from queue
                    task = tasksQueue.Dequeue();
                }
                
                if (task != null)
                {
                    // If task exist process him
                    ProcessAsyncRequest(task);
                }
                else
                {
                    // Empty task instruct for stop working
                    return;
                }

            }

        }

    }

}
