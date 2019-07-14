using System;
using System.Collections.Generic;
using System.Linq;
using Studio;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace KKAPI.Studio.UI
{
    /// <summary>
    /// Category under the Anim > CustomState tab
    /// </summary>
    public class CurrentStateCategory
    {
        private static GameObject _originalText;

        private readonly List<CurrentStateCategorySubItemBase> _subItems;
        private GameObject _categoryContentsObject;

        /// <summary>
        /// Create a new custom CurrentState category
        /// </summary>
        /// <param name="categoryName">Name of the category</param>
        /// <param name="subItems">Controls under this category</param>
        [Obsolete("Manually creating categories is no longer supported")]
        public CurrentStateCategory(string categoryName, IEnumerable<CurrentStateCategorySubItemBase> subItems)
        {
            CategoryName = categoryName;
            _subItems = subItems.ToList();
        }

        /// <summary>
        /// Create a new custom CurrentState category
        /// </summary>
        internal CurrentStateCategory(string categoryName)
        {
            CategoryName = categoryName;
            _subItems = new List<CurrentStateCategorySubItemBase>();
        }

        /// <summary>
        /// Name of the category. Controls are drawn under it.
        /// </summary>
        public string CategoryName { get; }

        /// <summary>
        /// All custom controls under this category.
        /// </summary>
        public IEnumerable<CurrentStateCategorySubItemBase> SubItems => _subItems;

        /// <summary>
        /// Used by the API to actually create the custom control object
        /// </summary>
        protected internal virtual void CreateCategory(GameObject containerObject)
        {
            if (containerObject == null) throw new ArgumentNullException(nameof(containerObject));
            if (!StudioAPI.StudioLoaded) throw new InvalidOperationException("Called before studio was loaded");

            if (Created) return;

            if (_originalText == null)
                _originalText = GameObject.Find("StudioScene/Canvas Main Menu/02_Manipulate/00_Chara/01_State/Viewport/Content/Cos/Text");

            var cat = Object.Instantiate(_originalText, containerObject.transform, true);
            cat.AddComponent<LayoutElement>();
            cat.name = CategoryName + "_Header_SAPI";

            var t = cat.GetComponent<Text>();
            t.fontSize = 15;
            t.resizeTextMaxSize = 15;
            t.text = CategoryName;

            _categoryContentsObject = new GameObject(CategoryName + "_Items_SAPI");
            _categoryContentsObject.transform.SetParent(containerObject.transform);

            var rt = _categoryContentsObject.AddComponent<RectTransform>();
            // Fixes issue with left offset being wrong
            rt.pivot = new Vector2(0, 0.5f);
            rt.localScale = Vector3.one;

            var vlg = _categoryContentsObject.AddComponent<VerticalLayoutGroup>();
            vlg.padding = new RectOffset(0, 0, 0, 0);
            vlg.spacing = 4;
            vlg.childAlignment = TextAnchor.UpperLeft;

            CreateControls(SubItems);
        }

        /// <summary>
        /// The category was created and still exists.
        /// </summary>
        public bool Created => _categoryContentsObject != null;

        /// <summary>
        /// Fired when currently selected character changes and the controls need to be updated
        /// </summary>
        /// <param name="ociChar">Newly selected character</param>
        protected internal virtual void UpdateInfo(OCIChar ociChar)
        {
            foreach (var subItem in SubItems)
                subItem.OnUpdateInfo(ociChar);
        }

        /// <summary>
        /// Add new control to this category
        /// </summary>
        public T AddControl<T>(T control) where T : CurrentStateCategorySubItemBase
        {
            _subItems.Add(control);

            if (StudioAPI.StudioLoaded)
                CreateControls(new CurrentStateCategorySubItemBase[] { control });

            return control;
        }

        /// <summary>
        /// Add new controls to this category
        /// </summary>
        public void AddControls(params CurrentStateCategorySubItemBase[] controls)
        {
            _subItems.AddRange(controls);

            if (StudioAPI.StudioLoaded)
                CreateControls(controls);
        }

        private void CreateControls(IEnumerable<CurrentStateCategorySubItemBase> controls)
        {
            foreach (var control in controls)
                control.CreateItemInt(_categoryContentsObject);
        }
    }
}
