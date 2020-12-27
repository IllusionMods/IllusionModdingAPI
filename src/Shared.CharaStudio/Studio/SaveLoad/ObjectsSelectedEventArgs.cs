using Studio;
using System;
using System.Collections.Generic;

namespace KKAPI.Studio.SaveLoad
{
    /// <summary>
    /// Arguments used in object deleted events
    /// </summary>
    /// <inheritdoc />
    public sealed class ObjectsSelectedEventArgs : EventArgs
    {
        /// <inheritdoc />
        /// <param name="selectedObjects">Objects being selected</param>
        public ObjectsSelectedEventArgs(List<ObjectCtrlInfo> selectedObjects)
        {
            SelectedObjects = selectedObjects;
        }

        /// <summary>
        /// Object modified by the event
        /// </summary>
        public List<ObjectCtrlInfo> SelectedObjects { get; }
    }
}
