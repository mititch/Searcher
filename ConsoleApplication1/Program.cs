using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.ComponentModel;

namespace ConsoleApplication1
{
    class MyWorker : BackgroundWorker
    {
        
        public MyWorker()
        {
            
        }

        protected override void OnRunWorkerCompleted(RunWorkerCompletedEventArgs e)
        {
            base.OnRunWorkerCompleted(e);
            
        }
    }
    
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

        void Meth(Action<Int32> act) 
        {
            Thread thr = new Thread(x => {});

            Task<Int32> tsk = new Task<Int32>(() => 1);

            var res = act.BeginInvoke(1, null, null);

            CancellationTokenSource cts = new CancellationTokenSource();
            CancellationToken ct = cts.Token;
            
        }

    }
}
