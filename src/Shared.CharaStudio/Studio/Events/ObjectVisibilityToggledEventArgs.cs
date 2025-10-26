using Studio;
using System;

namespace KKAPI.Studio.SaveLoad
{
    /// <summary>
    /// Arguments used in object deleted events
    /// </summary>
    /// <inheritdoc />
    public sealed class ObjectVisibilityToggledEventArgs : EventArgs
    {
        /// <inheritdoc />
        /// <param name="toggledObject">Object being toggled</param>
        /// <param name="visible">Visibility of the object</param>
        public ObjectVisibilityToggledEventArgs(ObjectCtrlInfo toggledObject, bool visible)
        {
            ToggledObject = toggledObject;
            Visible = visible;
        }

        /// <summary>
        /// Object being toggled
        /// </summary>
        public ObjectCtrlInfo ToggledObject { get; }

        /// <summary>
        /// Visibility of the object
        /// </summary>
        public bool Visible { get; }
    }
}
