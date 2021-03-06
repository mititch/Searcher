﻿//
// <copyright company="Softerra">
//    Copyright (c) Softerra, Ltd. All rights reserved.
// </copyright>
//
// <summary>
//    Search for random lines count in file
// </summary>
//
// <author email="mititch@softerra.com">Alex Mitin</author>
//
namespace LineSearchExec
{
    using System;
    using System.Diagnostics;
    using System.Threading.Tasks;

    class Program
    {
        /// <summary>
        /// Name of file
        /// </summary>
        private const string FILE_NAME = "E:/bf/f2.txt";//C:/Users/mititch/Downloads/bf/f3.txt";

        /// <summary>
        /// Common part of any line
        /// </summary>
        private const string SOME_STRING =
            "have very many outstanding loans but I do need to consolidate and move ";
        
        /// <summary>
        /// Starts execution
        /// </summary>
        /// <param name="args"></param>
        static void Main(String[] args)
        {
            Random random = new Random();

            Source source = new Source(FILE_NAME);

            source.PrepareSource();

            Searcher searcher = new Searcher(source);

            searcher.GetLinesCountAsync(String.Format("{0} {1}", SOME_STRING, random.Next(100))).ContinueWith(task =>
            {
                Console.WriteLine("Process..");
                Stopwatch sw = new Stopwatch();
                sw.Start();

                Task<Int32>[] tasks = new Task<Int32>[1000];

                for (Int32 j = 0; j < 10; j++)
                {
                    for (Int32 i = 0; i < 100; i++)
                    {
                        tasks[j * 100 + i] = searcher.GetLinesCountAsync(String.Format("{0} {1}", SOME_STRING, i));
                    }
                }
                Task.WaitAll(tasks);
                sw.Stop();
                foreach (Task<Int32> task1 in tasks)
                {
                    Console.WriteLine(task1.Result);
                }
                Console.WriteLine("Elapsed time - {0}", sw.ElapsedMilliseconds);
            });

            Console.ReadLine();
        }

    }
}
