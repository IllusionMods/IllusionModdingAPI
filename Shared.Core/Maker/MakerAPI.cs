using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using BepInEx;
using KKAPI.Chara;
using KKAPI.Maker.UI;
using KKAPI.Maker.UI.Sidebar;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;
#if KK || EC
using ChaCustom;
using UniRx;
#elif AI
using CharaCustom;
using AIChara;
#endif

namespace KKAPI.Maker
{
    /// <summary>
    /// Provides a way to add custom items to the in-game Character Maker, and gives useful methods for interfacing with the maker.
    /// </summary>
    public static partial class MakerAPI
    {
        private static readonly List<MakerCategory> _categories = new List<MakerCategory>();
        private static readonly List<BaseGuiEntry> _guiEntries = new List<BaseGuiEntry>();
        private static readonly List<BaseGuiEntry> _sidebarEntries = new List<BaseGuiEntry>();
        private static readonly List<BaseGuiEntry> _accessoryWindowEntries = new List<BaseGuiEntry>();

        private static readonly MakerCategory _accessorySlotWindowCategory = new MakerCategory("04_AccessoryTop", "Slots");

        private static void CreateCustomControls()
        {
            // Craete controls in tabs
            foreach (Transform categoryTransfrom in GameObject.Find("CvsMenuTree").transform)
                CreateCustomControlsInCategory(categoryTransfrom);

            // Create sidebar controls
            if (_sidebarEntries.Any())
            {
                var sidebarTop = GameObject.Find("CustomScene/CustomRoot/FrontUIGroup/CvsDraw/Top").transform;

                var sep = new SidebarSeparator(KoikatuAPI.Instance);
                sep.CreateControl(sidebarTop);

                foreach (var sidebarEntry in _sidebarEntries)
                    sidebarEntry.CreateControl(sidebarTop);

                KoikatuAPI.Logger.LogDebug(
                    $"[MakerAPI] Added {_sidebarEntries.Count} custom controls " +
                    "to Control Panel sidebar");
            }

            if (_accessoryWindowEntries.Any())
                CreateCustomAccessoryWindowControls();
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
                        $"[MakerAPI] Failed to add {subCategoryGroup.Count()} custom controls " +
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
                $"[MakerAPI] Added {entriesToAdd.Count} custom controls " +
                $"to {category.CategoryName}/{category.SubCategoryName}");
        }

        private static void CreateCustomAccessoryWindowControls()
        {
            var container = GameObject.Find("tglSlot01").transform.parent;
            foreach (var slotTransform in container.Cast<Transform>().Where(x => x.name.StartsWith("tglSlot")).OrderBy(x => x.name))
            {
                // Remove the red info text at the bottom to free up some space
                var contentParent = FindSubcategoryContentParent(slotTransform);
                foreach (var txtName in new[] { "txtExplanation", "txtAcsExplanation" })
                {
                    var text = contentParent.Find(txtName);
                    if (text != null)
                    {
                        text.GetComponent<LayoutElement>().enabled = false;
                        text.Cast<Transform>().First().gameObject.SetActive(false);
                    }
                }

                CreateCustomControlsInSubCategory(slotTransform, _accessoryWindowEntries);
            }
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

        private static void RemoveCustomControls()
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

        internal static Transform FindSubcategoryContentParent(Transform categorySubTransform)
        {
            var top = categorySubTransform.Cast<Transform>().First(x => x.name != "imgOff");
            return top.Find("Scroll View/Viewport/Content") ?? top;
        }

        /// <summary>
        /// Needs to run before UI_ToggleGroupCtrl.Start of the category runs, or it won't get added properly
        /// </summary>
        private static void AddMissingSubCategories(UI_ToggleGroupCtrl mainCategory)
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
                    KoikatuAPI.Logger.LogWarning($"[MakerAPI] Missing MakerCategory for existing transfrom {categoryTransfrom.name} / {subTransform.name}");
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

        /// <summary>
        /// Add custom controls. If you want to use custom sub categories, register them by calling AddSubCategory.
        /// </summary>
        internal static T AddControl<T>(T control) where T : BaseGuiEntry
        {
            if (control == null) throw new ArgumentNullException(nameof(control));
            if (control.IsDisposed) throw new ObjectDisposedException(nameof(control), "A new control has to be created every time maker is started");
            if (control is MakerLoadToggle || control is MakerCoordinateLoadToggle || control is ISidebarControl)
                throw new ArgumentException("Can't add " + control.GetType().FullName + " as a normal control", nameof(control));

            _guiEntries.Add(control);
            return control;
        }

        /// <summary>
        /// Add custom sub categories. They need to be added before maker starts loading,
        /// or in the <see cref="RegisterCustomSubCategories"/> event.
        /// </summary>
        internal static void AddSubCategory(MakerCategory category)
        {
            if (!_categories.Contains(category))
                _categories.Add(category);
            else
                KoikatuAPI.Logger.LogInfo($"[MakerAPI] Duplicate custom subcategory was added: {category} The duplicate will be ignored.");
        }

        /// <summary>
        /// Add a control to the right sidebar in chara maker (the "Control Panel" where you set eye blinking, mouth expressions etc.)
        /// </summary>
        public static T AddSidebarControl<T>(T control) where T : BaseGuiEntry, ISidebarControl
        {
            if (control == null) throw new ArgumentNullException(nameof(control));
            if (control.IsDisposed) throw new ObjectDisposedException(nameof(control), "A new control has to be created every time maker is started");

            _sidebarEntries.Add(control);
            return control;
        }

        /// <summary>
        /// Add a control to the accessory selection and settings window.
        /// </summary>
        public static T AddAccessoryWindowControl<T>(T control) where T : BaseGuiEntry
        {
            if (control == null) throw new ArgumentNullException(nameof(control));
            if (control.IsDisposed) throw new ObjectDisposedException(nameof(control), "A new control has to be created every time maker is started");

            control.Category = _accessorySlotWindowCategory;
            _accessoryWindowEntries.Add(control);
            return control;
        }

        /// <summary>
        /// 0 is male, 1 is female
        /// </summary>
        public static int GetMakerSex() => GetMakerBase().modeSex;

        /// <summary>
        /// Returns current maker logic instance.
        /// Same as <see cref="Singleton{CustomBase}.Instance"/>
        /// </summary>
        public static CustomBase GetMakerBase() => CustomBase.Instance;

        /// <summary>
        /// Get the ChaControl of the character serving as a preview in character maker.
        /// Outside of character maker and early on in maker load process this returns null.
        /// </summary>
        public static ChaControl GetCharacterControl() => InsideMaker ? GetMakerBase()?.chaCtrl : null;

#if KK 
        /// <summary>
        /// Check if the maker was loaded from within classroom select screen in main game
        /// </summary>
        public static bool IsInsideClassMaker() => InsideMaker && Manager.Scene.Instance.NowSceneNames.Contains("ClassRoomSelect");

        /// <summary>
        /// Currently selected maker coordinate
        /// </summary>
        public static ChaFileDefine.CoordinateType GetCurrentCoordinateType() => (ChaFileDefine.CoordinateType)GetMakerBase().chaCtrl.fileStatus.coordinateType;
#endif

        /// <summary>
        /// This event is fired every time the character maker is being loaded, near the very beginning.
        /// This is the only chance to add custom sub categories. Custom controls can be added now on later in <see cref="MakerBaseLoaded"/>.
        /// Warning: All custom subcategories and custom controls are cleared on maker exit and need to be re-added on next maker start.
        /// It's recommended to completely clear your GUI state in <see cref="MakerExiting"/> in preparation for loading into maker again.
        /// </summary>
        public static event EventHandler<RegisterSubCategoriesEvent> RegisterCustomSubCategories;

        /// <summary>
        /// Early in the process of maker loading. Most game components are initialized and had their Start methods ran.
        /// Warning: Some components and objects might not be loaded or initialized yet, especially if they are mods.
        /// Warning: All custom subcategories and custom controls are cleared on maker exit and need to be re-added on next maker
        /// start.
        /// </summary>
        public static event EventHandler<RegisterCustomControlsEvent> MakerStartedLoading;

        /// <summary>
        /// Maker is fully loaded. Use to load mods that rely on something that is loaded late, else use MakerStartedLoading.
        /// This is the last chance to add custom controls!
        /// Warning: All custom subcategories and custom controls are cleared on maker exit and need to be re-added on next maker
        /// start.
        /// </summary>
        public static event EventHandler<RegisterCustomControlsEvent> MakerBaseLoaded;

        /// <summary>
        /// Maker is fully loaded and the user has control.
        /// Warning: Avoid loading mods or doing anything heavy in this event, use EarlyMakerFinishedLoading instead.
        /// </summary>
        public static event EventHandler MakerFinishedLoading;

        /// <summary>
        /// Fired after the user exits the maker. Use this to clean up any references and resources.
        /// You want to return to the state you were in before maker was loaded.
        /// </summary>
        public static event EventHandler MakerExiting;

        private static void OnRegisterCustomSubCategories()
        {
            MakerLoadToggle.Setup();
            MakerCoordinateLoadToggle.Setup();

            if (RegisterCustomSubCategories != null)
            {
                var args = new RegisterSubCategoriesEvent();
                foreach (var handler in RegisterCustomSubCategories.GetInvocationList())
                {
                    try
                    {
                        ((EventHandler<RegisterSubCategoriesEvent>)handler).Invoke(KoikatuAPI.Instance, args);
                    }
                    catch (Exception e)
                    {
                        KoikatuAPI.Logger.LogError(e);
                    }
                }
            }
        }

        private static void OnMakerStartedLoading()
        {
            if (MakerStartedLoading != null)
            {
                var args = new RegisterCustomControlsEvent();
                foreach (var handler in MakerStartedLoading.GetInvocationList())
                {
                    try
                    {
                        ((EventHandler<RegisterCustomControlsEvent>)handler).Invoke(KoikatuAPI.Instance, args);
                    }
                    catch (Exception e)
                    {
                        KoikatuAPI.Logger.LogError(e);
                    }
                }
            }
        }

        private static void OnMakerFinishedLoading()
        {
            if (MakerFinishedLoading != null)
            {
                foreach (var handler in MakerFinishedLoading.GetInvocationList())
                {
                    try
                    {
                        ((EventHandler)handler).Invoke(KoikatuAPI.Instance, EventArgs.Empty);
                    }
                    catch (Exception e)
                    {
                        KoikatuAPI.Logger.LogError(e);
                    }
                }
            }

            _makerLoaded = true;
        }

        private static void OnMakerBaseLoaded()
        {
            if (MakerBaseLoaded != null)
            {
                var args = new RegisterCustomControlsEvent();
                foreach (var handler in MakerBaseLoaded.GetInvocationList())
                {
                    try
                    {
                        ((EventHandler<RegisterCustomControlsEvent>)handler).Invoke(KoikatuAPI.Instance, args);
                    }
                    catch (Exception e)
                    {
                        KoikatuAPI.Logger.LogError(e);
                    }
                }
            }

            DebugControls();

            foreach (var baseGuiEntry in _guiEntries)
                baseGuiEntry.Initialize();
        }

        private static void OnCreateCustomControls()
        {
            CreateCustomControls();
            MakerLoadToggle.CreateCustomToggles();
            MakerCoordinateLoadToggle.CreateCustomToggles();

#if KK 
            // Fix some plugins failing to update interface and losing state
            if (IsInsideClassMaker())
            {
                OnChaFileLoaded(new ChaFileLoadedEventArgs(null, (byte)GetMakerSex(), true, true, true, true, true, GetCharacterControl().chaFile, LastLoadedChaFile));
                OnReloadInterface(new CharaReloadEventArgs(GetCharacterControl()));
            }
#endif
        }

        private static void OnMakerExiting()
        {
            if (MakerExiting != null)
            {
                foreach (var handler in MakerExiting.GetInvocationList())
                {
                    try
                    {
                        ((EventHandler)handler).Invoke(KoikatuAPI.Instance, EventArgs.Empty);
                    }
                    catch (Exception e)
                    {
                        KoikatuAPI.Logger.LogError(e);
                    }
                }
            }

            RemoveCustomControls();
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

        /// <summary>
        /// Use to avoid unnecessary processing cards when they are loaded to the character list.
        /// For example, don't load extended data for these characters since it's never used.
        /// </summary>
        public static bool CharaListIsLoading { get; private set; }

        /// <summary>
        /// ChaFile of the character currently opened in maker. Do not use to save extended data, or it will be lost when saving the card.
        /// Use ChaFile from <code>ExtendedSave.CardBeingSaved</code> event to save extended data instead.
        /// </summary>
        public static ChaFile LastLoadedChaFile => InsideMaker ? (Hooks.InternalLastLoadedChaFile ?? GetCharacterControl()?.chaFile) : null;

        /// <summary>
        /// Fired when the current ChaFile in maker is being changed by loading other cards or coordinates.
        /// This event is only fired when inside the character maker.
        /// 
        /// You might need to wait for the next frame with <see cref="MonoBehaviour.StartCoroutine(IEnumerator)"/> before handling this.
        /// </summary>
        [Obsolete("Use ReloadCustomInterface instead")]
        public static event EventHandler<ChaFileLoadedEventArgs> ChaFileLoaded;

        private static void OnChaFileLoaded(ChaFileLoadedEventArgs chaFileLoadedEventArgs)
        {
            if (ChaFileLoaded != null)
            {
                foreach (var handler in ChaFileLoaded.GetInvocationList())
                {
                    try
                    {
                        ((EventHandler<ChaFileLoadedEventArgs>)handler).Invoke(KoikatuAPI.Instance, chaFileLoadedEventArgs);
                    }
                    catch (Exception e)
                    {
                        KoikatuAPI.Logger.LogError(e);
                    }
                }
            }
        }

        /// <summary>
        /// Fired after character or coordinate is loaded in maker, after all controllers had their events fired.
        /// This event is only fired when inside the character maker. Use this to update values of custom controls.
        /// EventArgs can be either <see cref="CharaReloadEventArgs"/> or <see cref="CoordinateEventArgs"/> depending on why the reload happened.
        /// </summary>
        public static event EventHandler ReloadCustomInterface;

        internal static void OnReloadInterface(EventArgs args)
        {
            if (ReloadCustomInterface != null)
            {
                foreach (var handler in ReloadCustomInterface.GetInvocationList())
                {
                    try
                    {
                        ((EventHandler)handler).Invoke(KoikatuAPI.Instance, args);
                    }
                    catch (Exception e)
                    {
                        KoikatuAPI.Logger.LogError(e);
                    }
                }
            }
        }

        private static bool _insideMaker;
        private static bool _makerLoaded;

        /// <summary>
        /// Firen whenever <see cref="InsideMaker"/> changes. This is the earliest event fired when user starts the character maker.
        /// </summary>
        public static event EventHandler InsideMakerChanged;

        /// <summary>
        /// The maker scene is currently loaded. It might still be loading!
        /// </summary>
        public static bool InsideMaker
        {
            get => _insideMaker;
            private set
            {
                if (_insideMaker != value)
                {
                    _insideMaker = value;

                    if (InsideMakerChanged != null)
                    {
                        foreach (var handler in InsideMakerChanged.GetInvocationList())
                        {
                            try
                            {
                                ((EventHandler)handler).Invoke(KoikatuAPI.Instance, EventArgs.Empty);
                            }
                            catch (Exception e)
                            {
                                KoikatuAPI.Logger.LogError(e);
                            }
                        }
                    }
                }

                if (!_insideMaker)
                    _makerLoaded = false;
            }
        }

        /// <summary>
        /// Maker is fully loaded and running
        /// </summary>
        public static bool InsideAndLoaded => InsideMaker && _makerLoaded;

        /// <summary>
        /// Get values of the default partial load checkboxes present at the bottom of the 
        /// character load window (load face, body, hair, parameters, clothes).
        /// Returns null if the values could not be collected (safe to assume it's the same as being enabled).
        /// </summary>
        public static CharacterLoadFlags GetCharacterLoadFlags()
        {
            if (!InsideAndLoaded) return null;

#if KK || EC
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
#elif AI 
			// todo
            return null;
#endif
        }

        internal static void Init(bool insideStudio)
        {
            if (insideStudio) return;

            Hooks.Init();
        }

        /// <summary>
        /// Check if maker interface is currently visible and not obscured by settings screen or other things.
        /// Useful for knowing when to display OnGui mod windows in maker.
        /// </summary>
        public static bool IsInterfaceVisible()
        {
            // Check if maker is loaded
            if (!InsideMaker)
                return false;
            var mbase = GetMakerBase();
            if (mbase == null || mbase.chaCtrl == null)
                return false;

            // Check if the loading screen is currently visible
            if (Manager.Scene.Instance.IsNowLoadingFade)
                return false;

#if KK || EC
            // Check if UI is hidden (by pressing space)
            if (mbase.customCtrl.hideFrontUI)
                return false;
#endif

            // Check if settings screen, game exit message box or similar are on top of the maker UI
            // In KK class maker the AddSceneName is set to CustomScene, but in normal maker it's empty
            if (!string.IsNullOrEmpty(Manager.Scene.Instance.AddSceneName) && Manager.Scene.Instance.AddSceneName != "CustomScene")
                return false;

            return true;
        }
    }
}
