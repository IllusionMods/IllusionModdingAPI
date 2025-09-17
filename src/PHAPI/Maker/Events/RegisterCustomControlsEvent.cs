using System;
using KKAPI.Maker.UI;
using KKAPI.Maker.UI.Sidebar;

#pragma warning disable CS0618 // Type or member is obsolete

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
        /// Add a control to the right sidebar in chara maker (the "Control Panel" where you set eye blinking, mouth expressions etc.)
        /// </summary>
        public T AddSidebarControl<T>(T control) where T : BaseGuiEntry, ISidebarControl
        {
            return MakerAPI.AddSidebarControl(control);
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
