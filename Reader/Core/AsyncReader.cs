namespace Reader.Core
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Text;
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;


    public class AsyncReader
    {

        private const int BUFFER_SIZE = 200000;

        private readonly string fileName;

        private Hashtable collection;

        private Dictionary<int, string[]> temp;

        private string lastLine;

        public AsyncReader(string fileName)
        {
            this.fileName = fileName;
            this.collection = new Hashtable();
            this.lastLine = String.Empty;
            this.temp = new Dictionary<int, string[]>();
        }

        private void UpdateCollection(string line)
        {
            Int32 hashCode = line.GetHashCode();
            lock (collection.SyncRoot)
            {
                Object prevValue = collection[hashCode];
                collection[hashCode] = prevValue == null ? 1 : (Int32)prevValue + 1;
            }
        }

        private void Parse(byte[] buffer)
        {
            using (Stream stream = new MemoryStream(buffer))
            {
                using (StreamReader streamReader = new StreamReader(stream))
                {
                    String line = streamReader.ReadLine();
                    if (!String.IsNullOrEmpty(lastLine))
                    {
                        line = lastLine + line;
                        lastLine = String.Empty;
                    }

                    do
                    {
                        UpdateCollection(line);

                        line = streamReader.ReadLine();

                    } while (!streamReader.EndOfStream);
                    this.lastLine = line;
                }
            }
        }

        private void ParseAsync(byte[] buffer, int number)
        {
            try
            {
                using (Stream stream = new MemoryStream(buffer))
                {
                    using (StreamReader streamReader = new StreamReader(stream))
                    {
                        String line = streamReader.ReadLine();

                        temp[number - 1][1] = line;

                        int counter = 0;
                        while (!streamReader.EndOfStream)
                        {
                            counter ++;
                            line = streamReader.ReadLine();

                            if (!streamReader.EndOfStream)
                            {
                                UpdateCollection(line);
                            }
                            else
                            {
                                temp[number][0] = line;
                            }
                        }
                        //Console.WriteLine("Task - {0}, processed items- {1}", Task.CurrentId, counter);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        public Hashtable ReadToHashtable()
        {
            using (FileStream fs = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read, BUFFER_SIZE))
            {
                Boolean eof = false;
                Int32 counter = 1;
                IList<Task> taskList = new List<Task>();
                temp[0] = new string[2];
                do
                {
                    Byte[] buffer = new Byte[BUFFER_SIZE];
                    var i = fs.Read(buffer, 0, BUFFER_SIZE);
                    if (i != BUFFER_SIZE)
                    {
                        eof = true;
                    }

                    temp[counter] = new string[2];
                    Int32 number = counter;
                    taskList.Add(Task.Factory.StartNew(() => ParseAsync(buffer, number)));
                    counter++;
                } while (!eof);
                
                Task.WaitAll(taskList.ToArray());

                foreach (var item in temp)
                {
                    UpdateCollection(string.Concat(item.Value));
                }

            }

            return this.collection;
        }

    }
}
