//
// <copyright company="Softerra">
//    Copyright (c) Softerra, Ltd. All rights reserved.
// </copyright>
//
// <summary>
//    Check the part of file.
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
        private readonly ReaderWriterLockSlim lockS;

        private readonly String fileName;

        private Int32 inTune;

        private Int32 count;

        private Int32 offset;

        private Int32 innerOffset;

        private Boolean firstCall;

        private readonly WeakReference hashtableWeakReference;

        private String firstSubline;

        private String lastSubline;

        /// <summary>
        /// Create instance of Checker object
        /// </summary>
        /// <param name="fileName">Name of file</param>
        /// <param name="offset">Start position</param>
        /// <param name="count">Bytes to read</param>
        internal Checker(string fileName, Int32 offset, Int32 count)
        {
            this.firstCall = true;
            this.fileName = fileName;
            this.offset = offset;
            this.innerOffset = 0;
            this.count = count;
            this.inTune = 0;
            this.hashtableWeakReference = new WeakReference(null, false);
            lockS = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);
        }

        /// <summary>
        /// Checking part of file and comparing lines with search string
        /// </summary>
        /// <param name="searchLine">Search text</param>
        /// <param name="result">Reference to Result</param>
        /// <param name="prevTuner">Reference to previous tuner</param>
        /// <param name="factory">TaskFactory for creation new Tasks</param>
        /// <returns>Reference to Tuner object, must be send to next Checker</returns>
        internal Tuner Check(Tuner prevTuner,  Result result)
        {

            Tuner thisTuner = inTune == 0 ? new Tuner(this, result) : null;

            Task.Factory.StartNew(() =>
            {
                CancellationToken token;

                if (!result.TryGetToken(out token))
                {
                    // No token - no results
                    return;
                }

                // Get hastable link.
                var hashtable = hashtableWeakReference.Target as Hashtable;
                // Hashtable can not be collect now

                if (hashtable == null)
                {
                    // Hashtable is not alive
                    lockS.EnterWriteLock();

                    if (token.IsCancellationRequested)
                    {
                        return;
                    }
                    
                    // Make one more check
                    hashtable = hashtableWeakReference.Target as Hashtable;
                    if (hashtable != null)
                    {
                        // Hashtable is ready
                        ReadFromHashtable(hashtable, result, ref thisTuner);
                    }
                    else
                    {
                        hashtable = this.Parse(result, thisTuner, prevTuner, token);
                        this.hashtableWeakReference.Target = hashtable;
                    }

                    lockS.ExitWriteLock();
                }
                else
                {
                    // Hashtable is alive, first and last substring is ready

                    lockS.EnterReadLock();

                    ReadFromHashtable(hashtable, result, ref thisTuner);

                    lockS.ExitReadLock();
                }

                if (prevTuner != null)
                {
                    // Send subline to previos tuner
                    prevTuner.SetSecond(this.firstSubline);
                }

                if (thisTuner != null)
                {
                    // Send referanse to hashtable to tuner
                    thisTuner.SetFirst(hashtable);
                }
            });

            return thisTuner;

        }

        /// <summary>
        /// If the object has not been tuned do it 
        /// and update Result of search
        /// </summary>
        /// <param name="nextCheckerFirstSubline">Part of line given by next Checker</param>
        /// <param name="result">Result reference</param>
        /// <param name="thisHashtable">Hastable referance</param>
        internal void Tune(String nextCheckerFirstSubline, 
                 Result result, Hashtable thisHashtable)
        {
            String concantinatedLine = lastSubline + nextCheckerFirstSubline;

            if (Interlocked.CompareExchange(ref this.inTune, 1, 0) == 0)
            {
                // Executed only once, only one tuner can arive this code
                lockS.EnterWriteLock();
                this.count = this.count + nextCheckerFirstSubline.Length;
                DoLineCheck(concantinatedLine, result, thisHashtable);
                lockS.ExitWriteLock();
            }
            else
            {
                // Check but not update hashtable
                DoLineCheck(concantinatedLine, result, null);
            }

        }

        /// <summary>
        /// Parse the part of file and update Result object and hachtable
        /// </summary>
        /// <param name="result">Result reference</param>
        /// <param name="thisTuner">Reference to this Tuner</param>
        /// <param name="prevTuner">Reference to previous Tuner</param>
        /// <param name="token">Cancellation Token</param>
        /// <returns>Referance to hashtable</returns>
        private Hashtable Parse(Result result, Tuner thisTuner,
                                Tuner prevTuner, CancellationToken token)
        {
            // Make one more check
            Hashtable hashtable = new Hashtable();
            using (Stream stream = this.GetStream())
            {
                using (StreamReader streamReader = new StreamReader(stream))
                {
                    // Process first line
                    string line = streamReader.ReadLine();

                    if (this.firstCall && prevTuner != null)
                    {
                        // Take first string while first enter
                                                                                                                                
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
                        if (token.IsCancellationRequested)
                        {
                            // Do not save broken hashtable
                            return null;
                        }

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

                    }

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

            return hashtable;
        }

        
        /// <summary>
        /// Find line in Hashable and udpate Result
        /// </summary>
        /// <param name="hashtable">Referance to collection</param>
        /// <param name="result">Reserence to result</param>
        /// <param name="thisTuner">Tnis object tuner</param>
        private void ReadFromHashtable(Hashtable hashtable,
                                       Result result, ref Tuner thisTuner)
        {
            // If checker become intune 
            if (this.inTune == 1)
            {
                //We do not need tuner more
                thisTuner = null;
            }

            // Get result from hashtable
            if (hashtable.Contains(result.SearchLine))
            {
                result.Increace((int)hashtable[result.SearchLine]);
            }

        }

        /// <summary>
        /// Check one line
        /// </summary>
        /// <param name="line">Line from file</param>
        /// <param name="result">Reserence to Result object</param>
        /// <param name="hashtable">Referance to collection</param>
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
        
        /// <summary>
        /// Releases instance resources
        /// </summary>
        public void Dispose()
        {
            this.lockS.Dispose();
        }
    }
}
