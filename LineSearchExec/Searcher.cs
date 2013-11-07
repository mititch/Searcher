//
// <copyright company="Softerra">
//    Copyright (c) Softerra, Ltd. All rights reserved.
// </copyright>
//
// <summary>
//    Make search in storage
// </summary>
//
// <author email="mititch@softerra.com">Alex Mitin</author>
//
namespace LineSearchExec
{
    using System;
    using System.CodeDom;
    using System.Threading.Tasks;

    public class Searcher
    {
        private Source source;
        
        public Searcher(Source source)
        {
            this.source = source;
        }

        public Int32 GetLinesCount(String line, Boolean checkReady = false)
        {
            if (checkReady && source.State != Source.StorageState.Ready)
            {
                throw NewSourceNotReadyException();
            }
            return source.GetLinesCountInSource(line);

        }

        public Task<Int32> GetLinesCountAsync(String line, Boolean checkReady = false)
        {
            Task<Int32> task;
            if (source.State == Source.StorageState.Ready)
            {
                var taskSource = new TaskCompletionSource<Int32>();
                taskSource.SetResult(this.FindLineCountInSource(line));
                task = taskSource.Task;
            }
            else
            {
                if (checkReady)
                {
                    throw NewSourceNotReadyException();
                }
                task = Task<Int32>.Factory.StartNew(FindLineCountInSource, line);
            }
            return task;
        }

        private MemberAccessException NewSourceNotReadyException()
        {
            return new MemberAccessException("Storage is not ready.");
        }

        private int FindLineCountInSource(object o)
        {
            return source.GetLinesCountInSource((string)o);
        }
    }
}
