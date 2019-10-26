using System;
using KKAPI.Utilities;
using Studio;

namespace KKAPI.Studio.SaveLoad
{
    /// <summary>
    /// Arguments used in objects copied events
    /// </summary>
    /// <inheritdoc />
    public sealed class ObjectsCopiedEventArgs : EventArgs
    {
        /// <inheritdoc />
        /// <param name="loadedObjects">Objects copied by the event</param>
        public ObjectsCopiedEventArgs(ReadOnlyDictionary<int, ObjectCtrlInfo> loadedObjects)
        {
            LoadedObjects = loadedObjects;
        }

        /// <summary>
        /// Objects copied by the event and their original IDs
        /// </summary>
        public ReadOnlyDictionary<int, ObjectCtrlInfo> LoadedObjects { get; }
    }
}
