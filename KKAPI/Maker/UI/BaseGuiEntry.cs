using System;
using BepInEx;
using UniRx;
using UnityEngine;

namespace KKAPI.Maker.UI
{
    public abstract class BaseGuiEntry : IDisposable
    {
        public static readonly string GuiApiNameAppendix = "(GUIAPI)";
        private static Transform _guiCacheTransfrom;

        protected BaseGuiEntry(MakerCategory category, BaseUnityPlugin owner)
        {
            Category = category;
            Owner = owner;

            Visible = new BehaviorSubject<bool>(true);
            Visible.Subscribe(b => ControlObject?.SetActive(b));
        }

        public MakerCategory Category { get; }

        protected internal static Transform GuiCacheTransfrom
        {
            get
            {
                if (_guiCacheTransfrom == null)
                {
                    var obj = new GameObject(nameof(MakerAPI) + " Cache");
                    obj.transform.SetParent(GameAPI.Instance.transform);
                    _guiCacheTransfrom = obj.transform;
                }
                return _guiCacheTransfrom;
            }
        }

        protected internal abstract void Initialize();

        public abstract void Dispose();

        internal void CreateControl(Transform subCategoryList)
        {
            ControlObject = OnCreateControl(subCategoryList);
            ControlObject.SetActive(Visible.Value);
        }
        /// <summary>
        /// Should return main GameObject of the control
        /// </summary>
        protected abstract GameObject OnCreateControl(Transform subCategoryList);

        public Color TextColor { get; set; } = Color.white;
        public BaseUnityPlugin Owner { get; }

        public BehaviorSubject<bool> Visible { get; }

        /// <summary>
        /// GameObject of the control. Populated once instantiated
        /// </summary>
        public GameObject ControlObject { get; private set; }

        /// <summary>
        /// True if the control is currently instantiated in the scene
        /// </summary>
        public bool Exists => ControlObject != null;
    }
}