using Studio;
using System;

namespace KKAPI.Studio.SaveLoad
{
    /// <summary>
    /// Arguments used in object deleted events
    /// </summary>
    /// <inheritdoc />
    public sealed class ObjectDeletedEventArgs : EventArgs
    {
        /// <inheritdoc />
        /// <param name="deletedObject">Object being deleted</param>
        public ObjectDeletedEventArgs(ObjectCtrlInfo deletedObject)
        {
            DeletedObject = deletedObject;
        }

        /// <summary>
        /// Object deleted by the event
        /// </summary>
        public ObjectCtrlInfo DeletedObject { get; }
    }
}
