using BepInEx;
using UnityEngine;

namespace KKAPI.Maker.UI.Sidebar
{
    /// <summary>
    /// A separator to be used in the right "Control Panel" sidebar in character maker.
    /// The space is limited so use sparingly.
    /// </summary>
    public class SidebarSeparator : BaseGuiEntry, ISidebarControl
    {
        /// <inheritdoc />
        public SidebarSeparator(BaseUnityPlugin owner) : base(null, owner) { }

        /// <inheritdoc />
        protected internal override void Initialize() { }

        /// <inheritdoc />
        protected override GameObject OnCreateControl(Transform subCategoryList)
        {
            var orig = GameObject.Find("CustomScene/CustomRoot/FrontUIGroup/CvsDraw/Top/Separate");
            var copy = Object.Instantiate(orig, orig.transform.parent);
            return copy;
        }
    }
}
