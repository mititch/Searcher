//
// <copyright company="Softerra">
//    Copyright (c) Softerra, Ltd. All rights reserved.
// </copyright>
//
// <summary>
//    Instance can read lines from stream
//    and generate dictionary which contains existing lines
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

    public class LinesReader
    {

        #region fields
        //
        // fields
        //

        // Contains true if execution was canceled
        private Boolean canceled;

        #endregion

        #region Properties
        //
        // Properties
        //

        /// <summary>
        /// Return true if execution was canceled
        /// </summary>
        public Boolean IsCanceled 
        { 
            get { return this.canceled; } 
        }

        #endregion

        #region methods
        //
        // methods
        //

        /// <summary>
        /// Cancel execution of the read
        /// </summary>
        public void Cancel()
        {
            this.canceled = true;
        }

        /// <summary>
        /// Reads lines from stream and returns the dictionary which contains existing lines
        /// </summary>
        /// <param name="stream">Stream of data</param>
        /// <returns>Dictionary containing the existing lines</returns>
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
