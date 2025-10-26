using System;

#pragma warning disable 1591

namespace KKAPI
{
    /// <summary>
    /// Event argument used for when save type for cards is toggled
    /// </summary>
    public sealed class LocalSaveChangedEventArgs : EventArgs
    {
        public LocalSaveChangedEventArgs(TextureSaveType saveType)
        {
            NewSetting = saveType;
        }

        /// <summary>
        /// The new state of the setting
        /// </summary>
        public TextureSaveType NewSetting { get; }
    }
}
