using System;
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
        private const String WRONG_FILE_NAME = "C:/Users/mititch/Downloads/bf11.txt";

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


        static void Request(ILinesChecker checker)
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
            ThreadPool.QueueUserWorkItem(x => { checker.Reset(); });
        }

        static void Main(String[] args)
        {
            
            ILinesChecker checker = new FileLinesCheckProcessor(FILE_NAME);

            Console.WriteLine("Run async");
            for (int i = 0; i < 8; i++)
            {
                AsyncRequest(checker);
                if (i == 3) 
                {
                    for (int j = 0; j < 14; j++)
                    {
                        ResetAsync(checker);    
                    }

                }
            }
            Console.WriteLine("All async runed");
            Console.WriteLine("Run sync");
            for (int i = 0; i < 5; i++)
            {
                Request(checker);
                /*if (i == 3)
                {
                    checker.Reset();
                }*/
            }
            Console.WriteLine("All sync runed");

            Thread.Sleep(100);
            checker.Reset();

            Request(checker);

            AsyncRequest(checker);

            checker.Cancel();

            Request(checker);

            AsyncRequest(checker);

            Console.ReadLine();

        }
    }
}
