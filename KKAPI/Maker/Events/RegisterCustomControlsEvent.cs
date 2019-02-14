using System;
using KKAPI.Maker.UI;

namespace KKAPI.Maker
{
    /// <summary>
    /// Event fired when character maker is starting and plugins are given an opportunity to register custom controls
    /// </summary>
    public class RegisterCustomControlsEvent : EventArgs
    {
        /// <summary>
        /// Add custom controls. If you want to use custom sub categories, register them by calling AddSubCategory.
        /// </summary>
        public T AddControl<T>(T control) where T : BaseGuiEntry
        {
            return MakerAPI.AddControl(control);
        }

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