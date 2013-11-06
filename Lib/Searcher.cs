//
// <copyright company="Softerra">
//    Copyright (c) Softerra, Ltd. All rights reserved.
// </copyright>
//
// <summary>
//    Search for a string in a file.
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
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;

    public class Searcher : IDisposable
    {
        private readonly Checker[] checkers;
        
        private readonly Int32 partsCount;

        /// <summary>
        /// Creates an instance of Searcher object
        /// </summary>
        /// <param name="fileName">File name</param>
        /// <param name="partsCount">Number of par for search</param>
        public Searcher(String fileName, Int32 partsCount)
        {
            this.partsCount = partsCount;

            FileInfo file = new FileInfo(fileName);
            
            Int32 bufferSize = Convert.ToInt32(
                Math.Round(Decimal.Divide(file.Length, partsCount)) + 1);

            this.checkers = new Checker[partsCount];
            
            for (Int32 i = 0; i < partsCount; i++)
            {
                this.checkers[i] = new Checker(fileName, i * bufferSize, bufferSize);
            }

        }

        /// <summary>
        /// Makes the search in the file
        /// </summary>
        /// <param name="searchLine">Search text</param>
        /// <returns>Reference to Result object</returns>
        public Result Search(String searchLine)
        {
           
            Result result = new Result(searchLine);

            Task.Factory.StartNew(() =>
            {
                CancellationToken token;
                if (!result.TryGetToken(out token))
                {
                    return;
                }

                //First checker has no previous checker with tuner
                Tuner tuner = null;
                for (Int32 i = 0; i < this.partsCount; i++)
                {
                    if (token.IsCancellationRequested)
                    {
                        continue;
                    }
                    Checker currentChecker = checkers[i];
                    Tuner prevTuner = tuner;
                    tuner = currentChecker.Check(prevTuner, result);
                }
                
                if (tuner != null)
                {
                    //Second subline in empty for the last Tuner
                    tuner.SetSecond(String.Empty);
                }
                
            });
           
            return result;
        }

        /// <summary>
        /// Releases instance resources
        /// </summary>
        public void Dispose()
        {
            foreach (Checker checker in this.checkers)
            {
                checker.Dispose();
            }
        }
    }
}
