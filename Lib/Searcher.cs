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
            
            // Calculating size of part
            Int32 bufferSize = Convert.ToInt32(
                Math.Round(Decimal.Divide(file.Length, partsCount),
                MidpointRounding.ToEven) + 1);

            this.checkers = new Checker[partsCount];
            
            for (Int32 i = 0; i < partsCount; i++)
            {
                this.checkers[i] = new Checker(fileName, i * bufferSize,
                    bufferSize);
            }

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

        /// <summary>
        /// Makes the search in the file
        /// </summary>
        /// <param name="searchLine">Search text</param>
        /// <returns>Reference to Result object</returns>
        public Result Search(String searchLine)
        {
           
            Result result = new Result(searchLine);

            CancellationToken token;
            if (result.TryGetToken(out token))
            {
                Task.Factory.StartNew(() =>
                {
                    //First checker has no previous checker with tuner
                    Tuner tuner = null;
                    for (Int32 i = 0; i < this.partsCount; i++)
                    {
                        if (token.IsCancellationRequested)
                        {
                            return;
                        }
                        Checker currentChecker = checkers[i];
                        Tuner prevTuner = tuner;
                        tuner = currentChecker.Check(prevTuner, result, token);
                    }

                    if (tuner != null)
                    {
                        //Second subline in empty for the last Tuner
                        tuner.SetTuner(String.Empty);
                    }

                }, token);
            }

            return result;
        }

    }
}
