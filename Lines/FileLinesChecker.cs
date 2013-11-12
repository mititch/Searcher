﻿namespace Lines
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using System.IO;
    using System.Collections;

    public class FileLinesChecker : ILinesChecker
    {

        private IDictionary data;

        private readonly Object locker = new Object();

        private String fileName;

        private LinesReader linesReader;

        private FileLinesCheckerState state = FileLinesCheckerState.Canceled;

        public FileLinesChecker(String fileName)
        {
            this.fileName = fileName;

            // Prepare the data
            Reset();
        }

        public void Cancel()
        {
            lock (locker)
            {
                // If stete is pending
                if (this.state == FileLinesCheckerState.Pending)
                {
                    // Cancel the previous reader execution
                    linesReader.Cancel(); 
                }

                // Change the state
                this.state = FileLinesCheckerState.Canceled;

                // Remove reader link from roots 
                this.linesReader = null;

                // Notify waiting threads about state changing
                Monitor.PulseAll(locker);
            }

        }

        public Boolean Contains(String line)
        {
            Boolean result;

            lock (locker)
            {
                // If instance waits for new data
                if (this.state == FileLinesCheckerState.Pending)
                {
                    // Wait new data
                    Monitor.Wait(locker);
                }

                // Final check
                if (this.state != FileLinesCheckerState.Ready)
                {
                    throw new Exception(String.Format("Except - {0}", this.state));
                }

                // Calculate the result
                result = this.data.Contains(line);
            }

            return result;

        }

        public void ContainsAsync(String line, Action<Boolean> onSuccess, Action<String> onFailure)
        {
            // Creates new thread to the request processing 
            ThreadPool.QueueUserWorkItem(ProcessRequest, new AsyncRequest(line, onSuccess, onFailure));
        }

        public void Reset()
        {
            // Load data in enother thread
            ThreadPool.QueueUserWorkItem(LoadData);
        }

        private void LoadData(Object notUsed)
        {
            
            LinesReader threadReader = new LinesReader();

            // Reset instance reader and update instance state
            lock (locker)
            {
                if (this.state == FileLinesCheckerState.Pending)
                {
                    // If State is pending - reader is exist
                    // Cancel previous reader execution
                    linesReader.Cancel();
                }
                else
                {
                    this.state = FileLinesCheckerState.Pending;
                }

                // Save link to the current reader
                this.linesReader = threadReader;
            }

            IDictionary newData = null;

            try
            {
                using (Stream stream = new FileStream(this.fileName, FileMode.Open, FileAccess.Read))
                {
                    // Load data
                    newData = threadReader.Read(stream);
                }
            }
            catch (Exception)
            {
                // TODO: Log exception
                // An unhandled exception causes to the program
            }

            lock (locker)
            {
                // If the load process was not canceled or changed with another one
                if (!threadReader.IsCanceled)
                {
                    // If cancelation of the instance work was not requested
                    if (this.state != FileLinesCheckerState.Canceled)
                    {
                        // Renew the data and status
                        this.data = newData;
                        this.state = this.data == null ? FileLinesCheckerState.Error : FileLinesCheckerState.Ready;
                    }
                    
                    // Reader not needed more
                    this.linesReader = null;

                    // Notify other threads about data is available
                    Monitor.PulseAll(locker);
                }
            }

        }

        private void ProcessRequest(Object @object)
        {
            AsyncRequest request = @object as AsyncRequest;

            lock (locker)
            {
                // If instance waits for new data
                if (this.state == FileLinesCheckerState.Pending)
                {
                    //Wait new data
                    Monitor.Wait(locker);
                }

                // If instance can return the answer
                if (this.state == FileLinesCheckerState.Ready)
                {
                    // Execite success callback 
                    request.SuccessCallback(this.data.Contains(request.Line));
                }
                else
                {
                    // Execite failure callback 
                    request.FailureCallback(this.state.ToString());
                }

            }

        }

        /// <summary>
        /// Represents FileLinesChecker instance state
        /// </summary>
        private enum FileLinesCheckerState
        {
            Pending,
            Ready,
            Error,
            Canceled
        }

        /// <summary>
        /// Incapsulates async request data
        /// </summary>
        private class AsyncRequest
        {
            
            // Stores the success callback
            private Action<Boolean> successCallback;

            // Stores the failure callback
            private Action<String> failureCallback;

            // Store the requested line
            private String line;

            /// <summary>
            /// 
            /// </summary>
            /// <param name="line">Line for check</param>
            /// <param name="sucessCallback">Success callback delegate</param>
            /// <param name="failureCallback">Failure callback delegate</param>
            public AsyncRequest(String line, Action<Boolean> sucessCallback, Action<String> failureCallback)
            {
                this.line = line;
                this.successCallback = sucessCallback;
                this.failureCallback = failureCallback;
            }

            // Get success callback
            public Action<Boolean> SuccessCallback
            {
                get { return this.successCallback; }
            }

            // Get failure callback
            public Action<String> FailureCallback
            {
                get { return this.failureCallback; }
            }

            // Get requested line callback
            public String Line
            {
                get { return this.line; }
            }

        }
    }

}
