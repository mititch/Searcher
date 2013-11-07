//
// <copyright company="Softerra">
//    Copyright (c) Softerra, Ltd. All rights reserved.
// </copyright>
//
// <summary>
//    Make search in specific storage
// </summary>
//
// <author email="mititch@softerra.com">Alex Mitin</author>
//
namespace LineSearchExec
{
    using System;
    using System.Threading.Tasks;

    public class Searcher
    {
        // Source for search
        private Source source;
        
        /// <summary>
        /// Creates an instance of Searcher
        /// </summary>
        /// <param name="source">Source for search</param>
        public Searcher(Source source)
        {
            this.source = source;
        }

        /// <summary>
        /// Calculate the count of lines in the source
        /// </summary>
        /// <param name="line">Line for search</param>
        /// <param name="checkReady">If true - check the source for ready</param>
        /// <returns>Count of lines</returns>
        /// <exception cref="MemberAccessException">Thrown if source not ready</exception>
        public Int32 GetLinesCount(String line, Boolean checkReady = false)
        {
            if (checkReady && this.source.State != Source.StorageState.Ready)
            {
                throw NewSourceNotReadyException();
            }
            return this.source.GetLinesCountInSource(line);

        }
        
        /// <summary>
        /// Calculate the count of lines in the source
        /// </summary>
        /// <param name="line">Line for search</param>
        /// <param name="checkReady">If true - check the source for ready</param>
        /// <returns>Task which represent the result of search</returns>
        /// <exception cref="MemberAccessException">Thrown if source not ready</exception>
        public Task<Int32> GetLinesCountAsync(String line, Boolean checkReady = false)
        {
            Task<Int32> task;
            if (this.source.State == Source.StorageState.Ready)
            {
                // Create task in ready state with result
                TaskCompletionSource<Int32> taskSource = new TaskCompletionSource<Int32>();
                
                taskSource.SetResult(this.FindLineCountInSource(line));
                
                task = taskSource.Task;
            }
            else
            {
                if (checkReady)
                {
                    throw NewSourceNotReadyException();
                }
                
                task = Task<Int32>.Factory.StartNew(FindLineCountInSource, line);
            }
            return task;
        }

        /// <summary>
        /// Creates a MemberAccessException instance
        /// </summary>
        /// <returns>MemberAccessException</returns>
        private MemberAccessException NewSourceNotReadyException()
        {
            return new MemberAccessException("Storage is not ready.");
        }

        private int FindLineCountInSource(Object o)
        {
            return source.GetLinesCountInSource((String)o);
        }
    }
}
