//
// <copyright company="Softerra">
//    Copyright (c) Softerra, Ltd. All rights reserved.
// </copyright>
//
// <summary>
//    Represent result of the search.
// </summary>
//
// <author email="mititch@softerra.com">Alex Mitin</author>
//
namespace Lib
{
    using System;
    using System.Threading;

    public class Result : IDisposable
    {
        private Boolean isAlive = true;

        private readonly String searchLine;
        
        private readonly CancellationTokenSource tokenSource = 
            new CancellationTokenSource();

        private Int32 value = 0;

        /// <summary>
        /// Creates an instance of Result class
        /// </summary>
        /// <param name="searchLine">Search text</param>
        internal Result(string searchLine)
        {
            this.searchLine = searchLine;
        }

        /// <summary>
        /// Releases instance resources
        /// </summary>
        public void Dispose()
        {
            this.isAlive = false;
            tokenSource.Dispose();
        }

        public String SearchLine
        {
            get { return this.searchLine; }
        }

        public int Value
        {
            get { return value; }
        }

        /// <summary>
        /// Cancel of search execution
        /// </summary>
        public void Cancel()
        {
            this.isAlive = false;
            tokenSource.Cancel();
        }

        /// <summary>
        /// Increasing of the result value on one
        /// </summary>
        internal void Increace()
        {
            this.Increace(1);
        }

        /// <summary>
        /// Increasing of the result value on increment value
        /// </summary>
        /// <param name="increment">Increment value</param>
        internal void Increace(Int32 increment)
        {
            Interlocked.Add(ref this.value, increment);
        }

        /// <summary>
        /// Trying to get CancellationToken
        /// </summary>
        /// <param name="token">CancellationToken object</param>
        /// <returns>True if token can be returned</returns>
        internal Boolean TryGetToken(out CancellationToken token)
        {
            token = isAlive ? tokenSource.Token : CancellationToken.None;
            return this.isAlive;
        }

    }
}
