namespace Lines
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using System.IO;
    using System.Collections;

    public class FileLinesChecker
    {

        private Int32 busy;

        private IDictionary data;

        private readonly Object dataLocker = new Object();

        private readonly Object resetLocker = new Object();

        private String fileName;

        private Thread readerThread;

        private FileLinesCheckerState state = FileLinesCheckerState.Pending;

        public FileLinesChecker(String fileName)
        {
            this.fileName = fileName;
            Reset();
        }

        public Boolean Contains(String line)
        {
            Boolean result;

            lock (dataLocker)
            {

                if (this.state == FileLinesCheckerState.Pending)
                {
                    Monitor.Wait(dataLocker);
                }

                if (this.state != FileLinesCheckerState.Ready)
                {
                    throw new Exception(String.Format("Except - {0}", this.state));
                }

                result = this.data.Contains(line);
            }

            return result;

        }

        public void ContainsAsync(String line, Action<Boolean> onSuccess, Action<FileLinesCheckerState> onFailure)
        {
            ThreadPool.QueueUserWorkItem(ProcessRequest, new CallbackRequest(line, onSuccess, onFailure));
        }

        public void Cancel()
        {
            lock (dataLocker)
            {
                this.state = FileLinesCheckerState.Canceled;


                if (readerThread != null && readerThread.IsAlive)
                {
                    this.readerThread.Abort();
                    this.readerThread.Join();
                }
            }

        }

        public void Reset()
        {
            Interlocked.Increment(ref busy);
            
            lock (dataLocker)
            {
                this.state = FileLinesCheckerState.Pending;
            }

            ThreadPool.QueueUserWorkItem(LoadData);


            /*if (readerThread != null && readerThread.IsAlive)
            {
                this.readerThread.Abort();
                this.readerThread.Join();
            }

            this.readerThread = new Thread(LoadData);

            this.readerThread.IsBackground = true;

            this.readerThread.Start();*/



        }

        private void LoadData(Object notUsed)
        {

            lock (dataLocker)
            {
                if (readerThread != null && readerThread.IsAlive)
                {
                    this.readerThread.Abort();
                    this.readerThread.Join();
                }

                this.readerThread = Thread.CurrentThread;
            }

            IDictionary newData = null;
            try
            {
                using (Stream stream = new FileStream(this.fileName, FileMode.Open))
                {
                    LinesReader reader = new LinesReader();

                    newData = reader.Read(stream);
                }
            }
            catch (Exception e)
            {
            }
            finally
            {
                if (Interlocked.Decrement(ref busy) == 0)
                {
                    lock (dataLocker)
                    {
                        if (this.state != FileLinesCheckerState.Canceled)
                        {
                            this.data = newData;
                            this.state = this.data == null ? FileLinesCheckerState.Error : FileLinesCheckerState.Ready;
                        }
                        Monitor.PulseAll(dataLocker);
                    }
                }
            }

            //this.state = this.data == null ? FileLinesCheckerState.Error : FileLinesCheckerState.Ready;
        }

        private void ProcessRequest(Object @object)
        {
            CallbackRequest request = @object as CallbackRequest;

            lock (dataLocker)
            {
                if (this.state == FileLinesCheckerState.Pending)
                {
                    Monitor.Wait(dataLocker);
                }

                if (this.state != FileLinesCheckerState.Ready)
                {
                    request.FailureCallback(this.state);
                }
                else
                {
                    request.SuccessCallback(this.data.Contains(request.Line));
                }


            }

        }

        public enum FileLinesCheckerState
        {
            Pending,
            Ready,
            Error,
            Canceled
        }

        private class CallbackRequest
        {
            private Action<Boolean> sucessCallback;

            private Action<FileLinesCheckerState> failureCallback;

            private String line;

            public CallbackRequest(String line, Action<Boolean> sucessCallback, Action<FileLinesCheckerState> failureCallback)
            {
                this.line = line;
                this.sucessCallback = sucessCallback;
                this.failureCallback = failureCallback;
            }

            public Action<Boolean> SuccessCallback
            {
                get { return this.sucessCallback; }
            }

            public Action<FileLinesCheckerState> FailureCallback
            {
                get { return this.failureCallback; }
            }


            public String Line
            {
                get { return this.line; }
            }

        }
    }

}
