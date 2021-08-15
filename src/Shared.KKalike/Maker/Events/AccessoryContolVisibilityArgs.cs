using ChaCustom;
using System;
#pragma warning disable 612

namespace KKAPI.Maker
{
    /// <summary>
    /// Event args for events that are related to accessory control visibility.
    /// </summary>
    public class AccessoryContolVisibilityArgs : EventArgs
    {
        /// <inheritdoc />
        public AccessoryContolVisibilityArgs(bool _show)
        {
            Show = _show;
        }

        /// <summary>
        /// Visibility state of accessory control
        /// </summary>
        public bool Show { get; }
    }
}
