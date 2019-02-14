using System;
using BepInEx;
using UniRx;
using UnityEngine;

namespace KKAPI.Maker.UI
{
    /// <summary>
    /// Base of all custom character maker controls.
    /// </summary>
    public abstract class BaseGuiEntry : IDisposable
    {
        /// <summary>
        /// Added to the end of most custom controls to mark them as being created by this API.
        /// </summary>
        public static readonly string GuiApiNameAppendix = "(GUIAPI)";
        private static Transform _guiCacheTransfrom;

        protected BaseGuiEntry(MakerCategory category, BaseUnityPlugin owner)
        {
            Category = category;
            Owner = owner;

            Visible = new BehaviorSubject<bool>(true);
            Visible.Subscribe(b => ControlObject?.SetActive(b));
        }

        /// <summary>
        /// Category and subcategory that this control is inside of.
        /// </summary>
        public MakerCategory Category { get; }

        /// <summary>
        /// Parent transform that holds temporary gui entries used to instantiate custom controls.
        /// </summary>
        protected internal static Transform GuiCacheTransfrom
        {
            get
            {
                if (_guiCacheTransfrom == null)
                {
                    var obj = new GameObject(nameof(MakerAPI) + " Cache");
                    obj.transform.SetParent(KoikatuAPI.Instance.transform);
                    _guiCacheTransfrom = obj.transform;
                }
                return _guiCacheTransfrom;
            }
        }

        /// <summary>
        /// Called before OnCreateControl to setup the object before instantiating the control.
        /// </summary>
        protected internal abstract void Initialize();

        /// <summary>
        /// Remove the control. Called when maker is quitting.
        /// </summary>
        /// <inheritdoc />
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

        /// <summary>
        /// Text color of the control's description text (usually on the left).
        /// Can only set this before the control is created.
        /// </summary>
        public Color TextColor { get; set; } = Color.white;

        /// <summary>
        /// The plugin that owns this custom control.
        /// </summary>
        public BaseUnityPlugin Owner { get; }

        /// <summary>
        /// The control is visible to the user (usually the same as it's GameObject being active).
        /// </summary>
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