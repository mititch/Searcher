using System;
using System.Collections;
using System.Diagnostics;
using System.IO;
using Collections.Core;
using Reader.Core;

namespace Reader
{
    class Program
    {
        private const string FILE_NAME = "E:/bf/f1.txt";

        static void Main(string[] args)
        {
            SyncAsync();
        }

        private static void SyncAsync()
        {
            Console.WriteLine("Sync");

            var frs = new Hashtable().Fill(collection =>
            {
                using (StreamReader streamReader = new StreamReader(FILE_NAME))
                {
                    while (!streamReader.EndOfStream)
                    {
                        string line = streamReader.ReadLine();
                        Int32 code = line.GetHashCode();
                        Object prevValue = collection[code];
                        collection[code] = prevValue == null ? 1 : (Int32)prevValue + 1; // unbox
                    }
                }
            });

            Console.WriteLine("Async");

            GC.Collect();

            var beforeMemoryUsage = GC.GetTotalMemory(true);
            var watch = new Stopwatch();

            watch.Start();
            var reader = new AsyncReader(FILE_NAME);

            var ht = reader.ReadToHashtable();
            watch.Stop();

            Console.WriteLine("Memory usage - {0}", GC.GetTotalMemory(true) - beforeMemoryUsage);

            Console.WriteLine("Parce time - {0}", watch.ElapsedMilliseconds);

            Console.ReadLine();
        }
    }
}
