//
// <copyright company="Softerra">
//    Copyright (c) Softerra, Ltd. All rights reserved.
// </copyright>
//
// <summary>
//    Storage for items chains
// </summary>
//
// <author email="mititch@softerra.com">Alex Mitin</author>
//
namespace LineSearchExec
{
    using System;
    using System.Collections.Generic;

    class Storage
    {
        // Function to calculate hash code for table 
        private readonly Func<String, int> tableKeyHashProvider;

        // Function to calculate hash code for items chain
        private readonly Func<String, int> itemKeyHashProvider;

        // Inner collection
        private readonly IDictionary<Int32, Item> data = new Dictionary<int, Item>();

        /// <summary>
        /// Creates an instance of Storage class
        /// </summary>
        /// <param name="tableKeyHashProvider"></param>
        /// <param name="itemKeyHashProvider"></param>
        public Storage(Func<String, int> tableKeyHashProvider, Func<String, int> itemKeyHashProvider)
        {
            this.tableKeyHashProvider = tableKeyHashProvider;

            this.itemKeyHashProvider = itemKeyHashProvider;
        }
        
        /// <summary>
        /// Add or update item to chain in table cell
        /// </summary>
        /// <param name="line">String value for calculating hashes</param>
        public void AddItem(string line)
        {
            Int32 hash1 = tableKeyHashProvider(line);
            Int32 hash2 = itemKeyHashProvider(line);
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

        /// <summary>
        /// Get value of item from storage
        /// </summary>
        /// <param name="line">String value for calculating hashes</param>
        /// <returns>Integer value</returns>
        public Int32 GetValue(string line)
        {
            Int32 hash1 = tableKeyHashProvider(line);

            if (data.ContainsKey(hash1))
            {
                Int32 hash2 = itemKeyHashProvider(line);
                return data[hash1].GetValue(hash2);
            }
            else
            {
                return 0;
            }
        }


    }
}
