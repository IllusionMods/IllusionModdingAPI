using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using BepInEx;
using CharaCustom;
using HarmonyLib;
using Illusion.Extensions;
using KKAPI.Maker.UI;
using KKAPI.Maker.UI.Sidebar;
using KKAPI.Utilities;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace KKAPI.Maker
{
    internal static class MakerInterfaceCreator
    {
        private static readonly List<MakerCategory> _categories = new List<MakerCategory>();
        private static readonly List<BaseGuiEntry> _guiEntries = new List<BaseGuiEntry>();
        //private static readonly List<BaseGuiEntry> _sidebarEntries = new List<BaseGuiEntry>();
        private static readonly List<BaseGuiEntry> _accessoryWindowEntries = new List<BaseGuiEntry>();

        public static void RemoveCustomControls()
        {
            foreach (var guiEntry in _guiEntries)//.Concat(_sidebarEntries).Concat(_accessoryWindowEntries))
                guiEntry.Dispose();

            _guiEntries.Clear();
            _categories.Clear();
            //_sidebarEntries.Clear();
            _accessoryWindowEntries.Clear();

            MakerLoadToggle.Reset();
            MakerCoordinateLoadToggle.Reset();
        }

        public static T AddControl<T>(T control) where T : BaseGuiEntry
        {
            if (control is MakerLoadToggle || control is MakerCoordinateLoadToggle || control is ISidebarControl)
                throw new ArgumentException("Can't add " + control.GetType().FullName + " as a normal control", nameof(control));

            _guiEntries.Add(control);
            return control;
        }

        public static void AddSubCategory(MakerCategory category)
        {
            if (!_categories.Contains(category))
                _categories.Add(category);
            else
                KoikatuAPI.Logger.LogInfo($"Duplicate custom subcategory was added: {category} The duplicate will be ignored.");
        }

        public static void AddAccessoryWindowControl<T>(T control) where T : BaseGuiEntry
        {
            control.Category = new MakerCategory("Accessory", "");
            _accessoryWindowEntries.Add(control);
        }

        public static void InitializeGuiEntries()
        {
            foreach (var baseGuiEntry in _guiEntries)
                baseGuiEntry.Initialize();
        }

        [Conditional("DEBUG")]
        private static void DebugControls()
        {
            var instance = KoikatuAPI.Instance;

            var cat = MakerConstants.Face.All;

            AddControl(new MakerSeparator(cat, instance));
            AddControl(new MakerSeparator(cat, instance));
            AddControl(new MakerSeparator(cat, instance));
            //AddControl(new MakerToggle(cat, "test toggle", instance))
            //    .ValueChanged.Subscribe(b => KoikatuAPI.Logger.LogMessage(b));
            AddControl(new MakerButton("test btn", cat, instance))
                .OnClick.AddListener(() => KoikatuAPI.Logger.LogMessage("Clicked"));
            //AddControl(new MakerColor("test color", true, cat, Color.magenta, instance))
            //    .ValueChanged.Subscribe(color => KoikatuAPI.Logger.LogMessage(color));
            //AddControl(new MakerDropdown("test toggle", new[] { "t0", "t1", "t2", "t3" }, cat, 1, instance))
            //    .ValueChanged.Subscribe(b => KoikatuAPI.Logger.LogMessage(b));
            AddControl(new MakerRadioButtons(cat, instance, "radio btns", "b1", "b2", "b3"))
                .ValueChanged.Subscribe(b => KoikatuAPI.Logger.LogMessage(b));
            AddControl(new MakerSlider(cat, "test slider", 0, 1, 1, instance))
                .ValueChanged.Subscribe(b => KoikatuAPI.Logger.LogMessage(b));
            //AddControl(
            //    new MakerText(
            //        "test text test text test text test text test text test " +
            //        "text test text test text test text test text", cat, instance));
            //AddControl(new MakerTextbox(cat, "test textbox", "String test", instance))
            //    .ValueChanged.Subscribe(b => KoikatuAPI.Logger.LogMessage(b));
            //
            //MakerLoadToggle.AddLoadToggle(new MakerLoadToggle("Test toggle"))
            //    .ValueChanged.Subscribe(b => KoikatuAPI.Logger.LogMessage(b));
            //MakerLoadToggle.AddLoadToggle(new MakerLoadToggle("Test toggle 2"))
            //    .ValueChanged.Subscribe(b => KoikatuAPI.Logger.LogMessage(b));
            //
            //MakerCoordinateLoadToggle.AddLoadToggle(new MakerCoordinateLoadToggle("Test toggle"))
            //    .ValueChanged.Subscribe(b => KoikatuAPI.Logger.LogMessage(b));
            //MakerCoordinateLoadToggle.AddLoadToggle(new MakerCoordinateLoadToggle("Test toggle 2"))
            //    .ValueChanged.Subscribe(b => KoikatuAPI.Logger.LogMessage(b));
            //
            //AddSidebarControl(new SidebarToggle("Test toggle", false, instance))
            //    .ValueChanged.Subscribe(b => KoikatuAPI.Logger.LogMessage(b));
            //AddSidebarControl(new SidebarToggle("Test toggle2", true, instance))
            //    .ValueChanged.Subscribe(b => KoikatuAPI.Logger.LogMessage(b));
            //
            //AddAccessoryWindowControl(new MakerToggle(cat, "test toggle", null))
            //    .ValueChanged.Subscribe(b => KoikatuAPI.Logger.LogMessage($"Toggled to {b} in accessory slot index {AccessoriesApi.SelectedMakerAccSlot}"));
            //AddAccessoryWindowControl(new MakerColor("test accessory color", false, cat, Color.cyan, instance) { ColorBoxWidth = 230 })
            //    .ValueChanged.Subscribe(b => KoikatuAPI.Logger.LogMessage($"Color to {b} in accessory slot index {AccessoriesApi.SelectedMakerAccSlot}"));

            var copyCat = new MakerCategory(cat.CategoryName, "Test", 0, "Test subcategory");
            _categories.Add(copyCat);
            AddControl(new MakerSlider(copyCat, "test slider copyCat", 0, 1, 1, instance))
                .ValueChanged.Subscribe(b => KoikatuAPI.Logger.LogMessage(b));
            AddControl(new MakerSeparator(copyCat, instance));

            for (int i = 0; i < 12; i++)
                AddControl(new MakerSlider(MakerConstants.Face.FaceType, "test slider", 0, 1, 1, instance));

            var acc = new MakerSlider(MakerConstants.Face.FaceType, "test acc slider", 0, 1, 1, instance);
            AddAccessoryWindowControl(acc);
            var wrapped = new AccessoryControlWrapper<MakerSlider, float>(acc);
            wrapped.VisibleIndexChanged += (sender, args) => KoikatuAPI.Logger.LogMessage("VisibleIndexChanged");
            wrapped.AccessoryKindChanged += (sender, args) => KoikatuAPI.Logger.LogMessage("AccessoryKindChanged");
            wrapped.ValueChanged += (sender, args) => KoikatuAPI.Logger.LogMessage($"ValueChanged {wrapped.GetSelectedValue()} for acc {wrapped.CurrentlySelectedIndex}");
        }

        public static void CreateCustomControls()
        {
            // Craete controls in tabs
            foreach (var window in Object.FindObjectsOfType<CvsSelectWindow>())
                CreateCustomControlsInCategory(window);

            if (_accessoryWindowEntries.Any())
                CreateCustomAccessoryWindowControls();

            /*// Create sidebar controls
            if (_sidebarEntries.Any())
            {
                var sidebarTop = GameObject.Find("CustomScene/CustomRoot/FrontUIGroup/CvsDraw/Top").transform;

                var sep = new SidebarSeparator(KoikatuAPI.Instance);
                sep.CreateControl(sidebarTop);

                foreach (var sidebarEntry in _sidebarEntries)
                    sidebarEntry.CreateControl(sidebarTop);

                KoikatuAPI.Logger.LogDebug(
                    $"Added {_sidebarEntries.Count} custom controls " +
                    "to Control Panel sidebar");
            }*/

            MakerLoadToggle.CreateCustomToggles();
            MakerCoordinateLoadToggle.CreateCustomToggles();
        }

        private static void CreateCustomAccessoryWindowControls()
        {
            var top = GameObject.Find("CharaCustom/CustomControl/CanvasMain/SettingWindow/WinAccessory/A_Slot");
            var content = CreateModsCvsWindowCategory(top.GetComponent<CvsBase>());
            CreateCustomControlsInSubCategory(content, _accessoryWindowEntries);
        }

        private static void CreateCustomControlsInCategory(CvsSelectWindow category)
        {
            var categoryName = GetCategoryName(category);
            foreach (var subCategoryGroup in _guiEntries
                .Where(x => x.Category.CategoryName == categoryName)
                .GroupBy(x => x.Category.SubCategoryName))
            {
                var categorySubTransform = category.items.FirstOrDefault(x => x.btnItem.name == subCategoryGroup.Key);

                if (categorySubTransform != null)
                {
                    CreateCustomControlsInSubCategory(categorySubTransform.cgItem.First().transform, subCategoryGroup.ToList());
                }
                else
                {
                    KoikatuAPI.Logger.LogError(
                        $"Failed to add {subCategoryGroup.Count()} custom controls " +
                        $"to {categoryName}/{subCategoryGroup.Key} - The category was not registered with AddSubCategory.");
                }
            }
        }

        private static void CreateCustomControlsInSubCategory(Transform subCategoryTransform, ICollection<BaseGuiEntry> entriesToAdd)
        {
            if (entriesToAdd.Count == 0) return;

            Transform contentParent = null;
            if (subCategoryTransform.name != "Content")
            {
                // Handle some windows having groups instead of normal scroll view
                contentParent = subCategoryTransform.Find("Scroll View/Viewport/Content") ?? subCategoryTransform.Find("contents/Scroll View/Viewport/Content");
                if (contentParent == null)
                {
                    var cvsBase = subCategoryTransform.GetComponent<CvsBase>();

                    if (cvsBase != null)
                        contentParent = CreateModsCvsWindowCategory(cvsBase);
                }
            }
            if (contentParent == null)
                contentParent = subCategoryTransform;

            BaseUnityPlugin lastOwner = contentParent.childCount > 1 ? KoikatuAPI.Instance : null;
            foreach (var customControl in entriesToAdd)
            {
                if (lastOwner != customControl.Owner && lastOwner != null)
                    new MakerSeparator(new MakerCategory(null, null), KoikatuAPI.Instance).CreateControl(contentParent);

                customControl.CreateControl(contentParent);
                lastOwner = customControl.Owner;
            }

            var category = entriesToAdd.First().Category;
            KoikatuAPI.Logger.LogDebug(
                $"Added {entriesToAdd.Count} custom controls " +
                $"to {category.CategoryName}/{category.SubCategoryName}");
        }

        private static Transform CreateModsCvsWindowCategory(CvsBase cvsBase)
        {
            var baseTransform = cvsBase.transform.Find("contents") ?? cvsBase.transform;
            var select = baseTransform.Find("SelectMenu");
            if (select.Find("tgl05") != null)
            {
                // If no more can be added (max 5), use the last existing one and pray
                var existingSetting = baseTransform.Find("Setting/Setting05") ?? throw new ArgumentException("asd");
                return FindSubcategoryContentParent(existingSetting);
            }

            // If there is space for a new toggle, make a mods toggle
            var newIndex = cvsBase.items.Length;

            var newTgl = Object.Instantiate(GameObject.Find("CharaCustom/CustomControl/CanvasMain/SettingWindow/WinBody/B_Nip/SelectMenu/tgl02"), select);
            // Name format is important
            newTgl.name = "tgl0" + (newIndex + 1);
            newTgl.GetComponentInChildren<Text>().text = "Mods";
            var toggleEx = newTgl.GetComponent<UI_ToggleEx>();
            toggleEx.onValueChanged.ActuallyRemoveAllListeners();
            toggleEx.onValueChanged.AddListener(
                newVal =>
                {
                    toggleEx.isOn = newVal;
                    // Only trigger on a toggle being enabled. Based on CvsBase.Start
                    if (!newVal) return;
                    for (int i = 0; i < cvsBase.items.Length; i++)
                    {
                        if (cvsBase.items[i] != null)
                        {
                            var cgItem = cvsBase.items[i].cgItem;
                            if (cgItem)
                                cgItem.Enable(i == newIndex);
                        }
                    }
                    Singleton<CustomBase>.Instance.customCtrl.showColorCvs = false;
                    Singleton<CustomBase>.Instance.customCtrl.showPattern = false;
                });
            toggleEx.group = select.GetComponent<ToggleGroup>();

            var settingParent = baseTransform.Find("Setting");
            var newSetting = Object.Instantiate(GameObject.Find("CharaCustom/CustomControl/CanvasMain/SettingWindow/WinBody/B_Nip/Setting/Setting02"), settingParent);
            newSetting.name = "Setting0" + (newIndex + 1);

            cvsBase.ReacquireTab();
            var canvasGroup = newSetting.GetComponent<CanvasGroup>();
            canvasGroup.Enable(false);
            var newItemInfo = new CvsBase.ItemInfo { cgItem = canvasGroup, tglItem = newTgl.GetComponent<UI_ToggleEx>() };
            cvsBase.items = cvsBase.items.AddItem(newItemInfo).ToArray();

            var newContents = FindSubcategoryContentParent(newSetting.transform);
            foreach (var child in newContents.Children())
                Object.DestroyImmediate(child.gameObject);

            LayoutRebuilder.MarkLayoutForRebuild(select.GetComponent<RectTransform>());

            return newContents;
        }

        internal static Transform FindSubcategoryContentParent(Transform categorySubTransform)
        {
            var top = categorySubTransform;
            return top.Find("Scroll View/Viewport/Content") ?? top.Find("contents/Scroll View/Viewport/Content") ?? top;
        }

        /// <summary>
        /// Needs to run before UI_ToggleGroupCtrl.Start of the category runs, or it won't get added properly
        /// </summary>
        public static void AddMissingSubCategories(CvsSelectWindow mainCategory)
        {
            string categoryName = GetCategoryName(mainCategory);

            var allSubcategories = mainCategory.transform.GetComponentsInChildren<Transform>().Where(x => x.name == "CategoryTop").SelectMany(x => x.Cast<Transform>()).ToList();

            Transform modsCategoryTop = null;

            var addedSubCategories = new List<CvsSelectWindow.ItemInfo>();
            var window = mainCategory.cgBaseWindow.transform.Find("Win" + categoryName);
            foreach (var category in _categories)
            {
                if (categoryName != category.CategoryName) continue;

                if (allSubcategories.All(transform => transform.name != category.SubCategoryName))
                {
                    if (modsCategoryTop == null)
                    {
                        KoikatuAPI.Logger.LogDebug($"Creating subcategory Mods group in {categoryName}");

                        var sourceCategory = GameObject.Find("rbSType01").transform.parent.parent.parent;

                        var target = FindSubcategoryContentParent(mainCategory.transform);

                        var copy = Object.Instantiate(sourceCategory, target);
                        copy.Find("CategoryTitle").GetComponentInChildren<Text>().text = "Mods";

                        modsCategoryTop = copy.Find("CategoryTop");

                        foreach (var tr in modsCategoryTop.Children())
                            Object.DestroyImmediate(tr.gameObject);
                    }

                    KoikatuAPI.Logger.LogDebug($"Adding custom subcategory {category.SubCategoryName} to {categoryName}");

                    addedSubCategories.Add(SubCategoryCreator.AddNewSubCategory(modsCategoryTop, category, window));
                }
            }

            mainCategory.items = mainCategory.items.Concat(addedSubCategories).ToArray();
        }

        private static string GetCategoryName(CvsSelectWindow mainCategory)
        {
            return mainCategory.transform.name.Substring("SubMenu".Length);
        }

        public static void AddSidebarControl(BaseGuiEntry control)
        {
            KoikatuAPI.Logger.LogWarning("AddSidebarControl is not supported\n" + new StackTrace());
            //_sidebarEntries.Add(control);
        }

        public static void InitializeMaker()
        {
            DebugControls();
        }
    }
}