using System;
using System.Collections.Generic;
using UnityEngine;

namespace KKAPI.Utilities
{
    /// <summary>
    /// General utility extensions that don't fit in other categories.
    /// </summary>
    public static class Extensions
    {
        /// <summary>
        /// Wrap this dictionary in a read-only wrapper that will prevent any changes to it. 
        /// Warning: Any reference types inside the dictionary can still be modified.
        /// </summary>
        public static ReadOnlyDictionary<TKey, TValue> ToReadOnlyDictionary<TKey, TValue>(this IDictionary<TKey, TValue> original)
        {
            return new ReadOnlyDictionary<TKey, TValue>(original);
        }

        /// <summary>
        /// Mark GameObject of this Component as ignored by AutoTranslator. Prevents AutoTranslator from trying to translate custom UI elements.
        /// </summary>
        public static void MarkXuaIgnored(this Component target)
        {
            if (target == null) throw new ArgumentNullException(nameof(target));
            target.gameObject.name += "(XUAIGNORE)";
        }
    }
}
