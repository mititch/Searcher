namespace BigFile.Core
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
        private String fileName;
        
        private Int32 threadCount;

        private Checker[] checkers;

        public Searcher(String fileName, Int32 threadCount)
        {
            this.fileName = fileName;
            this.threadCount = threadCount;
            Int32 bufferSize;
            using (FileStream fs = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                bufferSize = Convert.ToInt32(Math.Round(Decimal.Divide(fs.Length, threadCount) + 1, MidpointRounding.ToEven));
            }

            checkers = new Checker[threadCount];
            for (int i = 0; i < threadCount; i++)
            {
                var offset = i * bufferSize;
                checkers[i] = new Checker(fileName, offset, bufferSize);
            }

        }

        public Result Search(String searchLine)
        {
           
            Result result = new Result();

            Task.Factory.StartNew(() =>
            {
                CancellationToken token = result.GetToken();
                Tuner tuner = null;
                for (int i = 0; i < threadCount; i++)
                {
                    if (token.IsCancellationRequested)
                    {
                        continue;
                    }
                    Checker currentChecker = checkers[i];
                    Tuner prevTuner = tuner;
                    tuner = currentChecker.Check(searchLine, result, prevTuner);
                }
                if (tuner != null && !token.IsCancellationRequested)
                {
                    tuner.SetSecond(string.Empty);
                }
            });
           
            return result;
        }

        public void Dispose()
        {
            foreach (Checker checker in checkers)
            {
                checker.Dispose();
            }
        }
    }
}
