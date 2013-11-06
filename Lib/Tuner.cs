//
// <copyright company="Softerra">
//    Copyright (c) Softerra, Ltd. All rights reserved.
// </copyright>
//
// <summary>
//    Check of the last line in block. Configures Checker object
// </summary>
//
// <author email="mititch@softerra.com">Alex Mitin</author>
//
namespace Lib
{
    using System;
    using System.Collections;
    using System.Threading;

    internal class Tuner 
    {
        private readonly Checker checker;

        private Hashtable hashtableReferance;

        private Int32 locksCount;

        private readonly Result result;

        private String secondLine;

        /// <summary>
        /// Creates an instance of Tuner class
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
        /// <param name="hashtable">Referece to Checker hashtable</param>
        internal void RequestTune(Hashtable hashtable)
        {
            // Checker ready for tune
            this.hashtableReferance = hashtable;
            this.CheckTuning();
        }

        /// <summary>
        /// One of two parts of tune process.
        /// Save second substring of tune string.
        /// </summary>
        /// <param name="secondLine">Second part of tune string</param>
        internal void SetTuner(String secondLine)
        {
            // The second part of the line obtained
            this.secondLine = secondLine;
            this.CheckTuning();
        }

        /// <summary>
        /// If both conditions are ready make tune of the Checker object
        /// </summary>
        private void CheckTuning()
        {
            if (Interlocked.Increment(ref this.locksCount) > 1)
            {
                this.checker.Tune(secondLine, result, hashtableReferance);
                this.hashtableReferance = null;
                //Now hastable has no roots
            }
        }

    }
}
