using System;

namespace KKAPI.Maker
{
    /// <summary>
    /// Event args used in <see cref="AccessoryControlWrapper{T,TVal}"/>.
    /// </summary>
    public sealed class AccessoryWindowControlValueChangedEventArgs<TVal> : EventArgs
    {
        /// <inheritdoc />
        public AccessoryWindowControlValueChangedEventArgs(TVal newValue, int slotIndex)
        {
            NewValue = newValue;
            SlotIndex = slotIndex;
        }

        /// <summary>
        /// Newly assigned value.
        /// </summary>
        public TVal NewValue { get; }

        /// <summary>
        /// Index of the accessory the value was assigned to.
        /// </summary>
        public int SlotIndex { get; }
    }
}
