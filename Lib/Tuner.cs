//
// <copyright company="Softerra">
//    Copyright (c) Softerra, Ltd. All rights reserved.
// </copyright>
//
// <summary>
//    Check last line. Can tune Checker object
// </summary>
//
// <author email="mititch@softerra.com">Alex Mitin</author>
//
namespace Lib
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading;

    internal class Tuner 
    {
        private Int32 locksCount;

        private readonly Checker checker;

        private Hashtable hashtableRef;

        private readonly Result result;

        private String secondLine;

        /// <summary>
        /// Get instance of Tuner class
        /// </summary>
        /// <param name="checker">Checker object which should be tuned</param>
        /// <param name="result">Result object which collect search results</param>
        internal Tuner(Checker checker, Result result)
        {
            this.checker = checker;
            this.result = result;
        }

        /// <summary>
        /// One of two parts of tune process.
        /// Save root to hastable.
        /// </summary>
        /// <param name="hashtable">Referece to checker hashtable</param>
        internal void SetFirst(Hashtable hashtable)
        {
            // Checker ready for tune
            this.hashtableRef = hashtable;
            this.CheckTuning();
        }

        /// <summary>
        /// One of two parts of tune process.
        /// Save second substring of tune string.
        /// </summary>
        /// <param name="secondLine">Second part of tune string</param>
        internal void SetSecond(String secondLine)
        {
            // Second part of string avaible
            this.secondLine = secondLine;
            this.CheckTuning();
        }

        /// <summary>
        /// If both conditions are ready make tune of Checker object
        /// </summary>
        private void CheckTuning()
        {
            if (Interlocked.Increment(ref this.locksCount) > 1)
            {
                checker.Tune(secondLine, result, hashtableRef);
                this.hashtableRef = null;
                //Now hastable has no roots
            }
        }

    }
}
