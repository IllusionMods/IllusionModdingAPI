using BepInEx;
using UnityEngine;

namespace MakerAPI
{
    public class MakerSeparator : BaseGuiEntry
    {
        private static Transform _sourceSeparator;
        
        protected internal override void CreateControl(Transform subCategoryList)
        {
            var s = Object.Instantiate(SourceSeparator, subCategoryList, false);
            s.name = "Separate" + GuiApiNameAppendix;
        }

        public override void Dispose()
        {
        }

        private static Transform SourceSeparator
        {
            get
            {
                if (_sourceSeparator == null)
                {
                    // Exists in male and female maker
                    _sourceSeparator = GameObject.Find("CustomScene/CustomRoot/FrontUIGroup/CustomUIGroup/CvsMenuTree/00_FaceTop/tglAll/AllTop/Separate").transform;
                }

                return _sourceSeparator;
            }
        }

        public MakerSeparator(MakerCategory category, BaseUnityPlugin owner) : base(category, owner)
        {
        }
    }
}