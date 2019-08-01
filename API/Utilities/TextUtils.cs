using System;
using System.Text.RegularExpressions;

namespace KKAPI.Utilities
{
    /// <summary>
    /// Utility methods for working with text.
    /// </summary>
    public static class TextUtils
    {
        /// <summary>
        /// Convert PascalCase to Sentence case.
        /// </summary>
        public static string PascalCaseToSentenceCase(this string str)
        {
            if (str == null) throw new ArgumentNullException(nameof(str));
            return Regex.Replace(str, "[a-z][A-Z]", m => $"{m.Value[0]} {char.ToLower(m.Value[1])}");
        }
    }
}