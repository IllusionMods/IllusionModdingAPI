using System;
using System.Collections.Generic;

namespace KKAPI.Maker
{
    /// <summary>
    /// Event args for accessory copy events.
    /// </summary>
    public class AccessoryCopyEventArgs : EventArgs
    {
        /// <inheritdoc />
        public AccessoryCopyEventArgs(IEnumerable<int> copiedSlotIndexes,
            ChaFileDefine.CoordinateType copySource, ChaFileDefine.CoordinateType copyDestination)
        {
            CopiedSlotIndexes = copiedSlotIndexes ?? throw new ArgumentNullException(nameof(copiedSlotIndexes));
            CopySource = copySource;
            CopyDestination = copyDestination;
        }

        /// <summary>
        /// Indexes of accessories that were selected to be copied.
        /// </summary>
        public IEnumerable<int> CopiedSlotIndexes { get; }

        /// <summary>
        /// Coordinate the accessories are copied from.
        /// </summary>
        public ChaFileDefine.CoordinateType CopySource { get; }

        /// <summary>
        /// Coordinate the accessories are copied into.
        /// </summary>
        public ChaFileDefine.CoordinateType CopyDestination { get; }
    }
}
