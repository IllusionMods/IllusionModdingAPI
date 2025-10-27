using System;

#pragma warning disable 1591

namespace KKAPI.Maker
{
    /// <summary>
    /// Event argument used for when save type for cards is toggled
    /// </summary>
    public sealed class CharaTextureSaveTypeChangedEventArgs : EventArgs
    {
        public CharaTextureSaveTypeChangedEventArgs(CharaTextureSaveType saveType)
        {
            NewSetting = saveType;
        }

        /// <summary>
        /// The new state of the setting
        /// </summary>
        public CharaTextureSaveType NewSetting { get; }
    }
}
