using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace LineSearchExec
{
    class Storage
    {
        private Func<String, int> firstHashProvider;

        private Func<String, int> secondHashProvider;

        private IDictionary<Int32, Item> data = new Dictionary<int, Item>();

        public Storage(Func<String, int> firstHashProvider, Func<String, int> secondHashProvider)
        {
            this.firstHashProvider = firstHashProvider;
            
            this.secondHashProvider = secondHashProvider;
        }
        
        public void AddItem(string line)
        {
            Int32 hash1 = firstHashProvider(line);
            Int32 hash2 = secondHashProvider(line);
            if (data.ContainsKey(hash1))
            {
                Item item = data[hash1];

                item.AddValue(hash2);
            }
            else
            {
                data.Add(hash1, new Item(hash2, null));
            }
        }

        public Int32 GetValue(string line)
        {
            Int32 hash1 = firstHashProvider(line);

            if (data.ContainsKey(hash1))
            {
                Int32 hash2 = secondHashProvider(line);
                return data[hash1].GetValue(hash2);
            }
            else
            {
                return 0;
            }
        }


    }
}
