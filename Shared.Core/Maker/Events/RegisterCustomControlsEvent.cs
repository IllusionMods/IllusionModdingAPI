using System;
using KKAPI.Maker.UI;
using KKAPI.Maker.UI.Sidebar;

namespace KKAPI.Maker
{
    /// <summary>
    /// Event fired when character maker is starting and plugins are given an opportunity to register custom controls
    /// </summary>
    public partial class RegisterCustomControlsEvent : EventArgs
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
    }
}
