using System;

namespace KKAPI.Studio.UI.Toolbars
{
    /// <summary>
    /// Represents the position of a toolbar in a grid layout, defined by its row and column indices.
    /// </summary>
    public readonly struct ToolbarPosition : IEquatable<ToolbarPosition>
    {
        /// <summary>
        /// Vertical position (row index).
        /// </summary>
        public readonly int Row;

        /// <summary>
        /// Horizontal position (column index).
        /// </summary>
        public readonly int Column;

        /// <summary>
        /// Initializes a new instance of the <see cref="ToolbarPosition"/> class with the specified row and column.
        /// </summary>
        public ToolbarPosition(int row, int column)
        {
            Row = row;
            Column = column;
        }

        /// <inheritdoc />
        public bool Equals(ToolbarPosition other) => Row == other.Row && Column == other.Column;

        /// <inheritdoc />
        public override bool Equals(object obj) => obj is ToolbarPosition other && Equals(other);

        /// <inheritdoc />
        public override int GetHashCode()
        {
            unchecked
            {
                return (Row * 397) ^ Column;
            }
        }
    }
}
