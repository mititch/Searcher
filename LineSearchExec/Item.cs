using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LineSearchExec
{

    internal class Item
    {
        
        private Int32 hash;
        
        private Item next;

        private Int32 value = 1;

        internal Item(Int32 hash, Item next)
        {
            this.hash = hash;
            this.next = next;
        }
        
        internal Int32 Value 
        {
            get { return this.value; }
        }

        internal Int32 GetValue(Int32 hash)
        {
            Item item = this.FindItem(hash);

            return item != null ? item.Value : 0;
        }

        internal void AddValue(Int32 hash)
        {
            Item item = this.FindItem(hash);

            if (item == null)
            {
                item = new Item(hash, this);
            }
            else
            {
                item.IncrValue();
            }
        }

        internal void IncrValue()
        {
            this.value++;
        }

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
