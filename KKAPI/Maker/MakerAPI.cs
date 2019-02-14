using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using BepInEx;
using BepInEx.Logging;
using ChaCustom;
using Harmony;
using KKAPI.Maker.UI;
using UniRx;
using UnityEngine;
using Logger = BepInEx.Logger;
using Object = UnityEngine.Object;

namespace KKAPI.Maker
{
    /// <summary>
    /// Provides a way to add custom items to the in-game Character Maker, and gives useful methods for interfacing with the maker.
    /// </summary>
    public static partial class MakerAPI
    {
        private static readonly List<MakerCategory> _categories = new List<MakerCategory>();
        private static readonly List<BaseGuiEntry> _guiEntries = new List<BaseGuiEntry>();

        private static void CreateCustomControls()
        {
            foreach (Transform categoryTransfrom in GameObject.Find("CvsMenuTree").transform)
                CreateCustomControlsInCategory(categoryTransfrom);
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
                    var contentParent = FindSubcategoryContentParent(categorySubTransform);

                    BaseUnityPlugin lastOwner = contentParent.childCount > 1 ? KoikatuAPI.Instance : null;
                    foreach (var customControl in subCategoryGroup)
                    {
                        if (lastOwner != customControl.Owner && lastOwner != null)
                            new MakerSeparator(new MakerCategory(null, null), KoikatuAPI.Instance).CreateControl(contentParent);

                        customControl.CreateControl(contentParent);
                        lastOwner = customControl.Owner;
                    }

                    Logger.Log(LogLevel.Debug, $"[MakerAPI] Added {subCategoryGroup.Count()} custom controls " +
                                               $"to {categoryTransfrom.name}/{subCategoryGroup.Key}");
                }
                else
                {
                    Logger.Log(LogLevel.Error, $"[MakerAPI] Failed to add {subCategoryGroup.Count()} custom controls " +
                        $"to {categoryTransfrom.name}/{subCategoryGroup.Key} - The category was not registered with AddSubCategory.");
                }
            }
        }

        private static void RemoveCustomControls()
        {
            foreach (var guiEntry in _guiEntries)
                guiEntry.Dispose();

            _guiEntries.Clear();
            _categories.Clear();
        }

        private static Transform FindSubcategoryContentParent(Transform categorySubTransform)
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

            var transformsToSort = new List<Tuple<Transform, int>>();
            foreach (var category in _categories)
            {
                if (categoryTransfrom.name != category.CategoryName) continue;

                var categorySubTransform = categoryTransfrom.Find(category.SubCategoryName);
                if (categorySubTransform == null)
                {
                    categorySubTransform = SubCategoryCreator.AddNewSubCategory(mainCategory, category);
                }

                transformsToSort.Add(new Tuple<Transform, int>(categorySubTransform, category.Position));
            }

            if (noSorting) return;

            foreach (Transform subTransform in categoryTransfrom)
            {
                if (transformsToSort.Any(x => x.Item1 == subTransform)) continue;

                var builtInCategory = MakerConstants.GetBuiltInCategory(categoryTransfrom.name, subTransform.name);
                if (builtInCategory != null)
                    transformsToSort.Add(new Tuple<Transform, int>(subTransform, builtInCategory.Position));
                else
                    Logger.Log(LogLevel.Warning, $"[MakerAPI] Missing MakerCategory for existing transfrom {categoryTransfrom.name} / {subTransform.name}");
            }

            var index = 0;
            foreach (var tuple in transformsToSort.OrderBy(x => x.Item2))
                tuple.Item1.SetSiblingIndex(index++);

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
            if (control is MakerLoadToggle || control is MakerCoordinateLoadToggle)
                throw new ArgumentException("Can't add a MakerLoadToggle as a control", nameof(control));

            _guiEntries.Add(control);
            return control;
        }

        /// <summary>
        /// Add custom sub categories. They need to be added before maker starts loading,
        /// or in the RegisterCustomSubCategories event.
        /// </summary>
        internal static  void AddSubCategory(MakerCategory category)
        {
            if (!_categories.Contains(category))
                _categories.Add(category);
            else
                Logger.Log(LogLevel.Info, $"[MakerAPI] Duplicate custom subcategory was added: {category} The duplicate will be ignored.");
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

        /// <summary>
        /// Currently selected maker coordinate
        /// </summary>
        public static ChaFileDefine.CoordinateType GetCurrentCoordinateType() => (ChaFileDefine.CoordinateType)GetMakerBase().chaCtrl.fileStatus.coordinateType;

        /// <summary>
        /// Called at the very beginning of maker loading. This is the only chance to add custom sub categories.
        /// Warning: All custom subcategories and custom controls are cleared on maker exit and need to be re-added on next maker
        /// start.
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
        /// </summary>
        public static event EventHandler MakerExiting;

        private static void OnRegisterCustomSubCategories()
        {
            MakerLoadToggle.Setup();
            MakerCoordinateLoadToggle.Setup();

            //Logger.Log(LogLevel.Debug, "OnRegisterCustomSubCategories");
            RegisterCustomSubCategories?.Invoke(KoikatuAPI.Instance, new RegisterSubCategoriesEvent());
        }

        private static void OnMakerStartedLoading()
        {
            //Logger.Log(LogLevel.Debug, "Character Maker Started Loading");
            MakerStartedLoading?.Invoke(KoikatuAPI.Instance, new RegisterCustomControlsEvent());
        }

        private static void OnMakerFinishedLoading()
        {
            //Logger.Log(LogLevel.Debug, "Character Maker Finished Loading");
            MakerFinishedLoading?.Invoke(KoikatuAPI.Instance, EventArgs.Empty);

            _makerLoaded = true;
        }

        private static void OnMakerBaseLoaded()
        {
            //Logger.Log(LogLevel.Debug, "Character Maker Base Loaded");
            MakerBaseLoaded?.Invoke(KoikatuAPI.Instance, new RegisterCustomControlsEvent());

            DebugControls();

            foreach (var baseGuiEntry in _guiEntries)
                baseGuiEntry.Initialize();
        }

        private static void OnCreateCustomControls()
        {
            CreateCustomControls();
            MakerLoadToggle.CreateCustomToggles();
            MakerCoordinateLoadToggle.CreateCustomToggles();
        }

        private static void OnMakerExiting()
        {
            //Logger.Log(LogLevel.Debug, "Character Maker is exiting");
            MakerExiting?.Invoke(KoikatuAPI.Instance, EventArgs.Empty);

            RemoveCustomControls();
            MakerLoadToggle.Reset();
            MakerCoordinateLoadToggle.Reset();
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
                .ValueChanged.Subscribe(b => Logger.Log(LogLevel.Message, b));
            AddControl(new MakerButton("test btn", cat, instance))
                .OnClick.AddListener(() => Logger.Log(LogLevel.Message, "Clicked"));
            AddControl(new MakerColor("test color", true, cat, Color.magenta, instance))
                .ValueChanged.Subscribe(color => Logger.Log(LogLevel.Message, color));
            AddControl(new MakerDropdown("test toggle", new[] { "t0", "t1", "t2", "t3" }, cat, 1, instance))
                .ValueChanged.Subscribe(b => Logger.Log(LogLevel.Message, b));
            AddControl(new MakerRadioButtons(cat, instance, "radio btns", "b1", "b2"))
                .ValueChanged.Subscribe(b => Logger.Log(LogLevel.Message, b));
            AddControl(new MakerSlider(cat, "test slider", 0, 1, 1, instance))
                .ValueChanged.Subscribe(b => Logger.Log(LogLevel.Message, b));
            AddControl(new MakerText("test text test text test text test text test text test " +
                                     "text test text test text test text test text", cat, instance));

            MakerLoadToggle.AddLoadToggle(new MakerLoadToggle("Test toggle"))
                .ValueChanged.Subscribe(b => Logger.Log(LogLevel.Message, b));
            MakerLoadToggle.AddLoadToggle(new MakerLoadToggle("Test toggle 2"))
                .ValueChanged.Subscribe(b => Logger.Log(LogLevel.Message, b));

            MakerCoordinateLoadToggle.AddLoadToggle(new MakerCoordinateLoadToggle("Test toggle"))
                .ValueChanged.Subscribe(b => Logger.Log(LogLevel.Message, b));
            MakerCoordinateLoadToggle.AddLoadToggle(new MakerCoordinateLoadToggle("Test toggle 2"))
                .ValueChanged.Subscribe(b => Logger.Log(LogLevel.Message, b));
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
        public static ChaFile LastLoadedChaFile => InsideMaker ? (Hooks.LastLoadedChaFile ?? GetCharacterControl()?.chaFile) : null;

        /// <summary>
        /// Fired when the current ChaFile in maker is changed by loading other cards or coordinates.
        /// Only fired when inside maker.
        /// </summary>
        public static event EventHandler<ChaFileLoadedEventArgs> ChaFileLoaded;

        private static void OnChaFileLoaded(ChaFileLoadedEventArgs chaFileLoadedEventArgs)
        {
            ChaFileLoaded?.Invoke(KoikatuAPI.Instance, chaFileLoadedEventArgs);
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
            get { return _insideMaker; }
            private set
            {
                if (_insideMaker != value)
                {
                    _insideMaker = value;
                    InsideMakerChanged?.Invoke(KoikatuAPI.Instance, EventArgs.Empty);
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
            if (!InsideMaker) return null;

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

        internal static void Init(bool insideStudio)
        {
            if(insideStudio) return;

            HarmonyInstance.Create(typeof(Hooks).FullName).PatchAll(typeof(Hooks));
        }
    }
}
