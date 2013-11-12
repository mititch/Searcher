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
namespace Lines
{
    using System.IO;
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Collections;

    class LinesReader
    {

        #region fields
        //
        // fields
        //

        // Is instance work canceled
        private Boolean canceled;

        #endregion

        #region Properties
        //
        // Properties
        //

        public Boolean IsCanceled { get { return this.canceled; } }

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
        /// Reads lines from stream and prepares the dictionary which contains existing linnes
        /// </summary>
        /// <param name="stream">Stream of data</param>
        /// <returns>Dictionary with hashes as keys and lines counts as values</returns>
        public IDictionary Read(Stream stream)
        {
            IDictionary result = new Hashtable();

            using (StreamReader reader = new StreamReader(stream))
            {
                while (!this.canceled && !reader.EndOfStream)
                {
                    this.ProcessLine(reader.ReadLine(), result);
                }
            }

            return !this.canceled ? result : null; 
        }

        /// <summary>
        /// Checks a line and updates the dictionary
        /// </summary>
        /// <param name="line">Line for search</param>
        /// <param name="dictionary">Data storage</param>
        private void ProcessLine(String line, IDictionary dictionary)
        {
           
            if (!dictionary.Contains(line))
            {
                dictionary.Add(line, null);
            }
            
        }

        #endregion

    }
}
