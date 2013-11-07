//
// <copyright company="Softerra">
//    Copyright (c) Softerra, Ltd. All rights reserved.
// </copyright>
//
// <summary>
//    Represents an element that can be added to the chain
// </summary>
//
// <author email="mititch@softerra.com">Alex Mitin</author>
//
namespace LineSearchExec
{
    using System;

    internal class Item
    {
        // Element key
        private Int32 hash;
        
        // Next element link
        private Item next;

        // Element value
        private Int32 value = 1;

        /// <summary>
        /// Creates an instance of Element
        /// </summary>
        /// <param name="hash">Item key</param>
        /// <param name="next">Next element reference</param>
        internal Item(Int32 hash, Item next)
        {
            this.hash = hash;
            this.next = next;
        }
        
        // Element value
        internal Int32 Value 
        {
            get { return this.value; }
        }

        /// <summary>
        /// Searches for an item by key and returns its value
        /// </summary>
        /// <param name="hash">Item key</param>
        /// <returns>Element value</returns>
        internal Int32 GetValue(Int32 hash)
        {
            Item item = this.FindItem(hash);

            return item != null ? item.Value : 0;
        }

        /// <summary>
        /// Increases the value of the item if it is present in a chain or add a new
        /// </summary>
        /// <param name="hash">Item key</param>
        internal void AddValue(Int32 hash)
        {
            Item item = this.FindItem(hash);

            if (item == null)
            {
                item = new Item(hash, this);
            }
            else
            {
                item.IncrItemValue();
            }
        }

        /// <summary>
        /// Increases the value of this item
        /// </summary>
        internal void IncrItemValue()
        {
            this.value++;
        }

        /// <summary>
        /// Searches for an item in chain
        /// </summary>
        /// <param name="hash">Item key</param>
        /// <returns>Value</returns>
        internal Item FindItem(Int32 hash)
        {
            if (this.hash == hash)
            {
                return this;
            }
            else
            {
                return this.next != null ? next.FindItem(hash) : null;
            }
        }

    }
}
