//
// <copyright company="Softerra">
//    Copyright (c) Softerra, Ltd. All rights reserved.
// </copyright>
//
// <summary>
//    Search for random lines count in file
// </summary>
//
// <author email="mititch@softerra.com">Alex Mitin</author>
//
namespace LineSearchExec
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Threading;

    public class Source : IDisposable
    {
        //
        // fields
        //

        // Used for locks in user mode
        private Int32 u_lock;

        // Used for locks in core mode
        private AutoResetEvent c_lock = new AutoResetEvent(false);

        // Storage of data
        private IDictionary<string, int> storage;

        // Track whether Dispose has been called.
        private Boolean disposed = false;

        private StorageState state = StorageState.NotReady;

        // Name of file
        private readonly String filename;

        //
        // constructors
        //

        /// <summary>
        /// Creates an instance of Source class
        /// </summary>
        /// <param name="filename">Name of file</param>
        public Source(String filename)
        {
            this.filename = filename;
        }

        //
        // IDisposable implementation
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

        //
        // properties
        //

        // Get info about source ready state
        public StorageState State
        {
            get
            {
                return this.state;
            }
        }

        //
        // methods
        //

        /// <summary>
        /// Prepere source to use
        /// </summary>
        /// <returns></returns>
        public Boolean PrepereSource()
        {
            // Only one thread can enter
            if (Interlocked.Exchange(ref u_lock, 1) == 0)
            {
                // Fill source
                try
                {
                    // Fill storage with file data
                    this.Fill(filename);
                    // Set instance state to ready
                    this.state = StorageState.Ready;
                }
                catch (Exception exception)
                {
                    throw NewParsingFileException(exception);
                }
                finally
                {
                    // Set instance state to broken
                    this.state = StorageState.Broken;
                    // Release lock
                    c_lock.Set();
                }
            }
            else
            {
                // Lock other threads
                c_lock.WaitOne();
                // Throw exception if storage state become broken while lock
                if (this.state == StorageState.Broken)
                {
                    throw NewParsingFileException(null);
                }
            }

            return this.State == StorageState.Ready;
        }


        /// <summary>
        /// Return line count in source
        /// </summary>
        /// <param name="line">line to compare</param>
        /// <returns>Count of lines</returns>
        public int GetLinesCountInSource(String line)
        {
            if (this.State != StorageState.Ready)
            {
                this.PrepereSource();
            }

            // Return result of line count search
            return storage.ContainsKey(line) ? this.storage[line] : 0;
        }

        ///<summary>
        /// Central method for cleaning up resources
        ///</summary>
        protected virtual void Dispose(Boolean disposing)
        {
            // Check to see if Dispose has already been called.
            if (!this.disposed)
            {
                // If explicit is true, then this method was called through the
                // public Dispose()
                if (disposing)
                {
                    // Release or cleanup managed resources
                    c_lock.Dispose();
                }
                // Always release or cleanup (any) unmanaged resources
            }

            this.disposed = true;
        }

        /// <summary>
        /// Fill storage by file data
        /// </summary>
        /// <param name="filename">Name of file</param>
        private void Fill(String filename)
        {
            //Creates new dictionary
            IDictionary<string, int> cache = new Dictionary<string, int>();
            
            using (StreamReader reader = new StreamReader(filename))
            {
                while (!reader.EndOfStream)
                {
                    String line = reader.ReadLine();

                    if (cache.ContainsKey(line))
                    {
                        // if cache already contains key increase it value
                        cache[line] = cache[line] + 1;
                    }
                    else
                    {
                        // Add new element to dictionary
                        cache.Add(line, 1);
                    }
                }

            }

            // Set storage object
            this.storage = cache;
        }

        /// <summary>
        /// Creates Exception about file parcing crash
        /// </summary>
        /// <param name="exception">Inner exception</param>
        /// <returns>New exception</returns>
        private Exception NewParsingFileException(Exception exception)
        {
            return new Exception("Error in file parsing.", exception);
        }

        //
        // nested types
        //

        /// <summary>
        /// Representetion of Storage object state
        /// </summary>
        public enum StorageState
        {
            Ready,
            NotReady,
            Broken
        }

        //
        // destructor
        //

        ~Source()
        {
            // Since other managed objects are disposed automatically, we
            // should not try to dispose any managed resources (see Rule 5`114).
            // We therefore pass false to Dispose()
            Dispose(false);
        }
    }
}
