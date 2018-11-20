using System.Collections;
using BepInEx.Logging;
using ChaCustom;
using Harmony;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Logger = BepInEx.Logger;

namespace MakerAPI
{
    internal class SubCategoryCreator
    {
        private static Transform _subCategoryCopy;
        private static Transform _scrollbarCopy;

        private static Transform SubCategoryCopy
        {
            get
            {
                if (_subCategoryCopy == null)
                {
                    // 00_FaceTop/tglEar is present in Both male and female maker
                    var original = GameObject.Find("00_FaceTop").transform.Find("tglEar");

                    _subCategoryCopy = Object.Instantiate(original, BaseGuiEntry.GuiCacheTransfrom, true);
                    _subCategoryCopy.gameObject.SetActive(false);

                    var toggle = _subCategoryCopy.GetComponent<Toggle>();
                    toggle.onValueChanged.RemoveAllListeners();
                    toggle.isOn = false;

                    _subCategoryCopy.name = "CustomSubcategory" + BaseGuiEntry.GuiApiNameAppendix;
                    var copyTop = _subCategoryCopy.Find("EarTop");
                    copyTop.name = "CustomSubcategoryTop";

                    Object.DestroyImmediate(copyTop.GetComponent<CvsEar>());

                    // This doesn't happen until after we return from the hook, 
                    // so these will still be copied and need to be deleted afterwards
                    foreach (Transform tr in copyTop)
                        Object.Destroy(tr.gameObject);

                    foreach (var renderer in _subCategoryCopy.GetComponentsInChildren<Image>())
                        renderer.raycastTarget = true;

                    var originalScroll = GameObject.Find("CustomScene/CustomRoot/FrontUIGroup/CustomUIGroup/CvsMenuTree/03_ClothesTop/tglSocks/SocksTop/Scroll View");
                    _scrollbarCopy = Object.Instantiate(originalScroll.transform, BaseGuiEntry.GuiCacheTransfrom, true);
                    _scrollbarCopy.gameObject.SetActive(false);

                    foreach (Transform tr in _scrollbarCopy.Find("Viewport/Content"))
                        Object.Destroy(tr.gameObject);

                    Object.DestroyImmediate(copyTop.GetComponent<Image>());
                    Object.DestroyImmediate(copyTop.GetComponent<ContentSizeFitter>());
                    Object.DestroyImmediate(copyTop.GetComponent<VerticalLayoutGroup>());
                    Object.DestroyImmediate(copyTop.GetComponent<CanvasRenderer>());
                }
                return _subCategoryCopy;
            }
        }

        private static string GetSubcategoryDisplayName(MakerCategory subCategory)
        {
            if (subCategory.DisplayName != null) return subCategory.DisplayName;
            return subCategory.SubCategoryName.StartsWith("tgl") ? subCategory.SubCategoryName.Substring(3) : subCategory.SubCategoryName;
        }

        public static Transform AddNewSubCategory(UI_ToggleGroupCtrl mainCategory, MakerCategory subCategory)
        {
            Logger.Log(LogLevel.Debug, $"[MakerAPI] Adding custom subcategory {subCategory.SubCategoryName} to {mainCategory.transform.name}");

            var tr = Object.Instantiate(SubCategoryCopy.gameObject, mainCategory.transform, true).transform;
            tr.name = subCategory.SubCategoryName;
            var trTop = tr.Find("CustomSubcategoryTop");
            trTop.name = subCategory.SubCategoryName + "Top";

            foreach (Transform oldObj in trTop.transform)
                Object.Destroy(oldObj.gameObject);

            tr.GetComponentInChildren<TextMeshProUGUI>().text = GetSubcategoryDisplayName(subCategory);

            var tgl = tr.GetComponent<Toggle>();
            tgl.group = mainCategory.transform.GetComponent<ToggleGroup>();

            var cgroup = trTop.GetComponent<CanvasGroup>();
            mainCategory.items = mainCategory.items.AddToArray(new UI_ToggleGroupCtrl.ItemInfo { tglItem = tgl, cgItem = cgroup });

            foreach (var renderer in _subCategoryCopy.GetComponentsInChildren<UI_RaycastCtrl>())
                renderer.Reset();
            
            MakerAPI.Instance.StartCoroutine(FinishInit(trTop));

            tr.gameObject.SetActive(true);

            return tr;
        }
        
        private static IEnumerator FinishInit(Transform trTop)
        {
            yield return null;
            var scrl = Object.Instantiate(_scrollbarCopy, trTop, true);
            scrl.name = "Scroll View";
            var rt = scrl.GetComponent<RectTransform>();
            rt.offsetMax = new Vector2(472, 0);
            rt.offsetMin = new Vector2(0, -952);
            scrl.Find("Viewport/Content").GetComponent<Image>().raycastTarget = true;
            scrl.Find("Scrollbar Vertical").GetComponent<Image>().raycastTarget = true;
            scrl.gameObject.SetActive(true);

            trTop.GetComponentInChildren<UI_RaycastCtrl>(true)?.Reset();
        }
    }
}