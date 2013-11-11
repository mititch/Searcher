namespace Strings
{
    using System;
    
    class Program
    {
        /// <summary>
        /// File name
        /// </summary>
        private const String FILE_NAME = "C:/Users/mititch/Downloads/bf/f2.txt";

        /// <summary>
        /// Common part of any line
        /// </summary>
        private const String SOME_STRING =
            "have very many outstanding loans but I do need to consolidate and move ";

        static void Main(String[] args)
        {
            
            ILinesCounter counter = new FileLinesCounter(FILE_NAME, HashCodeProvider);
            
            Int32 result;
            
            Console.WriteLine(counter.TryGetLinesCount(SOME_STRING + " 1", out result));
            
            counter.GetLinesCountAsync(SOME_STRING + " 1", Console.WriteLine);
            
            counter.GetLinesCountAsync(SOME_STRING + " 2", Console.WriteLine);
            
            counter.GetLinesCountAsync(SOME_STRING + " 3", Console.WriteLine);
            
            Console.WriteLine("Sync - {0}", counter.GetLinesCount(SOME_STRING + " 3"));
            
            Console.WriteLine(counter.TryGetLinesCount(SOME_STRING + " 1", out result));
            
            Console.ReadLine();
            
            Console.WriteLine("Sync - {0}", counter.GetLinesCount(SOME_STRING + " 3"));
            
            Console.ReadLine();
        }

        /// <summary>
        /// Simple hash code provider
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        private static Int32 HashCodeProvider(String s)
        {
            return s.GetHashCode();
        }
    }
}
