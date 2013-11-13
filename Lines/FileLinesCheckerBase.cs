//
// <copyright company="Softerra">
//    Copyright (c) Softerra, Ltd. All rights reserved.
// </copyright>
//
// <summary>
//    Abstract class with basic functionality of checking lines in file
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

        // Helper object for the data access lock
        protected readonly Object dataLocker = new Object();

        // Name of file
        private String fileName;

        // Contains link to the LinesReader object, which reads the file now
        private LinesReader linesReader;

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
        /// <param name="fileName">Name of file</param>
        protected FileLinesCheckerBase(String fileName)
        {
            // Save file name
            this.fileName = fileName;
            
            // Request storage data updating
            ThreadPool.QueueUserWorkItem(LoadData);
        }

        #endregion

        #region methods
        //
        // methods
        //

        /// <summary>
        /// After call of this methods all existing and new requests 
        /// will be thrown or returned as canceled
        /// </summary>
        public void Cancel()
        {
            // Lock the data access
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
        /// <param name="line">Line for check</param>
        /// <returns>True if the line exist in file</returns>
        /// <exception cref="InvalidOperationException">
        /// Thrown if inctance can not porocess request</exception>
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

                // Check the state
                if (this.state != FileLinesCheckerState.Ready)
                {
                    // Request can be processed only in Ready state
                    // Throw exception
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

        /// <summary>
        /// Creates new LinesReader object for the storage updating
        /// Sent notification to the other threads if update was not canceled
        /// </summary>
        /// <param name="notUsed">Not used parameter</param>
        private void LoadData(Object notUsed)
        {
            LinesReader currentReader = new LinesReader();

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

                // Save link to the current reader for cancel
                this.linesReader = currentReader;
            }

            // Get new data from LinesReader
            IDictionary newData = null;
            try
            {
                using (Stream stream = new FileStream(this.fileName,
                    FileMode.Open, FileAccess.Read))
                {
                    // Load data
                    newData = currentReader.Read(stream);
                }
            }
            catch (Exception)
            {
                // TODO: Log exception
                // An unhandled exception causes to the program crach
            }

            // If process was not canseled - update the data
            lock (this.dataLocker)
            {
                // If the load process was not canceled or changed with another one
                if (!currentReader.IsCanceled)
                {
                    // If cancelation of the instance work was not requested
                    if (this.state != FileLinesCheckerState.Canceled)
                    {
                        // Renew the data and status
                        this.data = newData;
                        this.state = this.data == null 
                            ? FileLinesCheckerState.Error 
                            : FileLinesCheckerState.Ready;
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
        private InvalidOperationException NewInvalidOperationException(
            FileLinesCheckerState state)
        {
            return new InvalidOperationException(
                    String.Format("Can not process request. " + 
                    "Instanse state is {1}. ManagedThreadId={0}",
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
            // Some error occurred with the storage update
            Error,
            // In this state all requests will be canceled
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
            public AsyncRequest(String line,
                                Action<Boolean> sucessCallback,
                                Action<String> failureCallback)
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
