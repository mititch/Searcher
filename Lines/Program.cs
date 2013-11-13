//
// <copyright company="Softerra">
//    Copyright (c) Softerra, Ltd. All rights reserved.
// </copyright>
//
// <summary>
//    Tests the FileLinesCheckerWithQueue instance
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

    class Program
    {
        #region fields
        //
        // fields
        //

        // File name
        private const String FILE_NAME = "C:/Users/mititch/Downloads/bf1.txt";

        // Common part of any line in file
        private const String SOME_STRING =
            "have very many outstanding loans but I do need to consolidate and move ";

        #endregion

        #region helpers

        // Random number generator
        private static Random random = new Random();

        /// <summary>
        /// Prepares some line which can be (or not) in file
        /// </summary>
        /// <returns>Line</returns>
        private static String GetSomeLine()
        {
            return String.Format("{0} {1}", SOME_STRING, random.Next(200));
        }

        /// <summary>
        /// Makes async request to checker with random line from new thread
        /// </summary>
        /// <param name="checker">FileLinesCheckerBase</param>        
        private static void AsyncAsyncRequest(FileLinesCheckerBase checker)
        {
            ThreadPool.QueueUserWorkItem(AsyncRequest, checker);
        }

        /// <summary>
        /// Makes async request to checker with random line
        /// </summary>
        /// <param name="checker">FileLinesCheckerBase</param>
        private static void AsyncRequest(Object @object)
        {
            FileLinesCheckerBase checker = @object as FileLinesCheckerBase;    
            checker.ContainsAsync(GetSomeLine(),
                ShowSuccessResultFromAsync, ShowFailureResultFromAsync);
        }

        /// <summary>
        /// Makes sync request to checker with random line from new thread
        /// </summary>
        /// <param name="checker">FileLinesCheckerBase</param>        
        private static void AsyncSyncRequest(FileLinesCheckerBase checker)
        {
            ThreadPool.QueueUserWorkItem(SyncRequest, checker);
        }

        /// <summary>
        /// Makes sync request to checker with random line
        /// </summary>
        /// <param name="checker">FileLinesCheckerBase</param>
        private static void SyncRequest(Object @object)
        {
            FileLinesCheckerBase checker = @object as FileLinesCheckerBase;
            
            try
            {
                Console.WriteLine("  Sync done: Thread={0}, Result={1}",
                    Thread.CurrentThread.ManagedThreadId,
                    checker.Contains(GetSomeLine()));
            }
            catch (Exception exception)
            {
                Console.WriteLine("  Sync fail: Thread={0}, Exception={1}",
                    Thread.CurrentThread.ManagedThreadId,
                    exception.Message);
            }

        }

        /// <summary>
        /// Represent success callback
        /// </summary>
        /// <param name="result">FileLinesCheckerBase</param>
        private static void ShowSuccessResultFromAsync(Boolean result)
        {
            Console.WriteLine("  Async done: Thread={0}, Result={1}",
                Thread.CurrentThread.ManagedThreadId,
                result);
        }

        /// <summary>
        /// Represent failure callback
        /// </summary>
        /// <param name="result">FileLinesCheckerBase</param>
        private static void ShowFailureResultFromAsync(String result)
        {
            Console.WriteLine("  Async fail: Thread={0}, Reason={1}",
                Thread.CurrentThread.ManagedThreadId,
                result);
        }

        /// <summary>
        /// Call to FileLinesCheckerBase.Reset from new thread
        /// </summary>
        /// <param name="checker"></param>
        private static void ResetAsync(FileLinesCheckerBase checker)
        {
            ThreadPool.QueueUserWorkItem(ResetChecker, checker);
        }

        /// <summary>
        /// Call to FileLinesCheckerBase.Reset
        /// </summary>
        /// <param name="object">FileLinesCheckerBase as object</param>
        private static void ResetChecker(Object @object)
        {
            FileLinesCheckerBase checker = @object as FileLinesCheckerBase;
            Console.WriteLine("Reset call!");
            checker.Reset();
        }

        /// <summary>
        /// Call to FileLinesCheckerBase.Cancel from new thread
        /// </summary>
        /// <param name="checker">FileLinesCheckerBase</param>
        private static void CancelAsync(FileLinesCheckerBase checker)
        {
            ThreadPool.QueueUserWorkItem(CancelChecker, checker);
        }

        /// <summary>
        /// Call to FileLinesCheckerBase.Cancel
        /// </summary>
        /// <param name="object">FileLinesCheckerBase as object</param>
        private static void CancelChecker(Object @object)
        {
            FileLinesCheckerBase checker = @object as FileLinesCheckerBase;
            Console.WriteLine("Cancel call!");
            checker.Cancel();
        }

        #endregion

        #region test methods

        /// <summary>
        /// Makes 50 random calls to the FileLinesCheckerWithQueue methods
        /// All calls make from the different threads
        /// Waits 1 sec
        /// Dispose FileLinesCheckerWithQueue instance
        /// </summary>
        private static void RandomTest()
        {
            //TODO: ?
            using (FileLinesCheckerWithQueue checker 
                = new FileLinesCheckerWithQueue(FILE_NAME))
            {

                for (Int32 i = 0; i < 50; i++)
                {
                    switch (random.Next(10))
                    {
                        case 1:
                            ResetAsync(checker);
                            break;

                        case 2:
                            CancelAsync(checker);
                            break;

                        default:
                            {
                                if (i % 2 == 1)
                                {
                                    // Request from new thread
                                    AsyncSyncRequest(checker);
                                }
                                else
                                {
                                    // Async request from new thread
                                    AsyncAsyncRequest(checker);
                                }
                            }
                            break;
                    }
                }

                Thread.Sleep(1000);
            }
        }

        /// <summary>
        /// Makes 5 async requests
        /// Makes 5 sync requests
        /// Stop all requests with FileLinesCheckerWithQueue.Cancel call
        /// Makes 5 async requests
        /// Makes 5 sync requests
        /// Reset FileLinesCheckerWithQueue instance
        /// Makes 5 async requests
        /// Makes 5 sync requests
        /// Waits 1 sec
        /// Dispose FileLinesCheckerWithQueue instance
        /// </summary>
        private static void LineTest()
        {
            // TODO: ? 
            using (FileLinesCheckerWithQueue checker 
                = new FileLinesCheckerWithQueue(FILE_NAME))
            {

                Console.WriteLine("5 Async Requesting");
                for (Int32 i = 0; i < 5; i++)
                {
                    AsyncRequest(checker);
                }
                Console.WriteLine("5 Sync Requesting");
                for (Int32 i = 0; i < 5; i++)
                {
                    SyncRequest(checker);
                }

                CancelChecker(checker);

                Console.WriteLine("5 Async Requesting");
                for (Int32 i = 0; i < 5; i++)
                {
                    AsyncRequest(checker);
                }
                Console.WriteLine("5 Sync Requesting");
                for (Int32 i = 0; i < 5; i++)
                {
                    SyncRequest(checker);
                }

                ResetChecker(checker);

                Console.WriteLine("5 Async Requesting");
                for (Int32 i = 0; i < 5; i++)
                {
                    AsyncRequest(checker);
                }
                Console.WriteLine("5 Sync Requesting");
                for (Int32 i = 0; i < 5; i++)
                {
                    SyncRequest(checker);
                }

                Thread.Sleep(1000);

            }

        }

        #endregion

        /// <summary>
        /// Entrance method
        /// </summary>
        /// <param name="args"></param>
        private static void Main(String[] args)
        {

            RandomTest();
            //LineTest();

            Console.ReadLine();
        }

    }
}
