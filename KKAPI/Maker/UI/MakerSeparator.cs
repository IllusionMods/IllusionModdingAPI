using BepInEx;
using UnityEngine;

namespace KKAPI.Maker.UI
{
    /// <summary>
    /// Custom control that draws a simple horizontal separator
    /// </summary>
    public class MakerSeparator : BaseGuiEntry
    {
        private static Transform _sourceSeparator;

        /// <inheritdoc />
        protected override GameObject OnCreateControl(Transform subCategoryList)
        {
            var s = Object.Instantiate(SourceSeparator, subCategoryList, false);
            s.name = "Separate" + GuiApiNameAppendix;
            return s.gameObject;
        }

        /// <inheritdoc />
        protected internal override void Initialize()
        {
            if (_sourceSeparator == null)
                MakeCopy();
        }

        /// <inheritdoc />
        public override void Dispose()
        {
        }

        private static Transform SourceSeparator
        {
            get
            {
                if (_sourceSeparator == null)
                    MakeCopy();

                return _sourceSeparator;
            }
        }

        private static void MakeCopy()
        {

            // Exists in male and female maker
            _sourceSeparator = GameObject.Find("CustomScene/CustomRoot/FrontUIGroup/CustomUIGroup/CvsMenuTree/00_FaceTop/tglAll/AllTop/Separate").transform;
        }

        public MakerSeparator(MakerCategory category, BaseUnityPlugin owner) : base(category, owner)
        {
        }
    }
}