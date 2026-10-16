//------------------------------------------
// ArrowEnds.cs (c) 2007 by Charles Petzold
// Modified 2026 for Avalonia by Tolly Hill
//------------------------------------------
using System;

namespace Petzold.Media2D
{
    /// <summary>
    ///     Indicates which end of the line has an arrow.
    /// </summary>
    [Flags]
    public enum ArrowEnds
    {
        None = 0,
        Start = 1,
        End = 2,
        Both = 3
    }
}
