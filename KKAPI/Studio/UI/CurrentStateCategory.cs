using System.Collections.Generic;
using System.Linq;
using Studio;
using UnityEngine;
using UnityEngine.UI;

namespace KKAPI.Studio.UI {
    /// <summary>
    /// Category under the Anim > CustomState tab
    /// </summary>
    public class CurrentStateCategory {
        /// <summary>
        /// Create a new custom CurrentState category
        /// </summary>
        /// <param name="categoryName">Name of the category</param>
        /// <param name="subItems">Controls under this category</param>
        /// public CurrentStateCategory(string categoryName, IEnumerable<CurrentStateCategorySubItemBase> subItems, bool showHeader = true) {
        public CurrentStateCategory(string categoryName, IEnumerable<CurrentStateCategorySubItemBase> subItems) {
            CategoryName = categoryName;
            SubItems = subItems.ToList();
            //this.showHeader = showHeader;
        }

        /// <summary>
        /// Name of the category. Controls are drawn under it.
        /// </summary>
        public string CategoryName { get; }
        //private bool showHeader;

        /// <summary>
        /// All custom controls under this category.
        /// </summary>
        public IEnumerable<CurrentStateCategorySubItemBase> SubItems { get; }

        /// <summary>
        /// Used by the API to actually create the custom control object
        /// </summary>             
       
        protected internal virtual void CreateCategory(GameObject containerObject) {
            var original = GameObject.Find("StudioScene/Canvas Main Menu/02_Manipulate/00_Chara/01_State/Viewport/Content/Cos/Text");

            var cat = Object.Instantiate(original, containerObject.transform, true);
            cat.AddComponent<LayoutElement>();

            cat.name = CategoryName + "_Header_SAPI";

            var t = cat.GetComponent<Text>();
          
                t.fontSize = 15;
                t.resizeTextMaxSize = 15;
                t.text = CategoryName; 

            var catContents = new GameObject(CategoryName + "_Items_SAPI");
            catContents.transform.SetParent(containerObject.transform);

            var rt = catContents.AddComponent<RectTransform>();
            // Fixes issue with left offset being wrong
            rt.pivot = new Vector2(0, 0.5f);
            rt.localScale = Vector3.one;

            var vlg = catContents.AddComponent<VerticalLayoutGroup>();
            vlg.padding = new RectOffset(0, 0, 0, 0);
            vlg.spacing = 4;
            vlg.childAlignment = TextAnchor.UpperLeft;            if (CategoryName.IsNullOrEmpty()) cat.SetActive(false);
            // if (!showHeader) cat.SetActive(false);
            foreach (var subItem in SubItems)
                subItem.CreateItem(catContents);
        }

        /// <summary>
        /// Fired when currently selected character changes and the controls need to be updated
        /// </summary>
        /// <param name="ociChar">Newly selected character</param>
        protected internal virtual void UpdateInfo(OCIChar ociChar) {
            foreach (var subItem in SubItems)
                subItem.OnUpdateInfo(ociChar);
        }
    }
}