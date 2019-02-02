using BepInEx;
using UnityEngine;

namespace MakerAPI
{
    public class MakerSeparator : BaseGuiEntry
    {
        private static Transform _sourceSeparator;

        protected override GameObject OnCreateControl(Transform subCategoryList)
        {
            var s = Object.Instantiate(SourceSeparator, subCategoryList, false);
            s.name = "Separate" + GuiApiNameAppendix;
            return s.gameObject;
        }

        protected internal override void Initialize()
        {
            if (_sourceSeparator == null)
                MakeCopy();
        }

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