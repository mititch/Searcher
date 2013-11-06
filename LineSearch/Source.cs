using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LineSearch
{
    using System.Collections;
    using System.IO;
    using System.Security.Cryptography;
using System.Threading;

    internal class Source
    {

        private Int32 u_lock;

        private AutoResetEvent c_lock = new AutoResetEvent(false);

        private IDictionary<string, int> source;

        private readonly String filename;
        
        public Source(String filename)
        {
            this.filename = filename;
        }

     

        private void Fill(String filename)
        {
            var list = new Dictionary<string, int>();
            using (StreamReader reader = new StreamReader(filename))
            {
                while (!reader.EndOfStream)
                {
                    String line = reader.ReadLine();
                    if (list.ContainsKey(line))
                    {
                        list[line] = list[line] + 1;
                    }
                    else
                    {
                        list.Add(line, 1);
                    }
                }
                
            }
            
            this.source = list;

        }

        public Boolean IsReady
        {
            get { return source != null; }
        }

        public int GetValue(String line)
        {
            if (source == null)
            {
                if (Interlocked.Exchange(ref u_lock, 1) == 0)
                {
                    this.Fill(filename);
                    c_lock.Set();
                }
                else
                {
                    c_lock.WaitOne();
                }
            }

            return source.ContainsKey(line) ? this.source[line] : 0;
        }

    }
}
