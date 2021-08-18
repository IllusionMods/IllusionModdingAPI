using System.Linq;
using CharaCustom;
using Illusion.Extensions;
using UnityEngine;
using UnityEngine.UI;

namespace KKAPI.Maker.UI
{
    internal static class SubCategoryCreator
    {
        private static string GetSubcategoryDisplayName(MakerCategory subCategory)
        {
            if (subCategory.DisplayName != null) return subCategory.DisplayName;
            return subCategory.SubCategoryName;
        }

        public static CvsSelectWindow.ItemInfo AddNewSubCategory(Transform modsCategoryTop, MakerCategory subCategory, Transform window)
        {
            var btn = Object.Instantiate(GameObject.Find("CharaCustom/CustomControl/CanvasMain/SubMenu/SubMenuHair/Scroll View/Viewport/Content/Category/CategoryTop/HairSetting"), modsCategoryTop);
            btn.name = subCategory.SubCategoryName;

            btn.GetComponentInChildren<Text>().text = GetSubcategoryDisplayName(subCategory);

            var winContents = Object.Instantiate(GameObject.Find("B_ShapeWhole"), window);
            winContents.name = "AIAPI_" + subCategory.SubCategoryName;
            winContents.transform.Find("title").GetComponentInChildren<Text>().text = GetSubcategoryDisplayName(subCategory);
            foreach (var transform in winContents.transform.Find("Scroll View/Viewport/Content").Cast<Transform>())
                Object.Destroy(transform.gameObject);

            var canvasGroup = winContents.GetComponent<CanvasGroup>();
            canvasGroup.Enable(false);

            var btnItem = btn.GetComponent<UI_ButtonEx>();

            var itemInfo = new CvsSelectWindow.ItemInfo
            {
                btnItem = btnItem,
                cgItem = new[] { canvasGroup }
            };

            if (window.Find("imgWinBack") == null)
            {
                var settingsWindow = GameObject.Find("CharaCustom/CustomControl/CanvasSub/SettingWindow").GetComponent<CvsSettingWindow>();

                var origWinBack = GameObject.Find("CharaCustom/CustomControl/CanvasSub/SettingWindow/WinHair/imgWinBack");
                var origDragRect = GameObject.Find("CharaCustom/CustomControl/CanvasSub/SettingWindow/WinClothes/DefaultWin/DragRect");

                var copyWinBack = Object.Instantiate(origWinBack, winContents.transform);
                copyWinBack.transform.SetSiblingIndex(0);
                copyWinBack.AddComponent<LayoutElement>().ignoreLayout = true;

                var winBackRectTransform = copyWinBack.GetComponent<RectTransform>();
                winBackRectTransform.offsetMin = new Vector2(0, 300);
                winBackRectTransform.offsetMax = new Vector2(0, 0);

                var copyDragRect = Object.Instantiate(origDragRect, winContents.transform);
                copyDragRect.transform.SetSiblingIndex(1);
                copyDragRect.AddComponent<LayoutElement>().ignoreLayout = true;
                
                var closeButton = copyWinBack.GetComponentInChildren<UI_ButtonEx>();
                closeButton.onClick = new Button.ButtonClickedEvent();
                closeButton.onClick.AddListener(delegate
                {
                    settingsWindow.cgbaseWindow.alpha = 0f;
                    settingsWindow.cgbaseWindow.interactable = false;
                    settingsWindow.cgbaseWindow.blocksRaycasts = false;
                });
            }
            
            /* todo fix for missing window background. Very messy, messes up recttransform values, try to find a way that doesn't change the structure.
            // Deal with some categories having a different window back image per subcategory
            if (window.Find("imgWinBack") == null)
            {
                var subContainer = new GameObject("contents");
                subContainer.transform.SetParent(winContents.transform);

                GameObject.DestroyImmediate(winContents.GetComponent<VerticalLayoutGroup>());
                var vg = subContainer.AddComponent<VerticalLayoutGroup>();
                vg.spacing = 8;
                vg.childForceExpandWidth = true;
                vg.childForceExpandHeight = false;
                vg.childControlWidth = true;
                vg.childControlWidth = true;

                foreach (var childTransform in winContents.transform.Cast<Transform>().ToList())
                    childTransform.SetParent(subContainer.transform, true);

                // Clothes category has each content group with its own back image for some reason
                var instance = GameObject.Instantiate(GameObject.Find("SettingWindow/WinFace/imgWinBack"), winContents.transform);
                instance.transform.SetAsFirstSibling();
            }

            if (winContents.name == "WinClothes")
            {
                var rt = winContents.GetComponent<RectTransform>();
                rt.anchorMin = Vector2.one;
                rt.sizeDelta = new Vector2(0, 564);
            }*/

            return itemInfo;
        }
    }
}
