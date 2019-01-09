using System.Collections.Generic;
using System.Linq;
using Studio;
using UnityEngine;
using UnityEngine.UI;

namespace MakerAPI.Studio
{
    public class CurrentStateCategory
    {
        public CurrentStateCategory(string categoryName, IEnumerable<CurrentStateCategorySubItemBase> subItems)
        {
            CategoryName = categoryName;
            SubItems = subItems.ToList();
        }
        public string CategoryName { get; }
        public IEnumerable<CurrentStateCategorySubItemBase> SubItems { get; }

        protected internal virtual void CreateCategory(GameObject containerObject)
        {
            var original = GameObject.Find("StudioScene/Canvas Main Menu/02_Manipulate/00_Chara/01_State/Viewport/Content/Cos/Text");

            var cat = Object.Instantiate(original, containerObject.transform, true);
            cat.AddComponent<LayoutElement>();
            cat.name = "StudioAPI-Header-" + CategoryName;

            var t = cat.GetComponent<Text>();
            t.fontSize = 15;
            t.resizeTextMaxSize = 15;
            t.text = CategoryName;

            var catContents = new GameObject("StudioAPI-Category-" + CategoryName);
            catContents.transform.SetParent(containerObject.transform);
            catContents.AddComponent<RectTransform>().anchorMin = new Vector2(0, 1.04f);
            var vlg = catContents.AddComponent<VerticalLayoutGroup>();
            vlg.padding = new RectOffset(0, 0, 4, 4);

            foreach (var subItem in SubItems)
                subItem.CreateItem(catContents);
        }

        protected internal virtual void UpdateInfo(OCIChar ociChar)
        {
            foreach (var subItem in SubItems)
                subItem.UpdateInfo(ociChar);
        }
    }
}