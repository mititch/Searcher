//
// <copyright company="Softerra">
//    Copyright (c) Softerra, Ltd. All rights reserved.
// </copyright>
//
// <summary>
//    Reads lines from steam and generate dictionary.
//    Dictionary keys - line hash code
//    Dictionary values - number of identical lines in file 
// </summary>
//
// <author email="mititch@softerra.com">Alex Mitin</author>
//
namespace Strings
{
    using System.IO;
    using System;
    using System.Collections.Generic;
    using System.Threading;

    class LinesReader
    {

#region fields
        
        // Is instance work canceled
        private Boolean canceled;

        // Generates a hash code for dictionary items
        private readonly Func<String, Int32> hashCodeProvider;

#endregion

#region constructors
        //
        // constructors
        //


        /// <summary>
        /// Creates an instance of LinesReader class
        /// </summary>
        /// <param name="hashCodeProvider">Hash code generator</param>
        public LinesReader(Func<String, Int32> hashCodeProvider)
        {
            this.hashCodeProvider = hashCodeProvider;
        }

#endregion

#region methods
        //
        // methods
        //

        /// <summary>
        /// Cancel execution of the read process
        /// </summary>
        public void Cancel()
        {
            this.canceled = true;
        }

        
        /// <summary>
        /// Reads lines from string and prepares the dictionary
        /// </summary>
        /// <param name="stream">Stream of data</param>
        /// <returns>Dictionary with hashes as keys and lines counts as values</returns>
        public IDictionary<Int32, Int32> Read(Stream stream)
        {
            IDictionary<Int32, Int32> result = new Dictionary<Int32, Int32>();
            try
            {
                using (StreamReader reader = new StreamReader(stream))
                {
                    while (!this.canceled && !reader.EndOfStream)
                    {
                        this.ProcessLine(reader.ReadLine(), result);
                    }
                }
            }
            catch (Exception exception)
            {
                String message = String.Format(
                    "Instance of LinesReader can not process the stream. ManagedThreadId={0}",
                    Thread.CurrentThread.ManagedThreadId);

                throw new InvalidOperationException(message, exception);
            }
            finally
            {
                stream.Dispose();
            }
            
            return result;
        }

        /// <summary>
        /// Checks a line and updates the dictionary
        /// </summary>
        /// <param name="line">Line for search</param>
        /// <param name="dictionary">Data storage</param>
        private void ProcessLine(String line, IDictionary<Int32, Int32> dictionary)
        {
            Int32 hashcode = this.hashCodeProvider(line);
            
            if (dictionary.ContainsKey(hashcode))
            {
                dictionary[hashcode] = dictionary[hashcode] + 1;
            }
            else
            {
                dictionary.Add(hashcode, 1);
            }
        }

#endregion

    }
}
