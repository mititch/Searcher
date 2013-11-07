//
// <copyright company="Softerra">
//    Copyright (c) Softerra, Ltd. All rights reserved.
// </copyright>
//
// <summary>
//    Methods return number of identical lines in file
// </summary>
//
// <author email="mititch@softerra.com">Alex Mitin</author>
//
namespace Strings
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Threading;

    class TaskBasedFileLinesCounter : ILinesCounter
    {

        #region fields
        //
        // fields
        //

        // Data storage
        private IDictionary<Int32, Int32> data;

        // Track whether Dispose has been called.
        private Boolean disposed = false;

        // Name of file
        private readonly String fileName;

        // Generates a hash code for dictionary items
        private readonly Func<String, Int32> hashCodeProvider;

        // Helper object for threads locks
        private readonly Object locker = new Object();

        // Instance state
        private LinesCounterState state = LinesCounterState.Created;

        // Instance of LinesReader 
        private LinesReader reader;

        #endregion

        #region constructors
        //
        // constructors
        //

        /// <summary>
        /// Creates an instance of FileLinesCounter class
        /// </summary>
        /// <param name="fileName">Name of file</param>
        /// <param name="hashCodeProvider">Hash code generator</param>
        public TaskBasedFileLinesCounter(String fileName, Func<String, Int32> hashCodeProvider)
        {
            this.hashCodeProvider = hashCodeProvider;

            this.fileName = fileName;
        }

        #endregion

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

        #region ILinesCounter implementation
        //
        // ILinesCounter implementation
        //

        /// <summary>
        /// <see cref="ILinesCounter.State"/>
        /// </summary>
        public LinesCounterState State
        {
            get
            {
                return this.state;
            }
        }

        /// <summary>
        /// <see cref="ILinesCounter.Cancel"/>
        /// </summary>
        public void Cancel()
        {
            this.Dispose(true);
        }

        /// <summary>
        /// <see cref="ILinesCounter.GetLinesCount"/>
        /// </summary>
        /// <exception cref="FieldAccessException">Thrown if data can not be read</exception>
        public int GetLinesCount(String line)
        {
            if (this.state != LinesCounterState.Ready)
            {
                this.state = LinesCounterState.Pending;
                this.LoadData(null);
            }

            //NOTE: return what?
            return this.disposed ? -1 : this.CheckLine(line);

        }

        /// <summary>
        /// <see cref="ILinesCounter.GetLinesCountAsync"/>
        /// </summary>
        public void GetLinesCountAsync(String line, Action<Int32> callback)
        {
            if (this.state == LinesCounterState.Ready)
            {
                callback(this.CheckLine(line));
            }
            else
            {
                ThreadPool.QueueUserWorkItem(this.GetLinesCountAsyncCallBack,
                    new AsyncCallBackState(line, callback));
            }
        }

        /// <summary>
        /// <see cref="ILinesCounter.TryGetLinesCount"/>
        /// </summary>
        public LinesCounterState TryGetLinesCount(String line, out Int32 result)
        {
            if (this.state == LinesCounterState.Created)
            {
                this.state = LinesCounterState.Pending;
                ThreadPool.QueueUserWorkItem(this.LoadData);
            }

            result = this.state == LinesCounterState.Ready ? this.CheckLine(line) : -1;

            return this.state;

        }

        #endregion

        #region methods
        //
        // methods
        //

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
                    if (this.reader != null)
                    {
                        this.reader.Cancel();
                    }
                }
                // Always release or cleanup (any) unmanaged resources
            }

            this.disposed = true;
        }

        /// <summary>
        /// Performs function adopted as parameter of GetLinesCountAsync method
        /// </summary>
        /// <param name="obj">Callback state</param>
        private void GetLinesCountAsyncCallBack(Object obj)
        {
            AsyncCallBackState callbackState = obj as AsyncCallBackState;

            if (!this.disposed)
            {
                callbackState.Action(this.GetLinesCount(callbackState.Line));
            }
        }

        /// <summary>
        /// Checks is line hashcode exist in data source
        /// </summary>
        /// <param name="line">Line</param>
        /// <returns>Count of lines in source</returns>
        private Int32 CheckLine(String line)
        {
            if (this.data == null)
            {
                throw this.NewInvalidOperationException();
            }

            Int32 hashCode = this.hashCodeProvider(line);

            return this.data.ContainsKey(hashCode) ? this.data[hashCode] : 0;
        }

        /// <summary>
        /// Reads data from file
        /// </summary>
        private void LoadData(Object obj)
        {
            lock (locker)
            {
                if (this.State != LinesCounterState.Ready)
                {
                    this.reader = new LinesReader(this.hashCodeProvider);

                    Stream stream = new FileStream(this.fileName, FileMode.Open);

                    this.data = this.reader.Read(stream);

                    this.reader = null;

                    //this.state = LinesCounterState.Ready;
                }
            }

            this.state = LinesCounterState.Ready;
        }

        /// <summary>
        /// Return new InvalidOperationException
        /// </summary>
        /// <returns>New InvalidOperationException</returns>
        /// <exception cref="InvalidOperationException">Thrown if data can not be read</exception>
        private InvalidOperationException NewInvalidOperationException()
        {
            return new InvalidOperationException(
                    String.Format("Instance of FileLinesCounter can not read data. ManagedThreadId={0}",
                    Thread.CurrentThread.ManagedThreadId));
        }

        #endregion

        #region nested types
        //
        // nested types
        //

        /// <summary>
        /// Encapsulates the callback request data
        /// </summary>
        private class AsyncCallBackState
        {
            private readonly Action<Int32> action;

            private readonly String line;

            public AsyncCallBackState(String line, Action<Int32> action)
            {
                this.action = action;

                this.line = line;
            }

            /// <summary>
            /// Callback action
            /// </summary>
            public Action<Int32> Action { get { return this.action; } }

            /// <summary>
            /// Request parameter
            /// </summary>
            public String Line { get { return this.line; } }
        }

        #endregion

        #region destructors

        ~FileLinesCounter()
        {
            Dispose(false);
        }

        #endregion

    }
}
