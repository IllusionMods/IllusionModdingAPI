using System;
using System.Collections.Generic;
using System.Linq;
using BepInEx;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

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
        public static readonly string GuiApiNameAppendix = "(MakerAPI)";
        private static Transform _guiCacheTransfrom;
        private readonly List<GameObject> _controlObjects = new List<GameObject>(1);

        private static readonly Type[] _localisationComponentTypes = typeof(Manager.Scene).Assembly.GetTypes()
            .Where(t => string.Equals(t.Namespace, "Localize.Translate", StringComparison.Ordinal))
            .Where(t => typeof(Component).IsAssignableFrom(t))
            .ToArray();

        /// <summary>
        /// Create a new custom control
        /// </summary>
        /// <param name="category">Category the control will be created under</param>
        /// <param name="owner">Plugin that owns the control</param>
        protected BaseGuiEntry(MakerCategory category, BaseUnityPlugin owner)
        {
            Category = category;
            Owner = owner;

            Visible = new BehaviorSubject<bool>(true);
            Visible.Subscribe(b =>
            {
                foreach (var controlObject in ControlObjects)
                    controlObject.SetActive(b);
            });
        }

        /// <summary>
        /// Category and subcategory that this control is inside of.
        /// </summary>
        public MakerCategory Category { get; internal set; }

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

        internal static void RemoveLocalisation(GameObject control)
        {
            foreach (var localisationComponentType in _localisationComponentTypes)
            {
                foreach (var localisationComponent in control.GetComponentsInChildren(localisationComponentType, true))
                    UnityEngine.Object.DestroyImmediate(localisationComponent, false);
            }
        }

        /// <summary>
        /// Find first control of this name under the specified category transform
        /// </summary>
        protected static Transform GetExistingControl(string categoryPath, string controlName)
        {
            var cat = GameObject.Find(categoryPath);
            var catTop = MakerAPI.FindSubcategoryContentParent(cat.transform);
            return catTop.Find(controlName);
        }

        /// <summary>
        /// Called before OnCreateControl to setup the object before instantiating the control.
        /// </summary>
        protected internal abstract void Initialize();

        /// <summary>
        /// Remove the control. Called when maker is quitting.
        /// </summary>
        /// <inheritdoc />
        public virtual void Dispose()
        {
            IsDisposed = true;
        }

        /// <summary>
        /// If true, the control has been disposed and can no longer be used, likely because the character maker exited.
        /// A new control has to be created to be used again.
        /// </summary>
        public bool IsDisposed { get; private set; }

        internal virtual void CreateControl(Transform subCategoryList)
        {
            var control = OnCreateControl(subCategoryList);

            control.name += GuiApiNameAppendix;

            // Play nice with the accessory window (lower max width)
            var layoutElement = control.GetComponent<LayoutElement>();
            if (layoutElement != null) layoutElement.minWidth = 300;

            control.SetActive(Visible.Value);
            _controlObjects.Add(control);
        }

        /// <summary>
        /// Used by the API to actually create the custom control.
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
        /// GameObject(s) of the control. Populated once instantiated.
        /// Contains 1 item in most cases, can contain multiple in case of accessory window controls.
        /// </summary>
        public IEnumerable<GameObject> ControlObjects => _controlObjects.Where(x => x != null);

        /// <summary>
        /// GameObject of the control. Populated once instantiated.
        /// If there are multiple objects, returns one of them. Use <see cref="ControlObjects"/> in that case.
        /// </summary>
        public GameObject ControlObject => _controlObjects.FirstOrDefault(x => x != null);

        /// <summary>
        /// True if the control is currently instantiated in the scene
        /// </summary>
        public bool Exists => _controlObjects.Any(x => x != null);
    }
}
