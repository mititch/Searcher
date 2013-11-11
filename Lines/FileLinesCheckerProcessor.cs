namespace Lines
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using System.IO;
    using System.Collections;

    public class FileLinesCheckerProcessor
    {

        private readonly Object dataLocker = new Object();

        private readonly Object requestQueueLocker = new Object();
        
        private LinesReader reader;

        private Thread readerThread;

        private String fileName;

        private IDictionary data;

        private FileLinesCheckerState state = FileLinesCheckerState.Pending;

        private Queue<CallbackRequest> requestQueue = new Queue<CallbackRequest>();

        public FileLinesCheckerProcessor(String fileName)
        {
            this.fileName = fileName;

            Reset();
        }

       
        public Boolean Contains(String line) 
        {
            if (readerThread != null)
            {
                readerThread.Join();
            }
           
            if (this.state != FileLinesCheckerState.Ready) 
            {
                throw new Exception();
            }
            
            Boolean result;
            lock(dataLocker)  
            {
                result = this.data.Contains(line);
            }

            return result;
            
        }

        public void Contains(String line, Action<Boolean> callback)
        {
            /*lock (this.requestQueueLocker)
            {
                this.requestQueue.Enqueue(new CallbackRequest(line, callback));
            }*/

            //if ready 
            //TODO signal to execution

            Boolean? result = null;
            lock (this.dataLocker)
            {
                if (this.state == FileLinesCheckerState.Ready) 
                {
                    result = this.data.Contains(line);
                }
            }

            ThreadPool.QueueUserWorkItem(ProcessRequest, new CallbackRequest(line, callback));

            if (result == null) 
            {
                readerThread.Join();
            }
            
            
            
            
            if (this.state != FileLinesCheckerState.Ready)
            {
                throw new Exception();
            }

            //Boolean result;
            lock (dataLocker)
            {
                result = this.data.Contains(line);
            }

            //return result;


        }

        public void Reset() 
        {
            ThreadPool.QueueUserWorkItem(LoadData);
        }

        private void LoadData(Object notUsed)
        {
            this.readerThread = Thread.CurrentThread;

            this.state = FileLinesCheckerState.Pending;

            using (Stream stream = new FileStream(this.fileName, FileMode.Open))
            {
                this.reader = new LinesReader();
                
                IDictionary newData = this.reader.Read(stream);
                lock (dataLocker) 
                {
                    this.data = newData;
                }
            }

            this.state = this.data == null ? FileLinesCheckerState.Error : FileLinesCheckerState.Ready;
            
            //todo signal to worker
        }

        private void ProcessRequest(Object @object) 
        {
            CallbackRequest request = @object as CallbackRequest;

            Boolean result = this.Contains(request.Line);
            
            request.Callback(result);
        }
        
        private void ProcessRequests() 
        {
            lock (requestQueueLocker) 
            {

            }
        
        }

        private enum FileLinesCheckerState 
        {
            Pending,
            Ready,
            Error
        }

        private class CallbackRequest 
        {
            private Action<Boolean> callback;

            private String line;

            public CallbackRequest(String line, Action<Boolean> callback)
            {

            }

            public Action<Boolean> Callback 
            {
                get { return this.callback; } 
            }

            public String Line
            {
                get { return this.line; }
            }

        }
    }

}
