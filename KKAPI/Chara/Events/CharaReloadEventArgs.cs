using System;

namespace KKAPI.Chara
{
    public sealed class CharaReloadEventArgs : EventArgs
    {
        public CharaReloadEventArgs(ChaControl reloadedCharacter)
        {
            ReloadedCharacter = reloadedCharacter;
        }

        /// <summary>
        /// Can be null when all characters in a scene are reloaded
        /// </summary>
        public ChaControl ReloadedCharacter { get; }
    }
}
