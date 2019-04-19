using System;

namespace KKAPI.Maker
{
    public sealed class AccessoryControlValueChangedEventArgs<TVal> : EventArgs
    {
        public AccessoryControlValueChangedEventArgs(TVal newValue, int accessoryIndex)
        {
            NewValue = newValue;
            AccessoryIndex = accessoryIndex;
        }

        /// <summary>
        /// Newly assigned value.
        /// </summary>
        public TVal NewValue { get; }

        /// <summary>
        /// Index of the accessory the value was assigned to.
        /// </summary>
        public int AccessoryIndex { get; }
    }
}