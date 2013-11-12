﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Lines
{
    class Program
    {
        /// <summary>
        /// File name
        /// </summary>
        private const String FILE_NAME = "C:/Users/mititch/Downloads/bf1.txt";

        /// <summary>
        /// Common part of any line
        /// </summary>
        private const String SOME_STRING =
            "have very many outstanding loans but I do need to consolidate and move ";

        private static Random random = new Random();

        static String GetSomeLine()
        {
            return String.Format("{0} {1}", SOME_STRING, random.Next(200));
        }

        static void AsyncRequest(ILinesChecker checker)
        {
            checker.ContainsAsync(GetSomeLine(), ShowSuccessResultFromAsync, ShowFailureResultFromAsync);
        }

        static void AsyncSyncRequest(ILinesChecker checker)
        {

            ThreadPool.QueueUserWorkItem(x => { SyncRequest(checker); });

            checker.ContainsAsync(GetSomeLine(), ShowSuccessResultFromAsync, ShowFailureResultFromAsync);
        }

        static void SyncRequest(ILinesChecker checker)
        {
            try
            {
                Console.WriteLine("Sync Ok - {0} - {1}", Thread.CurrentThread.ManagedThreadId, checker.Contains(GetSomeLine()));
            }
            catch (Exception exception)
            {
                Console.WriteLine("Sync Fail - {0} - {1}", Thread.CurrentThread.ManagedThreadId, exception.Message);
            }

        }

        static void ShowSuccessResultFromAsync(Boolean result)
        {
            Console.WriteLine("Async Ok - {0} - {1}", Thread.CurrentThread.ManagedThreadId, result);
        }

        static void ShowFailureResultFromAsync(String result)
        {
            Console.WriteLine("Async Fail - {0} - {1}", Thread.CurrentThread.ManagedThreadId, result);
        }

        static void ResetAsync(ILinesChecker checker)
        {
            ThreadPool.QueueUserWorkItem(x => {
                Console.WriteLine("  Reset requested!");    
                checker.Reset();
            });
        }

        static void CancelAsync(ILinesChecker checker)
        {
            ThreadPool.QueueUserWorkItem(x => {
                Console.WriteLine("  Cancel requested!");
                checker.Cancel(); 
            });
        }

        static void Main(String[] args)
        {

            RandomTest();

            Console.ReadLine();
        }

        static void RandomTest()
        {
            ILinesChecker checker = new FileLinesCheckProcessor(FILE_NAME);

            for (int i = 0; i < 50; i++)
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
                                AsyncSyncRequest(checker);
                            }
                            else
                            {
                                AsyncRequest(checker);
                            }
                        }
                        break;
                }
            }  
        }

        static void Test()
        {
            ILinesChecker checker = new FileLinesCheckProcessor(FILE_NAME);

            Console.WriteLine("  5 Async Requested");
            for (int i = 0; i < 5; i++)
            {
                AsyncRequest(checker);
            }
            Console.WriteLine("  5 Sync Requested");
            for (int i = 0; i < 5; i++)
            {
                SyncRequest(checker);
            }
            Console.WriteLine("  Cancel Requested");
            checker.Cancel();
            Console.WriteLine("  5 Async Requested");
            for (int i = 0; i < 5; i++)
            {
                AsyncRequest(checker);
            }
            Console.WriteLine("  5 Async Requested");
            for (int i = 0; i < 5; i++)
            {
                AsyncRequest(checker);
            }
            Console.WriteLine("  5 Sync Requested");
            for (int i = 0; i < 5; i++)
            {
                SyncRequest(checker);
            }

            Console.WriteLine("  Reset Requested");
            checker.Reset();
            Console.WriteLine("  5 Async Requested");
            for (int i = 0; i < 5; i++)
            {
                AsyncRequest(checker);
            }
            Console.WriteLine("  5 Sync Requested");
            for (int i = 0; i < 5; i++)
            {
                SyncRequest(checker);
            }


        }


    }
}
