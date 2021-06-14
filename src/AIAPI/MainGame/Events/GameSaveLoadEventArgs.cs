using System;

namespace KKAPI.MainGame
{
    /// <summary>
    /// Arguments used with main game save/load events.
    /// </summary>
    public sealed class GameSaveLoadEventArgs : EventArgs
    {
        /// <summary>
        /// Create a new instance
        /// </summary>
        public GameSaveLoadEventArgs(string path, string fileName)
        {
            Path = path;
            FileName = fileName;
        }

        /// <summary>
        /// Name of the safe file.
        /// </summary>
        public string FileName { get; }

        /// <summary>
        /// Full filename of the save file.
        /// </summary>
        public string FullFilename => System.IO.Path.Combine(Path, FileName);

        /// <summary>
        /// Path to which the save file will be written.
        /// </summary>
        public string Path { get; }
    }
}
