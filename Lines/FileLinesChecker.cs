//
// <copyright company="Softerra">
//    Copyright (c) Softerra, Ltd. All rights reserved.
// </copyright>
//
// <summary>
//    Makes search for the line in file
//    Executes any async requests in the new thread
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

    [Obsolete("Lines.FileLinesChecker has been deprecated." + 
        " Please use the Lines.FileLinesCheckerWithQueue class")]
    public class FileLinesChecker : FileLinesCheckerBase
    {

        #region constructors
        //
        // constructors
        //

        /// <summary>
        /// Creates an instance
        /// </summary>
        /// <param name="fileName">Name of file</param>
        public FileLinesChecker(String fileName) : base(fileName)
        {
        }

        #endregion

        #region FileLinesCheckerBase overrides
        //
        // FileLinesCheckerBase overrides
        //

        /// <summary>
        /// Start new thread for the async request execution
        /// </summary>
        /// <param name="line">Line for check</param>
        /// <param name="onSuccess">Executed after success check<</param>
        /// <param name="onFailure">Executed if check can not be processed</param>
        public override void ContainsAsync(String line, 
                                           Action<Boolean> onSuccess, 
                                           Action<String> onFailure)
        {
            // Creates new thread for the request processing 
            ThreadPool.QueueUserWorkItem(ProcessRequest,
                new AsyncRequest(line, onSuccess, onFailure));
        }

        #endregion

        #region methods
        //
        // methods
        //

        /// <summary>
        /// If instance can process request execute success callback
        /// otherwise execute failure callback
        /// </summary>
        /// <param name="object">AsyncRequest</param>
        private void ProcessRequest(Object @object)
        {
            AsyncRequest request = @object as AsyncRequest;

            lock (this.dataLocker)
            {
                // If instance waits for the new data
                if (this.state == FileLinesCheckerState.Pending)
                {
                    //Wait new data
                    Monitor.Wait(this.dataLocker);
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

        #endregion

    }

}
