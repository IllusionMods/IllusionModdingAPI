using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace KKAPI.Utilities
{
    /// <summary>
    /// String comparer that is equivalent to the one used by Windows Explorer to sort files (e.g. 2 will go before 10, unlike normal compare).
    /// </summary>
    /// <inheritdoc />
    public class WindowsStringComparer : IComparer<string>
    {
        [DllImport("shlwapi.dll", CharSet = CharSet.Unicode, ExactSpelling = true)]
        private static extern int StrCmpLogicalW(String x, String y);

        /// <summary>
        /// Compare two strings with rules used by Windows Explorer to logically sort files.
        /// </summary>
        public int Compare(string x, string y)
        {
            return StrCmpLogicalW(x, y);
        }

        /// <summary>
        /// Compare two strings with rules used by Windows Explorer to logically sort files.
        /// </summary>
        public static int LogicalCompare(string x, string y)
        {
            return StrCmpLogicalW(x, y);
        }
    }
}