namespace Collections
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

    class Program
    {
        private const string FILE_NAME = "E:/bf/f1.txt";
        private const string SOME_STRING = "have very many outstanding loans but I do need to consolidate and move ";
        private const string SEARCH_FIRST_LINE = SOME_STRING + "1";
        private const string SEARCH_MIDDLE_LINE = SOME_STRING + "2";
        private const string SEARCH_LAST_LINE = SOME_STRING + "3";
        

        private static Stopwatch sw = new Stopwatch();

        static void Main(string[] args)
        {
            CollectionsTest();
        }

        private static void CollectionsTest()
        {
            Bad1();
            GC.Collect();
            Bad2();
            GC.Collect();
            Bad3();
            GC.Collect();
            Bad4();
            GC.Collect();
            Bad5();
        }

        private static void Bad1()
        {
            new Hashtable().Fill(collection =>
            {
                using (StreamReader streamReader = new StreamReader(FILE_NAME))
                {
                    while (!streamReader.EndOfStream)
                    {
                        String line = streamReader.ReadLine();
                        Object prevValue = collection[line];
                        collection[line] = prevValue == null ? 1 : (Int32)prevValue + 1;
                    }
                }
            })
                .Search(collection =>
                {
                    return (Int32)collection[SEARCH_FIRST_LINE] == 1
                        && (Int32)collection[SEARCH_MIDDLE_LINE] == 1
                        && (Int32)collection[SEARCH_LAST_LINE] == 1;
                });
        }

        private static void Bad2()
        {

            new ArrayList().Fill(collection =>
            {
                using (StreamReader streamReader = new StreamReader(FILE_NAME))
                {
                    while (!streamReader.EndOfStream)
                    {
                        String line = streamReader.ReadLine();
                        collection.Add(line);
                    }
                }
            })
                .Search(collection =>
                {
                    return collection.IndexOf(SEARCH_FIRST_LINE) != -1
                        && collection.IndexOf(SEARCH_MIDDLE_LINE) != -1
                        && collection.IndexOf(SEARCH_LAST_LINE) != -1;
                });

        }

        private static void Bad3()
        {
            new List<String>().Fill(collection =>
            {
                using (StreamReader streamReader = new StreamReader(FILE_NAME))
                {
                    while (!streamReader.EndOfStream)
                    {
                        String line = streamReader.ReadLine();
                        collection.Add(line);
                    }
                }
            })
                .Search(collection =>
                {
                    return collection.Any(x => x == SEARCH_FIRST_LINE)
                           && collection.Any(x => x == SEARCH_MIDDLE_LINE)
                           && collection.Any(x => x == SEARCH_LAST_LINE);
                });
        }

        private static void Bad4()
        {
            new SortedList().Fill(collection =>
            {
                using (StreamReader streamReader = new StreamReader(FILE_NAME))
                {
                    while (!streamReader.EndOfStream)
                    {
                        String line = streamReader.ReadLine();
                        Object prevValue = collection[line];
                        collection[line] = prevValue == null ? 1 : (Int32)prevValue + 1;
                    }
                }
            })
                .Search(collection =>
                {
                    return collection.ContainsKey(SEARCH_FIRST_LINE)
                           && collection.ContainsKey(SEARCH_MIDDLE_LINE)
                           && collection.ContainsKey(SEARCH_LAST_LINE);
                });
        }

        private static void Bad5()
        {
            new Hashtable().Fill(collection =>
            {
                using (StreamReader streamReader = new StreamReader(FILE_NAME))
                {
                    while (!streamReader.EndOfStream)
                    {
                        Int32 code = streamReader.ReadLine().GetHashCode();
                        Object prevValue = collection[code];
                        collection[code] = prevValue == null ? 1 : (Int32)prevValue + 1; // unbox
                    }
                }
            })
                .Search(collection =>
                {
                    return (Int32)collection[SEARCH_FIRST_LINE.GetHashCode()] == 1
                        && (Int32)collection[SEARCH_MIDDLE_LINE.GetHashCode()] == 1
                        && (Int32)collection[SEARCH_LAST_LINE.GetHashCode()] == 1;
                });
        }


    }
}


