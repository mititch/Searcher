//
// <copyright company="Softerra">
//    Copyright (c) Softerra, Ltd. All rights reserved.
// </copyright>
//
// <summary>
//    Checks the part of file.
// </summary>
//
// <author email="mititch@softerra.com">Alex Mitin</author>
//
namespace Lib
{
    using System;
    using System.Collections;
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;

    internal class Checker : IDisposable
    {
        private Int32 count;

        private readonly String fileName;

        private Boolean firstCall = true;

        private String firstSubline;

        private readonly WeakReference hashtableWeakReference = 
            new WeakReference(null, false);

        private Int32 innerOffset = 0;

        private Int32 inTune = 0;

        private String lastSubline;

        private readonly ReaderWriterLockSlim lockSlim = 
            new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);

        private readonly Int32 offset;

        /// <summary>
        /// Creates an instance of Сhecker class
        /// </summary>
        /// <param name="fileName">Name of file</param>
        /// <param name="offset">Start position</param>
        /// <param name="count">Bytes to read</param>
        internal Checker(string fileName, Int32 offset, Int32 count)
        {
            this.fileName = fileName;
            this.offset = offset;
            this.count = count;
        }

        /// <summary>
        /// Releases instance resources
        /// </summary>
        public void Dispose()
        {
            this.lockSlim.Dispose();
        }

        /// <summary>
        /// Checks the part of file and compare lines with search string
        /// </summary>
        /// <param name="prevTuner">Reference to previous Tuner</param>
        /// <param name="result">Reference to Result</param>
        /// <param name="token">Cancellation token</param>
        /// <returns>Reference to new Tuner object, must be send to the next Checker</returns>
        internal Tuner Check(Tuner prevTuner,  Result result, CancellationToken token)
        {

            Tuner thisTuner = inTune == 0 ? new Tuner(this, result) : null;

            Task.Factory.StartNew(() =>
            {

                var hashtable = hashtableWeakReference.Target as Hashtable;
                // Hashtable can not be collect now

                if (hashtable == null)
                {
                    // Hashtable is not alive
                    lockSlim.EnterWriteLock();

                    if (token.IsCancellationRequested)
                    {
                        lockSlim.ExitWriteLock();
                        return;
                    }

                    // Make one more check
                    if (!TryReadFromHashtable(result, ref thisTuner, out hashtable))
                    {
                        hashtable = this.Parse(result, thisTuner, prevTuner, token);
                        this.hashtableWeakReference.Target = hashtable;
                    }

                    lockSlim.ExitWriteLock();
                }
                else
                {
                    // Hashtable is alive, first and last substrings is set

                    lockSlim.EnterReadLock();

                    ReadFromHashtable(hashtable, result, ref thisTuner);

                    lockSlim.ExitReadLock();
                }
                
                if (prevTuner != null)
                {
                    // Send subline to previous Tuner
                    prevTuner.SetTuner(this.firstSubline);
                }

                if (thisTuner != null && hashtable != null)
                {
                    // Send reference to hashtable to the Tuner
                    thisTuner.RequestTune(hashtable);
                }

            }, token);

            return thisTuner;

        }

        /// <summary>
        /// If the object has not been tuned does it 
        /// and makes updating of the search Result
        /// </summary>
        /// <param name="nextCheckerFirstSubline">Subline from the previous Checker</param>
        /// <param name="result">Reference to Result</param>
        /// <param name="thisHashtable">Reference to Hashtable </param>
        internal void Tune(String nextCheckerFirstSubline, 
                 Result result, Hashtable thisHashtable)
        {
            String concantinatedLine = lastSubline + nextCheckerFirstSubline;

            if (Interlocked.CompareExchange(ref this.inTune, 1, 0) == 0)
            {
                // Executed only once, This code can be achieved by only one Tuner
                lockSlim.EnterWriteLock();

                this.count = this.count + nextCheckerFirstSubline.Length;
                DoLineCheck(concantinatedLine, result, thisHashtable);

                lockSlim.ExitWriteLock();
            }
            else
            {
                // Check but not update hashtable
                DoLineCheck(concantinatedLine, result, null);
            }

        }

        /// <summary>
        /// Parse the part of file and update Result object and hashtable
        /// </summary>
        /// <param name="result">Result reference</param>
        /// <param name="thisTuner">Reference to this Tuner</param>
        /// <param name="prevTuner">Reference to previous Tuner</param>
        /// <param name="token">Cancellation Token</param>
        /// <returns>Referеnce to hashtable</returns>
        private Hashtable Parse(Result result, Tuner thisTuner,
                                Tuner prevTuner, CancellationToken token)
        {
            Hashtable hashtable = new Hashtable();
            using (Stream stream = this.GetStream())
            {
                using (StreamReader streamReader = new StreamReader(stream))
                {
                    // Process first line
                    string line = streamReader.ReadLine();

                    if (this.firstCall && prevTuner != null)
                    {
                        // Take the first line during the first pass
                                                                                                                                
                        this.firstSubline = line;
                        this.innerOffset = String.IsNullOrEmpty(line) ? 0 : line.Length;
                    }
                    else
                    {
                        DoLineCheck(line, result, hashtable);
                    }
                    this.firstCall = false;
                    // Now execution can be canceled

                    // Process body
                    Boolean done = streamReader.EndOfStream;
                    while (!done)
                    {
                        
                        line = streamReader.ReadLine();

                        if (!streamReader.EndOfStream)
                        {
                            // Process all string but not last one
                            DoLineCheck(line, result, hashtable);
                        }
                        else
                        {
                            done = true;
                        }

                        if (token.IsCancellationRequested)
                        {
                            // Do not save broken hashtable
                            hashtable = null;
                            done = true;
                        }

                    }

                    if (!token.IsCancellationRequested)
                    {
                        // Process last line
                        if (this.inTune == 0)
                        {
                            //Save last string
                            this.lastSubline = line;
                        }

                        // If tuner stil alive he should process this line,
                        // even if checker already in tune
                        if (thisTuner == null)
                        {
                            // Tune is done - process last line
                            // Hastable was collect, but offset and byte count already changed
                            DoLineCheck(line, result, hashtable);
                        }
                    }
                }

            }

            return hashtable;
        }

        
        /// <summary>
        /// Find line in Hashable and udpate Result
        /// </summary>
        /// <param name="hashtable">Reference to collection</param>
        /// <param name="result">Reserence to Result</param>
        /// <param name="thisTuner">Tnis object Tuner</param>
        private void ReadFromHashtable(Hashtable hashtable,
                                       Result result, ref Tuner thisTuner)
        {
           
            // If checker become intune 
            if (this.inTune == 1)
            {
                // We do not need of tuner more
                thisTuner = null;
            }

            // Get result from hashtable
            if (hashtable.Contains(result.SearchLine))
            {
                result.Increace((int)hashtable[result.SearchLine]);
            }

        }

        /// <summary>
        /// Trying find line in Hashable and udpate Result
        /// </summary>
        /// <param name="result">Reserence to Result</param>
        /// <param name="thisTuner">Tnis object Tuner</param>
        /// <param name="hashtable">Reference to collection</param>
        private Boolean TryReadFromHashtable(Result result, ref Tuner thisTuner, out Hashtable hashtable)
        {
            Boolean success = false;
            
            hashtable = hashtableWeakReference.Target as Hashtable;
            
            if (hashtable != null)
            {
                // Hashtable is exist 
                ReadFromHashtable(hashtable, result, ref thisTuner);
                success = true;
            }

            return success;

        }

        /// <summary>
        /// Check one line
        /// </summary>
        /// <param name="line">Line from file</param>
        /// <param name="result">Reference to Result object</param>
        /// <param name="hashtable">Reference to collection</param>
        private void DoLineCheck(String line, 
                                 Result result, Hashtable hashtable)
        {

            // Update result manual
            if (result != null && String.Equals(line, result.SearchLine))
            {
                result.Increace();
            }

            // Try update hashtable 
            if (hashtable != null)
            {
                Object prevValue = hashtable[line];
                hashtable[line] = prevValue == null ? 1 : (Int32)prevValue + 1;
            }
        }


        /// <summary>
        /// Reading block of bytes from file and converting it in Stream
        /// </summary>
        /// <returns>Stream of bytes</returns>
        private Stream GetStream()
        {
            Byte[] buffer = new Byte[count];

            using (FileStream fileStream = 
                new FileStream(this.fileName, FileMode.Open, FileAccess.Read))
            {
                
                fileStream.Seek(this.offset + this.innerOffset, SeekOrigin.Begin);
                
                Int32 readed = fileStream.Read(buffer, 0, this.count - innerOffset);

                // Remove empty bytes
                this.count = readed;
            }

            // Convert bytes to memory stream
            return new MemoryStream(buffer, 0, this.count);

        }
        
    }
}
