using System;
using KKAPI.Utilities;
using Studio;

namespace KKAPI.Studio.SaveLoad
{
    /// <summary>
    /// Arguments used in scene loaded/imported events
    /// </summary>
    /// <inheritdoc />
    public sealed class SceneLoadEventArgs : EventArgs
    {
        /// <inheritdoc />
        /// <param name="operation">Operation that caused the event</param>
        /// <param name="loadedObjects">Objects loaded by the event</param>
        public SceneLoadEventArgs(SceneOperationKind operation, ReadOnlyDictionary<int, ObjectCtrlInfo> loadedObjects)
        {
            Operation = operation;
            LoadedObjects = loadedObjects;
        }

        /// <summary>
        /// Operation that caused the event
        /// </summary>
        public SceneOperationKind Operation { get; }

        /// <summary>
        /// Objects loaded by the event and their original IDs (from the time the scene was saved)
        /// </summary>
        public ReadOnlyDictionary<int, ObjectCtrlInfo> LoadedObjects { get; }
    }
}
