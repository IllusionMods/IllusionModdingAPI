using System;
using BepInEx;
using UnityEngine;

namespace MakerAPI
{
    public abstract class MakerGuiEntryBase : IDisposable
    {
        public static readonly string GuiApiNameAppendix = "(GUIAPI)";
        private static Transform _guiCacheTransfrom;

        protected MakerGuiEntryBase(MakerCategory category, BaseUnityPlugin owner)
        {
            Category = category;
            Owner = owner;
        }

        public MakerCategory Category { get; }

        protected static Transform GuiCacheTransfrom
        {
            get
            {
                if (_guiCacheTransfrom == null)
                {
                    var obj = new GameObject(nameof(MakerAPI) + " Cache");
                    obj.transform.SetParent(MakerAPI.Instance.transform);
                    _guiCacheTransfrom = obj.transform;
                }
                return _guiCacheTransfrom;
            }
        }

        public abstract void Dispose();
        protected internal abstract void CreateControl(Transform subCategoryList);

        public Color TextColor { get; set; } = Color.white;
        public BaseUnityPlugin Owner { get; }
    }
}