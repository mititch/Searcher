using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LineSearch
{
    public class Element
    {
        private Object next; 

        private Int32 count;

        private Int32 hash;
        
        public Element(object next)
        {
            this.next = next;
        }

        public object GetValue(Int32 hash)
        {
            return null;
        }


    }
}
