using System;

#pragma warning disable 1591

namespace KKAPI.Chara
{
    /// <summary>
    /// Event arguments used by character reload events
    /// </summary>
    public sealed class CharaReloadEventArgs : EventArgs
    {
        public CharaReloadEventArgs(Human reloadedCharacter)
        {
            ReloadedCharacter = reloadedCharacter;
        }

        /// <summary>
        /// Can be null when all characters in a scene are reloaded
        /// </summary>
        public Human ReloadedCharacter { get; }
    }
}
