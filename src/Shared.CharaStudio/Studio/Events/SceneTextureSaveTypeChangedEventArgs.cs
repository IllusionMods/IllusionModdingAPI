using System;

#pragma warning disable 1591

namespace KKAPI.Studio
{
    /// <summary>
    /// Event argument used for when texture save type for scenes is changed
    /// </summary>
    public sealed class SceneTextureSaveTypeChangedEventArgs : EventArgs
    {
        public SceneTextureSaveTypeChangedEventArgs(SceneTextureSaveType saveType)
        {
            NewSetting = saveType;
        }

        /// <summary>
        /// The new state of the setting
        /// </summary>
        public SceneTextureSaveType NewSetting { get; }
    }
}
