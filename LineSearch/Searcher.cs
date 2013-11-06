using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LineSearch
{
    using System.Collections;
    using System.Threading.Tasks;

    public class Searcher
    {
        private Source source;
        
        public delegate void ResultCallback(int result);

        public Searcher(String filename)
        {
            source = new Source(filename);
        }

        public Int32 FindLine(String line)
        {
            return source.GetValue(line);
        }

        public Task<Int32> FindLineAsync(String line)
        {

            Task<Int32> task;
            if (source.IsReady)
            {
                var taskSource = new TaskCompletionSource<Int32>();
                taskSource.SetResult(this.FindLine(line));
                task = taskSource.Task;
            }
            else
            {
                task = Task<Int32>.Factory.StartNew(FindLine, line);
            }
            return task;
        }

        private int FindLine(object o)
        {
            return FindLine((string) o);
        }
    }
}
