using System.Threading;

namespace BigFile
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Text;
    using System.Xml.Serialization;
    using Core;

    internal class Program
    {
        private const string FILE_NAME = "C:/Users/mititch/Downloads/bf/f2.txt";

        private const string SOME_STRING = "have very many outstanding loans but I do need to consolidate and move ";

        private static Stopwatch sw = new Stopwatch();

        private static void Main(string[] args)
        {

            //TST();
            Search();

            //Generate();
        }

        private static void TST()
        {
            Searcher searcher = new Searcher("E:/bf/tst.txt", 2);
            var res1 = searcher.Search("str1");
            var res2 = searcher.Search("str2");
            var res3 = searcher.Search("str3");
            Thread.Sleep(1000);
            GC.Collect();
            var res4 = searcher.Search("str4");
            Thread.Sleep(1000);
            Console.ReadLine();
            Console.WriteLine(res1.Value);
            Console.WriteLine(res2.Value);
            Console.WriteLine(res3.Value);
            Console.WriteLine(res4.Value);
            Console.ReadLine();

        }



        private static void Search()
        {
            sw.Start();
            Searcher searcher = new Searcher(FILE_NAME, 5);

            var results = new Dictionary<string, Result>();
            Random r = new Random();
            for (int i = 0; i < 10; i++)
            {
                String searchString = String.Format("{0} {1}", SOME_STRING, i);
                results[searchString] = searcher.Search(searchString);
                Thread.Sleep(r.Next(100));
            }

            Boolean done = false;
            Boolean cancelFirst = true;
            Int32 counter = 0;
            do
            {
                Console.Write("Enter to show results (Q-Exit):");

                done = Console.ReadLine().Equals("Q");
                //sw.Stop();
                Console.WriteLine("time - {0}", sw.ElapsedMilliseconds);
                Console.WriteLine("---------");

                foreach (var result in results)
                {
                    Console.WriteLine("{0} - {1}", result.Key.Substring(71), result.Value.Value);
                }
                if (cancelFirst)
                {
   
                    results.First().Value.Cancel();
                    cancelFirst = true;
                    cancelFirst = false;
                }
                
            } while (!done);

        }

        private static void Generate()
        {

            using (Stream stream = new FileStream(FILE_NAME, FileMode.CreateNew))
            {
                using (StreamWriter sw = new StreamWriter(stream))
                {
                    Random random = new Random();
                    for (Int32 j = 0; j < 100; j++)
                    {
                        for (Int32 i = 0; i < 100000; i++)
                        {
                            sw.WriteLine(String.Format("{0} {1}", SOME_STRING, random.Next(100)));
                        }
                        Console.WriteLine("{0} percent done.", j);
                    }
                }
            }
        }
    }

}


