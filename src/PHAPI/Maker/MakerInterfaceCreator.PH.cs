using BepInEx;
using HarmonyLib;
using KKAPI.Maker.UI;
using KKAPI.Maker.UI.Sidebar;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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
        private static readonly List<BaseGuiEntry> _sidebarEntries = new List<BaseGuiEntry>();
        private static readonly List<BaseGuiEntry> _accessoryWindowEntries = new List<BaseGuiEntry>();

        private static readonly MakerCategory _accessorySlotWindowCategory = new MakerCategory("04_AccessoryTop", "Slots");

        public static void RemoveCustomControls()
        {
            foreach (var guiEntry in _guiEntries.Concat(_sidebarEntries).Concat(_accessoryWindowEntries))
                guiEntry.Dispose();

            _guiEntries.Clear();
            _categories.Clear();
            _sidebarEntries.Clear();
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

        internal static void InitializeMaker()
        {
            MakerLoadToggle.Setup();
            MakerCoordinateLoadToggle.Setup();
        }

        public static T AddAccessoryWindowControl<T>(T control) where T : BaseGuiEntry
        {
            control.Category = _accessorySlotWindowCategory;
            _accessoryWindowEntries.Add(control);
            return control;
        }

        public static void InitializeGuiEntries()
        {
            DebugControls();

            foreach (var baseGuiEntry in _guiEntries)
                baseGuiEntry.Initialize();
        }

        [Conditional("DEBUG")]
        private static void DebugControls()
        {
            var instance = KoikatuAPI.Instance;
            var cat = MakerConstants.Face.Ear;

            AddControl(new MakerSeparator(cat, instance));
            AddControl(new MakerToggle(cat, "test toggle", instance))
                .ValueChanged.Subscribe(b => KoikatuAPI.Logger.LogMessage(b));
            AddControl(new MakerButton("test btn", cat, instance))
                .OnClick.AddListener(() => KoikatuAPI.Logger.LogMessage("Clicked"));
            AddControl(new MakerColor("test color", true, cat, Color.magenta, instance))
                .ValueChanged.Subscribe(color => KoikatuAPI.Logger.LogMessage(color));
            AddControl(new MakerDropdown("test toggle", new[] { "t0", "t1", "t2", "t3" }, cat, 1, instance))
                .ValueChanged.Subscribe(b => KoikatuAPI.Logger.LogMessage(b));
            //AddControl(new MakerRadioButtons(cat, instance, "radio btns", "b1", "b2"))
            //    .ValueChanged.Subscribe(b => KoikatuAPI.Logger.LogMessage(b));
            AddControl(new MakerSlider(cat, "test slider", 0, 1, 1, instance))
                .ValueChanged.Subscribe(b => KoikatuAPI.Logger.LogMessage(b));
            AddControl(
                new MakerText(
                    "test text test text test text test text test text test " +
                    "text test text test text test text test text", cat, instance));
            //AddControl(new MakerTextbox(cat, "test textbox", "String test", instance))
            //    .ValueChanged.Subscribe(b => KoikatuAPI.Logger.LogMessage(b));

            MakerLoadToggle.AddLoadToggle(new MakerLoadToggle("Test toggle"))
                .ValueChanged.Subscribe(b => KoikatuAPI.Logger.LogMessage(b));
            MakerLoadToggle.AddLoadToggle(new MakerLoadToggle("Test toggle 2"))
                .ValueChanged.Subscribe(b => KoikatuAPI.Logger.LogMessage(b));

            MakerCoordinateLoadToggle.AddLoadToggle(new MakerCoordinateLoadToggle("Test toggle"))
                .ValueChanged.Subscribe(b => KoikatuAPI.Logger.LogMessage(b));
            MakerCoordinateLoadToggle.AddLoadToggle(new MakerCoordinateLoadToggle("Test toggle 2"))
                .ValueChanged.Subscribe(b => KoikatuAPI.Logger.LogMessage(b));

            AddSidebarControl(new SidebarToggle("Test toggle", false, instance))
                .ValueChanged.Subscribe(b => KoikatuAPI.Logger.LogMessage(b));
            AddSidebarControl(new SidebarToggle("Test toggle2", true, instance))
                .ValueChanged.Subscribe(b => KoikatuAPI.Logger.LogMessage(b));

            //AddAccessoryWindowControl(new MakerToggle(cat, "test toggle", null))
            //    .ValueChanged.Subscribe(b => KoikatuAPI.Logger.LogMessage($"Toggled to {b} in accessory slot index {AccessoriesApi.SelectedMakerAccSlot}"));
            //AddAccessoryWindowControl(new MakerColor("test accessory color", false, cat, Color.cyan, instance) { ColorBoxWidth = 230 })
            //    .ValueChanged.Subscribe(b => KoikatuAPI.Logger.LogMessage($"Color to {b} in accessory slot index {AccessoriesApi.SelectedMakerAccSlot}"));
        }

        public static void CreateCustomControls()
        {
            // Create controls in tabs
            var editMode = Object.FindObjectOfType<EditMode>();
            foreach (Transform categoryTransfrom in editMode.transform.Find("Canvas").transform)
                CreateCustomControlsInCategory(categoryTransfrom);

            var sidebarToggles = _sidebarEntries.OfType<SidebarToggle>().ToList();
            if (sidebarToggles.Any())
                CreateSidebarToggles(sidebarToggles);

            if (_accessoryWindowEntries.Any())
                CreateCustomAccessoryWindowControls();

            MakerLoadToggle.CreateCustomToggles();
            MakerCoordinateLoadToggle.CreateCustomToggles();
        }

        private static void CreateSidebarToggles(List<SidebarToggle> sidebarToggles)
        {
            var editScene = Object.FindObjectOfType<EditScene>();
            if (editScene == null)
            {
                KoikatuAPI.Logger.LogWarning("Not in an EditScene, can't spawn sidebar toggles");
                return;
            }

            var sidebarTop = editScene.transform.Find("Canvas/Show").transform;

            var copy = Object.Instantiate(sidebarTop, sidebarTop.parent, false);
            copy.name = "PHAPI sidebar";
            copy.localPosition = new Vector3(copy.localPosition.x, copy.localPosition.y + 215);

            var glg = copy.GetComponentInChildren<GridLayoutGroup>();
            glg.childAlignment = TextAnchor.LowerLeft;

            var i = 0;
            foreach (Transform child in glg.transform)
            {
                if (_sidebarEntries.Count > i)
                {
                    var toggle = sidebarToggles[i];
                    toggle.CreateControl(child);
                }
                else
                {
                    Object.Destroy(child.gameObject);
                }

                i++;
            }

            KoikatuAPI.Logger.LogDebug(
                $"Added {sidebarToggles.Count} custom controls " +
                "to Control Panel sidebar");
        }

        private static void CreateCustomControlsInCategory(Transform categoryTransfrom)
        {
            foreach (var subCategoryGroup in _guiEntries
                .Where(x => x.Category.CategoryName == categoryTransfrom.name)
                .GroupBy(x => x.Category.SubCategoryName))
            {
                var categorySubTransform = categoryTransfrom.Find("Mains")?.Find(subCategoryGroup.Key);
                if (categorySubTransform == null)
                    categorySubTransform = categoryTransfrom.Find("Main")?.Find(subCategoryGroup.Key); //For clothing categories

                if (categorySubTransform != null)
                {
                    CreateCustomControlsInSubCategory(categorySubTransform, subCategoryGroup.ToList());
                }
                else
                {
                    KoikatuAPI.Logger.LogError(
                        $"Failed to add {subCategoryGroup.Count()} custom controls " +
                        $"to {categoryTransfrom.name}/{subCategoryGroup.Key} - The category was not registered with AddSubCategory.");
                }
            }
        }

        private static void CreateCustomControlsInSubCategory(Transform subCategoryTransform, ICollection<BaseGuiEntry> entriesToAdd)
        {
            if (entriesToAdd.Count == 0) return;

            var contentParent = FindSubcategoryContentParent(subCategoryTransform);

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

        private static void CreateCustomAccessoryWindowControls()
        {
            //todo
            //var customAcsChangeSlot = Object.FindObjectOfType<CustomAcsChangeSlot>();
            //if (customAcsChangeSlot == null) throw new ArgumentNullException(nameof(customAcsChangeSlot));
            //var tglSlot01GameObject = customAcsChangeSlot.transform.FindLoop("tglSlot01");
            //if (tglSlot01GameObject == null) throw new ArgumentNullException(nameof(tglSlot01GameObject));
            //var container = tglSlot01GameObject.transform.parent;
            //foreach (var slotTransform in container.Cast<Transform>().Where(x => x.name.StartsWith("tglSlot")).OrderBy(x => x.name))
            //{
            //    // Remove the red info text at the bottom to free up some space
            //    var contentParent = FindSubcategoryContentParent(slotTransform);
            //    foreach (var txtName in new[] { "txtExplanation", "txtAcsExplanation" }) // Named differently in KK and EC
            //    {
            //        var text = contentParent.Find(txtName);
            //        if (text != null)
            //        {
            //            text.GetComponent<LayoutElement>().enabled = false;
            //            text.Cast<Transform>().First().gameObject.SetActive(false);
            //        }
            //    }
            //
            //    CreateCustomControlsInSubCategory(slotTransform, _accessoryWindowEntries);
            //}
        }

        internal static void OnMakerAccSlotAdded(Transform newSlotTransform)
        {
            // Necessary because MoraAccessories copies existing slot, so controls get copied but with no events hooked up
            RemoveCustomControlsInSubCategory(newSlotTransform);

            CreateCustomControlsInSubCategory(newSlotTransform, _accessoryWindowEntries);
        }

        private static void RemoveCustomControlsInSubCategory(Transform newSlotTransform)
        {
            var contentParent = FindSubcategoryContentParent(newSlotTransform);

            foreach (var customControl in contentParent.Cast<Transform>().Where(x => x.name.EndsWith(BaseGuiEntry.GuiApiNameAppendix)))
                Object.Destroy(customControl.gameObject);
        }

        internal static Transform FindSubcategoryContentParent(Transform categorySubTransform)
        {
            return categorySubTransform;
            //var top = categorySubTransform.Cast<Transform>().First(x => x.name != "imgOff");
            //return top.Find("Scroll View/Viewport/Content") ?? top;
        }

        /// <summary>
        /// Needs to run before UI_ToggleGroupCtrl.Start of the category runs, or it won't get added properly
        /// </summary>
        internal static void AddMissingSubCategories() //UI_ToggleGroupCtrl mainCategory
        {
            //todo
            //var categoryTransfrom = mainCategory.transform;
            //
            //// Can break stuff, 06_SystemTop might be fine but needs testing
            //if (categoryTransfrom.name == "04_AccessoryTop" || categoryTransfrom.name == "06_SystemTop") return;
            //
            //// Sorting breaks hair selector layout, too lazy to fix
            //var noSorting = categoryTransfrom.name == "02_HairTop";
            //
            //var transformsToSort = new List<KeyValuePair<Transform, int>>();
            //foreach (var category in _categories)
            //{
            //    if (categoryTransfrom.name != category.CategoryName) continue;
            //
            //    var categorySubTransform = categoryTransfrom.Find(category.SubCategoryName)
            //                               ?? SubCategoryCreator.AddNewSubCategory(mainCategory, category);
            //
            //    transformsToSort.Add(new KeyValuePair<Transform, int>(categorySubTransform, category.Position));
            //}
            //
            //if (noSorting) return;
            //
            //foreach (Transform subTransform in categoryTransfrom)
            //{
            //    if (transformsToSort.Any(x => x.Key == subTransform)) continue;
            //
            //    var builtInCategory = MakerConstants.GetBuiltInCategory(categoryTransfrom.name, subTransform.name);
            //    if (builtInCategory != null)
            //        transformsToSort.Add(new KeyValuePair<Transform, int>(subTransform, builtInCategory.Position));
            //    else
            //        KoikatuAPI.Logger.LogWarning($"Missing MakerCategory for existing transfrom {categoryTransfrom.name} / {subTransform.name}");
            //}
            //
            //var index = 0;
            //foreach (var tuple in transformsToSort.OrderBy(x => x.Value))
            //    tuple.Key.SetSiblingIndex(index++);
            //
            //KoikatuAPI.Instance.StartCoroutine(FixCategoryContentOffsets(mainCategory));
        }

        //private static IEnumerator FixCategoryContentOffsets(UI_ToggleGroupCtrl mainCategory)
        //{
        //    yield return null;
        //
        //    const int padding = 40;
        //    var index = 0;
        //    foreach (Transform tab in mainCategory.transform)
        //    {
        //        if (!tab.gameObject.activeSelf) continue;
        //
        //        var contents = tab.Cast<Transform>().First(x => !x.name.Equals("imgOff"));
        //        var p = contents.localPosition;
        //        p.y = padding * index;
        //        contents.localPosition = p;
        //        index++;
        //    }
        //}

        public static T AddSidebarControl<T>(T control) where T : BaseGuiEntry
        {
            _sidebarEntries.Add(control);
            return control;
        }

        public static CharacterLoadFlags GetCharacterLoadFlags()
        {
            var cfw = Object.FindObjectOfType<CharaLoadUI>();

            if (cfw == null) return null;

            var res = Traverse.Create(cfw).Method("CalcFilter").GetValue<int>();

            return new CharacterLoadFlags
            {
                Hair = (res & 1) != 0,
                Face = (res & 2) != 0,
                Body = (res & 4) != 0,
                Clothes = (res & 8) != 0,
                Accessories = (res & 16) != 0,
            };
        }

        public static CoordinateLoadFlags GetCoordinateLoadFlags()
        {
            var cfw = Object.FindObjectOfType<CoordinateLoadUI>();

            if (cfw == null) return null;

            var res = Traverse.Create(cfw).Method("CalcFilter").GetValue<int>();

            return new CoordinateLoadFlags
            {
                Clothes = (res & 8) != 0,
                Accessories = (res & 16) != 0,
            };
        }
    }
}