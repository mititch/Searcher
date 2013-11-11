//
// <copyright company="Softerra">
//    Copyright (c) Softerra, Ltd. All rights reserved.
// </copyright>
//
// <summary>
//    Represents instance state
// </summary>
//
// <author email="mititch@softerra.com">Alex Mitin</author>
//
namespace Strings
{
    public enum LinesCounterState
    {
        // ILinesCounter implementation instance Created
        Created,

        // ILinesCounter implementation is pending for data
        Pending,

        // ILinesCounter implementation is ready for check lines
        Ready,

        // ILinesCounter implementation is brocken and can not be use
        Broken
    }
}
