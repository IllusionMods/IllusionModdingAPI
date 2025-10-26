using System;

#pragma warning disable 1591

namespace KKAPI.Maker
{
    /// <summary>
    /// Event argument used for when save type for cards is toggled
    /// </summary>
    public sealed class LocalSaveChangedEventArgs : EventArgs
    {
        public LocalSaveChangedEventArgs(LocalTextures.TextureSaveType saveType)
        {
            NewSetting = saveType;
        }

        /// <summary>
        /// The new state of the setting
        /// </summary>
        public LocalTextures.TextureSaveType NewSetting { get; }
    }
}
