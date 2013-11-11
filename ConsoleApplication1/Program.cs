using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace ConsoleApplication1
{
    class Program
    {
        static void Main(string[] args)
        {

            Thread thread = new Thread((o) =>
            {
                throw new Exception();
            });
            //thread.Join(
            thread.IsBackground = true;
            thread.Start();
        }
    }
}
