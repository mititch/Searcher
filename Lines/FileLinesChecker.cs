namespace Lines
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using System.IO;
    using System.Collections;

    public class FileLinesChecker : FileLinesCheckerBase
    {

        public FileLinesChecker(String fileName) : base(fileName)
        {
        }

        public override void ContainsAsync(string line, Action<bool> onSuccess, Action<string> onFailure)
        {
            // Creates new thread to the request processing 
            ThreadPool.QueueUserWorkItem(ProcessRequest, new AsyncRequest(line, onSuccess, onFailure));
        }

        private void ProcessRequest(Object @object)
        {
            AsyncRequest request = @object as AsyncRequest;

            lock (dataLocker)
            {
                // If instance waits for new data
                if (this.state == FileLinesCheckerState.Pending)
                {
                    //Wait new data
                    Monitor.Wait(dataLocker);
                }

                // If instance can return the answer
                if (this.state == FileLinesCheckerState.Ready)
                {
                    // Execite success callback 
                    request.SuccessCallback(this.data.Contains(request.Line));
                }
                else
                {
                    // Execite failure callback 
                    request.FailureCallback(this.state.ToString());
                }

            }

        }

    }

}
