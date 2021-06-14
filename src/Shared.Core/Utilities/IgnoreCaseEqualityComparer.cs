using System;
using System.Collections.Generic;

namespace KKAPI.MainGame
{
    /// <summary>
    /// An equality comparer that uses StringComparison.OrdinalIgnoreCase rule
    /// </summary>
    public class IgnoreCaseEqualityComparer : IEqualityComparer<string>
    {
        /// <inheritdoc />
        public bool Equals(string x, string y)
        {
            if (x == null || y == null) return false;
            return x.Equals(y, StringComparison.OrdinalIgnoreCase);
        }

        /// <inheritdoc />
        public int GetHashCode(string obj)
        {
            return obj.GetHashCode();
        }

        /// <summary>
        /// Instance of the comparer for use in linq and such
        /// </summary>
        public static readonly IgnoreCaseEqualityComparer Instance = new IgnoreCaseEqualityComparer();
    }
}