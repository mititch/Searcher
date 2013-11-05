using System.Threading;

namespace BigFile.Core
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    public class Tuner
    {
        private Int32 locksCount;

        private Checker checker;

        private String secondLine;

        private Result result;
        
        private String searchLine;

        private Hashtable hashtableRef = null;

        public Tuner(Checker checker, String searchLine, Result result)
        {
            this.checker = checker;
            this.result = result;
            this.searchLine = searchLine;
        }

        public void SetFirst(Hashtable hashtable)
        {
            // чекер готов принимать тюнинг
            this.hashtableRef = hashtable;
            this.CheckTuning();
        }

        public void SetSecond(String secondLine)
        {
            // вторая часть строки получена
            this.secondLine = secondLine;
            this.CheckTuning();
        }

        private void CheckTuning()
        {
            if (Interlocked.Increment(ref this.locksCount) > 1)
            {
                //оба условия сработали
                checker.Tune(secondLine, searchLine, result, hashtableRef);
                this.hashtableRef = null; // now hastable has no roots
            }
        }

    }
}
