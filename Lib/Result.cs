﻿//
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
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading;

    public class Result : IDisposable
    {
        private readonly CancellationTokenSource tokenSource = new CancellationTokenSource();

        private readonly String searchLine;

        private Boolean isAlive = false;

        private Int32 value;

        /// <summary>
        /// Greate instance of Result class
        /// </summary>
        /// <param name="searchLine">Search text</param>
        internal Result(string searchLine)
        {
            this.searchLine = searchLine;
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
        /// Cancel search execution
        /// </summary>
        public void Cancel()
        {
            this.isAlive = false;
            tokenSource.Cancel();
        }

        /// <summary>
        /// Try to get CancellationToken
        /// </summary>
        /// <param name="token">CancellationToken object</param>
        /// <returns>True if token can be returned</returns>
        internal Boolean TryGetToken(out CancellationToken token)
        {
            token = isAlive ? tokenSource.Token : CancellationToken.None;
            
            return this.isAlive;
        }

        /// <summary>
        /// Increasing the value of the result on one
        /// </summary>
        internal void Increace()
        {
            this.Increace(1);
        }

        /// <summary>
        /// Increasing the value of the result on increment value
        /// </summary>
        /// <param name="increment">Value increment</param>
        internal void Increace(Int32 increment)
        {
            Interlocked.Add(ref this.value, increment);
        }

        /// <summary>
        /// Releases instance resources
        /// </summary>
        public void Dispose()
        {
            this.isAlive = false;
            tokenSource.Dispose();
        }

    }
}
