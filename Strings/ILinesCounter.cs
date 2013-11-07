//
// <copyright company="Softerra">
//    Copyright (c) Softerra, Ltd. All rights reserved.
// </copyright>
//
// <summary>
//    ILinesCounter interface
//    Defines methods which return number of identical lines in some source
// </summary>
//
// <author email="mititch@softerra.com">Alex Mitin</author>
//
namespace Strings
{
    using System;

    public interface ILinesCounter
    {
        /// <summary>
        /// Represents instance state
        /// </summary>
        LinesCounterState State { get; }

        /// <summary>
        /// Cancel execution
        /// </summary>
        void Cancel();

        /// <summary>
        /// Return count of lines in the source
        /// </summary>
        /// <param name="line">Line for search</param>
        /// <returns>Count of lines</returns>
        Int32 GetLinesCount(String line);

        /// <summary>
        /// Return count of lines in the source asynchronously
        /// </summary>
        /// <param name="line">Line for search</param>
        /// <param name="callback">Callback delegate</param>
        /// <exception cref="FieldAccessException">Thrown if istance not ready</exception>
        void GetLinesCountAsync(String line, Action<Int32> callback);

        /// <summary>
        /// Return count of lines in the source as out parameter
        /// </summary>
        /// <param name="line">Line for search</param>
        /// <param name="result">Returns lines count</param>
        /// <returns>Instance state</returns>
        LinesCounterState TryGetLinesCount(String line, out Int32 result);
    }
}
