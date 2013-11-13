//
// <copyright company="Softerra">
//    Copyright (c) Softerra, Ltd. All rights reserved.
// </copyright>
//
// <summary>
//    Abstract class with basic functionality ....
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
    using System.Collections;
    using System.Threading;
    using System.IO;

    public abstract class FileLinesCheckerBase 
    {
        #region fields
        //
        // fields
        //
        
        // Existing lines storage
        protected IDictionary data;

        // Helper object for threads locks which needed in data access
        protected readonly Object dataLocker = new Object();

        // Name of file
        protected String fileName;

        // Contains link to reader object which readed file data now
        protected LinesReader linesReader;

        // Instance state
        protected FileLinesCheckerState state = FileLinesCheckerState.Canceled;

        #endregion

        #region constructors
        //
        // constructors
        //
        
        /// <summary>
        /// Base constructor 
        /// </summary>
        /// <param name="fileName"></param>
        protected FileLinesCheckerBase(String fileName)
        {
            this.fileName = fileName;
            
            // Request storage data updating
            ThreadPool.QueueUserWorkItem(LoadData);
        }

        #endregion

        #region ILinesChecker implementation
        //
        // ILinesChecker implementation
        //

        /// <summary>
        /// Cancel exising and new requests execution
        /// </summary>
        public void Cancel()
        {
            lock (this.dataLocker)
            {
                // If stete is pending
                if (this.state == FileLinesCheckerState.Pending)
                {
                    // Cancel the previous reader execution
                    this.linesReader.Cancel();
                }

                // Change the state
                this.state = FileLinesCheckerState.Canceled;

                // Remove reader link from roots 
                this.linesReader = null;

                // Notify waiting threads about state changing
                Monitor.PulseAll(this.dataLocker);
            }

        }

        /// <summary>
        /// Checks is line contains in the file
        /// </summary>
        /// <param name="line"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException">Thrown if inctance can not porocess request</exception>
        public Boolean Contains(String line)
        {
            Boolean result;

            lock (this.dataLocker)
            {
                // If instance waits for new data
                if (this.state == FileLinesCheckerState.Pending)
                {
                    // Wait new data
                    Monitor.Wait(this.dataLocker);
                }

                // Final check
                if (this.state != FileLinesCheckerState.Ready)
                {
                    throw NewInvalidOperationException(this.state);
                }

                // Calculate the result
                result = this.data.Contains(line);
            }

            return result;

        }

        /// <summary>
        /// Implementation must check is line contains in the file asynchronously
        /// </summary>
        /// <param name="line">Line for check</param>
        /// <param name="onSuccess">Executed after success check</param>
        /// <param name="onFailure">Executed if check can not be processed</param>
        public abstract void ContainsAsync(String line,
                                           Action<Boolean> onSuccess,
                                           Action<String> onFailure);

        /// <summary>
        /// Request storage update
        /// </summary>
        public void Reset()
        {
            // Load data in enother thread
            ThreadPool.QueueUserWorkItem(LoadData);
        }

        #endregion

        #region methods
        //
        // methods
        //

        /// <summary>
        /// Instantiated new LinesReader object for the storage updating
        /// Sent notification to the other threads if update was not canceled
        /// </summary>
        /// <param name="notUsed">Not used parameter</param>
        private void LoadData(Object notUsed)
        {
            LinesReader threadReader = new LinesReader();

            // Stop previous LinesReader execution
            lock (this.dataLocker)
            {
                if (this.state == FileLinesCheckerState.Pending)
                {
                    // If State is pending - reader is exist
                    // Cancel previous reader execution
                    this.linesReader.Cancel();
                }
                else
                {
                    this.state = FileLinesCheckerState.Pending;
                }

                // Save link to the current reader
                this.linesReader = threadReader;
            }

            // Get new data from LinesReader
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
                // An unhandled exception causes to the program crach
            }

            // If process was not canseled - update the data
            lock (dataLocker)
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
                    Monitor.PulseAll(this.dataLocker);
                }
            }

        }

        /// <summary>
        /// Return new InvalidOperationException
        /// </summary>
        /// <returns>New InvalidOperationException</returns>
        /// <param name="state">Instance state</param>
        private InvalidOperationException NewInvalidOperationException(FileLinesCheckerState state)
        {
            return new InvalidOperationException(
                    String.Format("Can not process request. Instanse state is {1}. ManagedThreadId={0}",
                    Thread.CurrentThread.ManagedThreadId,
                    state));
        }

        #endregion

        #region nested types
        //
        // nested types
        //

        /// <summary>
        /// Represents FileLinesChecker instance state
        /// </summary>
        protected enum FileLinesCheckerState
        {
            // Data storage update in progress
            Pending,
            // Data storage is ready
            Ready,
            // Some error with storage update
            Error,
            // All requests will be canceled
            Canceled
        }

        /// <summary>
        /// Incapsulates the async requests data
        /// </summary>
        protected class AsyncRequest
        {

            // Stores the success callback
            private Action<Boolean> successCallback;

            // Stores the failure callback
            private Action<String> failureCallback;

            // Store the requested line
            private String line;

            /// <summary>
            /// Creates an instance
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

            /// <summary>
            /// Provides access to the success callback
            /// </summary>
            public Action<Boolean> SuccessCallback
            {
                get { return this.successCallback; }
            }

            /// <summary>
            /// Provides access to the failure callback
            /// </summary>
            public Action<String> FailureCallback
            {
                get { return this.failureCallback; }
            }

            /// <summary>
            /// Provides access to the requested line
            /// </summary>
            public String Line
            {
                get { return this.line; }
            }

        }

        #endregion

    }
}
