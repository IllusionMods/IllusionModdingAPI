using System;
using KKAPI.Maker.UI;

namespace KKAPI.Maker
{
    /// <summary>
    /// Event fired when character maker is starting and plugins are given an opportunity to register custom controls
    /// </summary>
    public partial class RegisterCustomControlsEvent : EventArgs
    {
        /// <summary>
        /// Add a toggle to the bottom of the "Load character" window that allows for partial loading of characters.
        /// </summary>
        public MakerLoadToggle AddLoadToggle(MakerLoadToggle toggle)
        {
            return MakerLoadToggle.AddLoadToggle(toggle);
        }

        /// <summary>
        /// Add a toggle to the bottom of the "Load coordinate/clothes" window that allows for partial loading of coordinate cards.
        /// </summary>
        public MakerCoordinateLoadToggle AddCoordinateLoadToggle(MakerCoordinateLoadToggle toggle)
        {
            return MakerCoordinateLoadToggle.AddLoadToggle(toggle);
        }
    }
}
