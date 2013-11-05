namespace Collections.Core
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Diagnostics;

    public static class TestHelpers
    {
        public static T Fill<T>(this T collection, Action<T> fillAction) 
        {
            Console.WriteLine("{0} test :", collection.GetType());

            Stopwatch watch = new Stopwatch();

            Int64 beforeMemoryUsage = GC.GetTotalMemory(true);

            watch.Start();

            fillAction(collection);

            watch.Stop();

            Console.WriteLine("Memory usage - {0}", GC.GetTotalMemory(true) - beforeMemoryUsage);

            Console.WriteLine("Parce time - {0}", watch.ElapsedMilliseconds);

            return collection;

        }

        public static void Search<T>(this T collection, Func<T, bool> searchFunction)
        {
            Stopwatch watch = new Stopwatch();
            
            watch.Start();

            Boolean success = searchFunction(collection);

            watch.Stop();

            if (success)
            {
                Console.WriteLine("Search time - {0}", watch.ElapsedMilliseconds);
            }
            else
            {
                Console.WriteLine("Search failed");
            }
            Console.ReadLine();
        }
    }
}
