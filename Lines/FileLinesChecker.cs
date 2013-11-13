//
// <copyright company="Softerra">
//    Copyright (c) Softerra, Ltd. All rights reserved.
// </copyright>
//
// <summary>
//    Makes search for the line in file
//    Executes any async request in the new thread
// </summary>
//
// <author email="mititch@softerra.com">Alex Mitin</author>
//
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

        /// <summary>
        /// Creates an instance
        /// </summary>
        /// <param name="fileName">Name of file</param>
        public FileLinesChecker(String fileName) : base(fileName)
        {
        }

        /// <summary>
        /// Start new thread for the async request execution
        /// </summary>
        /// <param name="line">Line for check</param>
        /// <param name="onSuccess">Executed after success check<</param>
        /// <param name="onFailure">Executed if check can not be processed</param>
        public override void ContainsAsync(string line, Action<bool> onSuccess, Action<string> onFailure)
        {
            // Creates new thread for the request processing 
            ThreadPool.QueueUserWorkItem(ProcessRequest, new AsyncRequest(line, onSuccess, onFailure));
        }

        /// <summary>
        /// If instance can process request execute success callback
        /// otherwise execute failure callback
        /// </summary>
        /// <param name="object">AsyncRequest</param>
        private void ProcessRequest(Object @object)
        {
            AsyncRequest request = @object as AsyncRequest;

            lock (dataLocker)
            {
                // If instance waits for the new data
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
                    // Execute failure callback 
                    request.FailureCallback(this.state.ToString());
                }

            }

        }

    }

}
