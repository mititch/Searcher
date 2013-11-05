namespace BigFile.Core
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading;

    public class Result
    {
        object lockObj = new object();
        
        private Int32 value;

        private readonly CancellationTokenSource tokenSource;

        public int Value
        {
            get
            {
                return value;
            }
        }

        public Result()
        {
            tokenSource = new CancellationTokenSource();
        }

        public void Cancel()
        {
            tokenSource.Cancel();
        }

        public CancellationToken GetToken()
        {
            return tokenSource.Token;
        }

        public void Increace()
        {
            this.Increace(1);
        }

        public void Increace(Int32 increment)
        {
            Interlocked.Add(ref this.value, increment);
        }
     
    }
}
