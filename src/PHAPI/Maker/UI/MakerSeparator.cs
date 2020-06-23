using BepInEx;
using UnityEngine;

namespace KKAPI.Maker.UI
{
    /// <summary>
    /// Custom control that draws a simple horizontal separator
    /// </summary>
    public class MakerSeparator : BaseGuiEntry
    {
        /// <inheritdoc />
        protected override GameObject OnCreateControl(Transform subCategoryList)
        {
            return MakerAPI.GetMakerBase().CreateSpace(subCategoryList.gameObject).gameObject;
        }

        /// <inheritdoc />
        protected internal override void Initialize()
        {
        }

        /// <summary>
        /// Create a new custom control. Create and register it in <see cref="MakerAPI.RegisterCustomSubCategories"/>.
        /// </summary>
        /// <param name="category">Category the control will be created under</param>
        /// <param name="owner">Plugin that owns the control</param>
        public MakerSeparator(MakerCategory category, BaseUnityPlugin owner) : base(category, owner) { }
    }
}
