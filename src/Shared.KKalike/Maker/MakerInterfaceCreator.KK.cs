using ChaCustom;
using IllusionUtility.GetUtility;
using KKAPI.Maker.UI;
using KKAPI.Maker.UI.Sidebar;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UniRx;
using UniRx.Triggers;
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
            var cat = MakerConstants.Face.All;

            AddControl(new MakerSeparator(cat, instance));
            AddControl(new MakerSeparator(cat, instance));
            AddControl(new MakerSeparator(cat, instance));
            AddControl(new MakerToggle(cat, "test toggle", instance))
                .ValueChanged.Subscribe(b => KoikatuAPI.Logger.LogMessage(b));
            AddControl(new MakerButton("test btn", cat, instance))
                .OnClick.AddListener(() => KoikatuAPI.Logger.LogMessage("Clicked"));
            AddControl(new MakerColor("test color", true, cat, Color.magenta, instance))
                .ValueChanged.Subscribe(color => KoikatuAPI.Logger.LogMessage(color));
            AddControl(new MakerDropdown("test toggle", new[] { "t0", "t1", "t2", "t3" }, cat, 1, instance))
                .ValueChanged.Subscribe(b => KoikatuAPI.Logger.LogMessage(b));
            AddControl(new MakerRadioButtons(cat, instance, "radio btns", "b1", "b2"))
                .ValueChanged.Subscribe(b => KoikatuAPI.Logger.LogMessage(b));
            AddControl(new MakerRadioButtons(cat, instance, "radio btns 1 row", "b1", "b2", "b3", "b4", "b5", "b6", "b7", "b8"))
                .ValueChanged.Subscribe(b => KoikatuAPI.Logger.LogMessage(b));
            AddControl(new MakerRadioButtons(cat, instance, "radio btns 5 per row", "b1", "b2", "b3", "b4", "b5", "b6", "b7", "b8") { ColumnCount = 5 })
                .ValueChanged.Subscribe(b => KoikatuAPI.Logger.LogMessage(b));
            AddControl(new MakerSlider(cat, "test slider", 0, 1, 1, instance))
                .ValueChanged.Subscribe(b => KoikatuAPI.Logger.LogMessage(b));
            AddControl(
                new MakerText(
                    "test text test text test text test text test text test " +
                    "text test text test text test text test text", cat, instance));
            AddControl(new MakerTextbox(cat, "test textbox", "String test", instance))
                .ValueChanged.Subscribe(b => KoikatuAPI.Logger.LogMessage(b));

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

            AddAccessoryWindowControl(new MakerToggle(cat, "test toggle", null))
                .ValueChanged.Subscribe(b => KoikatuAPI.Logger.LogMessage($"Toggled to {b} in accessory slot index {AccessoriesApi.SelectedMakerAccSlot}"));
            AddAccessoryWindowControl(new MakerColor("test accessory color", false, cat, Color.cyan, instance) { ColorBoxWidth = 230 })
                .ValueChanged.Subscribe(b => KoikatuAPI.Logger.LogMessage($"Color to {b} in accessory slot index {AccessoriesApi.SelectedMakerAccSlot}"));
        }

        public static void CreateCustomControls()
        {
            // Craete controls in tabs
            foreach (Transform categoryTransfrom in GameObject.Find("CvsMenuTree").transform)
                CreateCustomControlsInCategory(categoryTransfrom);

            var sidebarTop = GameObject.Find("CustomScene/CustomRoot/FrontUIGroup/CvsDraw/Top").transform;

            // Create sidebar controls
            if (_sidebarEntries.Any())
            {
                var sep = new SidebarSeparator(KoikatuAPI.Instance);
                sep.CreateControl(sidebarTop);

                foreach (var sidebarEntry in _sidebarEntries)
                    sidebarEntry.CreateControl(sidebarTop);

                KoikatuAPI.Logger.LogDebug(
                    $"Added {_sidebarEntries.Count} custom controls " +
                    "to Control Panel sidebar");
            }

            if (_accessoryWindowEntries.Any())
                CreateCustomAccessoryWindowControls();

            MakerLoadToggle.CreateCustomToggles();
            MakerCoordinateLoadToggle.CreateCustomToggles();

#if !KKS
            // KKS sidebar is already scrollable
            MakeSidebarScrollable(sidebarTop);
#endif
        }

        private static void MakeSidebarScrollable(Transform sidebarTop)
        {
            // Disable mousewheel scrolling for sliders in the sidebar
            sidebarTop.Find("sldEyeOpen/Slider").GetComponent<ObservableScrollTrigger>().enabled = false;
            sidebarTop.Find("sldMouthOpen/Slider").GetComponent<ObservableScrollTrigger>().enabled = false;
            sidebarTop.Find("sldLightingX/Slider").GetComponent<ObservableScrollTrigger>().enabled = false;
            sidebarTop.Find("sldLightingY/Slider").GetComponent<ObservableScrollTrigger>().enabled = false;

            var elements = new List<Transform>();
            foreach (Transform t in sidebarTop)
                elements.Add(t);

            var go = DefaultControls.CreateScrollView(new DefaultControls.Resources());
            go.name = "SidebarScrollView";
            go.transform.SetParent(sidebarTop.transform, false);

            var scroll = go.GetComponent<ScrollRect>();
            scroll.horizontal = false;
            scroll.scrollSensitivity = 40f;
            scroll.movementType = ScrollRect.MovementType.Clamped;
            scroll.horizontalScrollbarVisibility = ScrollRect.ScrollbarVisibility.AutoHide;
            scroll.verticalScrollbarVisibility = ScrollRect.ScrollbarVisibility.AutoHide;
            GameObject.DestroyImmediate(scroll.GetComponent<Image>());
            GameObject.DestroyImmediate(scroll.horizontalScrollbar.gameObject);
            GameObject.DestroyImmediate(scroll.verticalScrollbar.gameObject);

            scroll.gameObject.AddComponent<LayoutElement>();
            sidebarTop.GetComponent<VerticalLayoutGroup>().childForceExpandHeight = true;

            var vlg = scroll.content.gameObject.AddComponent<VerticalLayoutGroup>();
            vlg.childControlWidth = true;
            vlg.childControlHeight = true;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;

            scroll.content.gameObject.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            foreach (var item in elements)
                item.SetParent(scroll.content);
        }

        private static void CreateCustomControlsInCategory(Transform categoryTransfrom)
        {
            foreach (var subCategoryGroup in _guiEntries
                .Where(x => x.Category.CategoryName == categoryTransfrom.name)
                .GroupBy(x => x.Category.SubCategoryName))
            {
                var categorySubTransform = categoryTransfrom.Find(subCategoryGroup.Key);

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

            var needsSeparator = contentParent.childCount > 1;
            foreach (var gr in entriesToAdd.GroupBy(x => x.GroupingID).OrderBy(x => x.Key))
            {
                if (needsSeparator)
                    new MakerSeparator(new MakerCategory(null, null), KoikatuAPI.Instance).CreateControl(contentParent);

                foreach (var control in gr)
                    control.CreateControl(contentParent);

                needsSeparator = true;
            }

            var category = entriesToAdd.First().Category;
            KoikatuAPI.Logger.LogDebug(
                $"Added {entriesToAdd.Count} custom controls " +
                $"to {category.CategoryName}/{category.SubCategoryName}");
        }

        private static void CreateCustomAccessoryWindowControls()
        {
            var customAcsChangeSlot = Object.FindObjectOfType<CustomAcsChangeSlot>();
            if (customAcsChangeSlot == null) throw new ArgumentNullException(nameof(customAcsChangeSlot));
            var tglSlot01GameObject = customAcsChangeSlot.transform.FindLoop("tglSlot01");
            if (tglSlot01GameObject == null) throw new ArgumentNullException(nameof(tglSlot01GameObject));
            var container = tglSlot01GameObject.transform.parent;
#if KKS
            //can possibly work with KK/KKP 
            //Set source early rather than search every time
            var original_scroll = GameObject.Find("CustomScene/CustomRoot/FrontUIGroup/CustomUIGroup/CvsMenuTree/03_ClothesTop/tglTop/TopTop/Scroll View").transform;
#endif
            foreach (var slotTransform in container.Cast<Transform>().Where(x => x.name.StartsWith("tglSlot")).OrderBy(x => x.name))
            {
                // Remove the red info text at the bottom to free up some space
                var contentParent = FindSubcategoryContentParent(slotTransform);
                foreach (var txtName in new[] { "txtExplanation", "txtAcsExplanation" }) // Named differently in KK and EC
                {
                    var text = contentParent.Find(txtName);
                    if (text != null)
                    {
                        text.GetComponent<LayoutElement>().enabled = false;
                        text.Cast<Transform>().First().gameObject.SetActive(false);
                    }
                }
                CreateCustomControlsInSubCategory(slotTransform, _accessoryWindowEntries);
#if KK || KKS
                /*todo KKS version
                 copy existing scroll view and use that instead of making a new one
                CustomScene/CustomRoot/FrontUIGroup/CustomUIGroup/CvsMenuTree/03_ClothesTop/tglGloves/GlovesTop/Scroll View/Viewport/Content 
                unparent everything under content, copy it into a work copy, reparent
                copy to acc slots, reparent all the controls, destroy old layout elements and image
                */
                var listParent = slotTransform.Cast<Transform>().Where(x => x.name.EndsWith("Top")).First();

                var elements = new List<Transform>();
                foreach (Transform t in listParent)
                    elements.Add(t);

                var fitter = listParent.GetComponent<ContentSizeFitter>();
                fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
                fitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
#if KK 
                //Possibly removeable with KKS version
                var go = DefaultControls.CreateScrollView(new DefaultControls.Resources());
                go.name = $"{slotTransform.name}ScrollView";
                go.transform.SetParent(listParent.transform, false);

                var scroll = go.GetComponent<ScrollRect>();
                scroll.horizontal = false;
                scroll.scrollSensitivity = 40f;
                scroll.movementType = ScrollRect.MovementType.Clamped;
                scroll.horizontalScrollbarVisibility = ScrollRect.ScrollbarVisibility.AutoHideAndExpandViewport;
                scroll.verticalScrollbarVisibility = ScrollRect.ScrollbarVisibility.AutoHideAndExpandViewport;
                GameObject.DestroyImmediate(scroll.GetComponent<Image>());
                GameObject.DestroyImmediate(scroll.horizontalScrollbar.gameObject);
                GameObject.DestroyImmediate(scroll.verticalScrollbar.gameObject);

                var le = scroll.gameObject.AddComponent<LayoutElement>();
                var height = (GameObject.Find("CustomScene/CustomRoot/FrontUIGroup/CustomUIGroup/CvsMenuTree/04_AccessoryTop/Slots").transform as RectTransform).rect.height;
                le.preferredHeight = height;
                le.preferredWidth = 360f;

                var vlg = scroll.content.gameObject.AddComponent<VerticalLayoutGroup>();
                vlg.childControlWidth = true;
                vlg.childControlHeight = true;
                vlg.childForceExpandWidth = true;
                vlg.childForceExpandHeight = false;

                scroll.content.gameObject.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
#elif KKS
                var scrollTransform = Object.Instantiate(original_scroll, listParent.transform, false);
                scrollTransform.name = $"{slotTransform.name}ScrollView";
                var scroll = scrollTransform.GetComponent<ScrollRect>();
                scroll.verticalScrollbarSpacing = 15; //optional

                var content = scroll.content.transform;

                GameObject.DestroyImmediate(content.GetComponent<Image>());//Destroy smaller image that doesn't contain scroll
                GameObject.DestroyImmediate(scroll.GetComponent<Image>());//standard unity image that devs didn't remove themselves

                for (int i = 0; i < content.childCount; i++)//Remove original gameobjects
                {
                    Object.Destroy(content.GetChild(i).gameObject);
                }

                var s_LE = scroll.gameObject.AddComponent<LayoutElement>();
                var V_LE = scroll.viewport.gameObject.AddComponent<LayoutElement>();
                s_LE.preferredHeight = V_LE.preferredHeight = 850; //Slots from KK doesn't exist
                s_LE.preferredWidth = V_LE.preferredWidth = 380;

                //VerticalLayoutGroup already exists
#endif
                foreach (var item in elements)
                    item.SetParent(scroll.content);
                slotTransform.SetParent(scroll.content);
#endif
            }
        }

        internal static void OnMakerAccSlotAdded(Transform newSlotTransform)
        {
            // Necessary because MoreAccessories copies existing slot, so controls get copied but with no events hooked up
#if KK || KKS
            //KeelsChildNeglect(newSlotTransform, 0); //used to print children paths in case it's needed in the future, like horizontal support or something
            if (!MakerAPI.InsideAndLoaded)
            {
                return;//Maker slots are added before CreateCustomAccessoryWindowControls is called. don't create controls yet
            }
            //find parent of Content where controls are placed, additional Slots are copies of first slot as such they are currently named downstream Slot01
            newSlotTransform = newSlotTransform.Find("Slot01Top/tglSlot01ScrollView/Viewport");
#endif
            RemoveCustomControlsInSubCategory(newSlotTransform);

            CreateCustomControlsInSubCategory(newSlotTransform, _accessoryWindowEntries);
        }

        private static void RemoveCustomControlsInSubCategory(Transform newSlotTransform)
        {
            var contentParent = FindSubcategoryContentParent(newSlotTransform);
            //string children = "";
            //for (int i = 0, n = contentParent.childCount; i < n; i++)
            //{
            //    children += contentParent.GetChild(i).name;
            //    if (i == n - 2)
            //    {
            //        children += " and ";
            //    }
            //    else if (i == n - 1)
            //    {
            //        children += ".";
            //    }
            //    else
            //    {
            //        children += ", ";
            //    }
            //}
            //KoikatuAPI.Logger.LogWarning($"Content Parent is {contentParent.name}, has {contentParent.childCount} children, and child of {contentParent.parent} and is the parent of " + children);
            foreach (var customControl in contentParent.Cast<Transform>().Where(x => x.name.EndsWith(BaseGuiEntry.GuiApiNameAppendix)))
            {
                Object.Destroy(customControl.gameObject);
            }
        }

        internal static Transform FindSubcategoryContentParent(Transform categorySubTransform)
        {
            var content = categorySubTransform.Find("Scroll View/Viewport/Content");
            if (content != null) return content;

            var imgoff = categorySubTransform.Find("imgOff");
            if (imgoff == null) return categorySubTransform;

            var top = categorySubTransform.Cast<Transform>().First(x => x != imgoff);
            return top.Find("Scroll View/Viewport/Content") ?? top;
        }

        /// <summary>
        /// Needs to run before UI_ToggleGroupCtrl.Start of the category runs, or it won't get added properly
        /// </summary>
        internal static void AddMissingSubCategories(UI_ToggleGroupCtrl mainCategory)
        {
            var categoryTransfrom = mainCategory.transform;

            // Can break stuff, 06_SystemTop might be fine but needs testing
            if (categoryTransfrom.name == "04_AccessoryTop" || categoryTransfrom.name == "06_SystemTop") return;

            // Sorting breaks hair selector layout, too lazy to fix
            var noSorting = categoryTransfrom.name == "02_HairTop";

            var transformsToSort = new List<KeyValuePair<Transform, int>>();
            foreach (var category in _categories)
            {
                if (categoryTransfrom.name != category.CategoryName) continue;

                var categorySubTransform = categoryTransfrom.Find(category.SubCategoryName)
                                           ?? SubCategoryCreator.AddNewSubCategory(mainCategory, category);

                transformsToSort.Add(new KeyValuePair<Transform, int>(categorySubTransform, category.Position));
            }

            if (noSorting) return;

            foreach (Transform subTransform in categoryTransfrom)
            {
                if (transformsToSort.Any(x => x.Key == subTransform)) continue;

                var builtInCategory = MakerConstants.GetBuiltInCategory(categoryTransfrom.name, subTransform.name);
                if (builtInCategory != null)
                    transformsToSort.Add(new KeyValuePair<Transform, int>(subTransform, builtInCategory.Position));
                else
                    KoikatuAPI.Logger.LogWarning($"Missing MakerCategory for existing transfrom {categoryTransfrom.name} / {subTransform.name}");
            }

            var index = 0;
            foreach (var tuple in transformsToSort.OrderBy(x => x.Value))
                tuple.Key.SetSiblingIndex(index++);

            KoikatuAPI.Instance.StartCoroutine(FixCategoryContentOffsets(mainCategory));
        }

        private static IEnumerator FixCategoryContentOffsets(UI_ToggleGroupCtrl mainCategory)
        {
            yield return null;

            const int padding = 40;
            var index = 0;
            foreach (Transform tab in mainCategory.transform)
            {
                if (!tab.gameObject.activeSelf) continue;

                var contents = tab.Cast<Transform>().First(x => !x.name.Equals("imgOff"));
                var p = contents.localPosition;
                p.y = padding * index;
                contents.localPosition = p;
                index++;
            }
        }

        public static T AddSidebarControl<T>(T control) where T : BaseGuiEntry
        {
            _sidebarEntries.Add(control);
            return control;
        }

        public static CharacterLoadFlags GetCharacterLoadFlags()
        {
            var cfw = Object.FindObjectsOfType<CustomFileWindow>()
                .FirstOrDefault(i => i.fwType == CustomFileWindow.FileWindowType.CharaLoad);

            if (cfw == null) return null;

            return new CharacterLoadFlags
            {
                Body = cfw.tglChaLoadBody.isOn,
                Clothes = cfw.tglChaLoadCoorde.isOn,
                Hair = cfw.tglChaLoadHair.isOn,
                Face = cfw.tglChaLoadFace.isOn,
                Parameters = cfw.tglChaLoadParam.isOn
            };
        }

        public static CoordinateLoadFlags GetCoordinateLoadFlags()
        {
            var clothes = GameObject.Find("CustomScene/CustomRoot/FrontUIGroup/CustomUIGroup/CvsMenuTree/06_SystemTop/cosFileControl/charaFileWindow/WinRect/CoordinateLoad/Select/tglItem01")?.GetComponentInChildren<Toggle>(true);
            if (clothes == null) return null;

            var accs = GameObject.Find("CustomScene/CustomRoot/FrontUIGroup/CustomUIGroup/CvsMenuTree/06_SystemTop/cosFileControl/charaFileWindow/WinRect/CoordinateLoad/Select/tglItem02")?.GetComponentInChildren<Toggle>(true);
            if (accs == null) return null;
            return new CoordinateLoadFlags { Clothes = clothes.isOn, Accessories = accs.isOn };
        }

        //private static void KeelsChildNeglect(Transform parent, int generation)
        //{
        //    generation++;
        //    string tabs = "";
        //    for (int i = 0; i < generation; i++)
        //    {
        //        tabs += "\t";
        //    }
        //    for (int i = 0, n = parent.childCount; i < n; i++)
        //    {
        //        var child = parent.GetChild(i);
        //        KoikatuAPI.Logger.LogWarning(tabs + $"{child.name} (child {i} of {generation} generation) has {child.childCount} children; Parent is {child.parent}");
        //        KeelsChildNeglect(child, generation);
        //    }
        //}
    }
}
