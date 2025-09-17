using System;
using BepInEx;
using UnityEngine;

namespace KKAPI.Maker.UI.Sidebar
{
    /// <inheritdoc cref="ISidebarControl"/>
    [Obsolete("Not supported")]
    public class SidebarSeparator : BaseGuiEntry, ISidebarControl
    {
        /// <inheritdoc />
        public SidebarSeparator(BaseUnityPlugin owner) : base(null, owner) { }

        /// <inheritdoc />
        protected internal override void Initialize() { }

        /// <inheritdoc />
        protected override GameObject OnCreateControl(Transform subCategoryList)
        {
            throw new NotImplementedException();
        }
    }
}
