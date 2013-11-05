using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Test
{
    class Program
    {
        static void Main(string[] args)
        {
            Int32 e = 0;
            if (Interlocked.CompareExchange(ref e, 1, 0) == 0)
            {
            }
            if (Interlocked.CompareExchange(ref e, 1, 0) == 0)
            {
            }

        }
    }
}
