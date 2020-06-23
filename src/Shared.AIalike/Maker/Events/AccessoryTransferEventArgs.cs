using System;

namespace KKAPI.Maker
{
    /// <summary>
    /// Event args for accessory transfer events.
    /// </summary>
    public class AccessoryTransferEventArgs : EventArgs
    {
        /// <inheritdoc />
        public AccessoryTransferEventArgs(int sourceSlotIndex, int destinationSlotIndex)
        {
            SourceSlotIndex = sourceSlotIndex;
            DestinationSlotIndex = destinationSlotIndex;
        }

        /// <summary>
        /// Index of the source accessory.
        /// </summary>
        public int SourceSlotIndex { get; }

        /// <summary>
        /// Index the source accessory is copied to.
        /// </summary>
        public int DestinationSlotIndex { get; }
    }
}
