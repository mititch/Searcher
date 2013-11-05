using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lib;

namespace LibTest
{
    using System.Threading;

    class Program
    {
        private const string FILE_NAME = "C:/Users/mititch/Downloads/bf/f2.txt";

        private const string SOME_STRING = "have very many outstanding loans but I do need to consolidate and move ";

        static void Main(string[] args)
        {
            Searcher searcher = new Searcher(FILE_NAME, 6);
            var res1 = searcher.Search(String.Format("{0} {1}", SOME_STRING, 1));
            var res2 = searcher.Search(String.Format("{0} {1}", SOME_STRING, 2));
            var res3 = searcher.Search(String.Format("{0} {1}", SOME_STRING, 3));
            var res4 = searcher.Search(String.Format("{0} {1}", SOME_STRING, 4));
            Console.ReadLine();
            Console.WriteLine(res1.Value);
            Console.WriteLine(res2.Value);
            Console.WriteLine(res3.Value);
            Console.WriteLine(res4.Value);
            Console.WriteLine("Collect?");
            Console.ReadLine();
            GC.Collect();
            Thread.Sleep(1000);
            Console.WriteLine("New Search");
            var res5 = searcher.Search(String.Format("{0} {1}", SOME_STRING, 5));
            Console.ReadLine();
            
            Console.WriteLine(res5.Value);
            Console.ReadLine();

        }
    }
}
