using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using BepInEx;
using BepInEx.Logging;
using ChaCustom;
using Harmony;
using UniRx;
using UnityEngine;
using Logger = BepInEx.Logger;

namespace MakerAPI
{
    [BepInPlugin(GUID, "Character Maker API", Version)]
    public partial class MakerAPI : BaseUnityPlugin
    {
        public const string Version = "1.2";
        public const string GUID = "com.bepis.makerapi";

        private readonly List<MakerCategory> _categories = new List<MakerCategory>();
        private readonly List<BaseGuiEntry> _guiEntries = new List<BaseGuiEntry>();
        private bool _insideMaker;

        public MakerAPI()
        {
            Instance = this;
        }

        public static MakerAPI Instance { get; private set; }
        //public CustomBase CurrentCustomScene { get; private set; }

        private void Start()
        {
            InsideStudio = Application.productName == "CharaStudio";

            if (!InsideStudio)
            {
                var harmony = HarmonyInstance.Create(GUID);
                harmony.PatchAll(typeof(Hooks));
            }
        }

        private void CreateCustomControls()
        {
            foreach (Transform categoryTransfrom in GameObject.Find("CvsMenuTree").transform)
                CreateCustomControlsInCategory(categoryTransfrom);
        }

        private void CreateCustomControlsInCategory(Transform categoryTransfrom)
        {
            foreach (var subCategoryGroup in _guiEntries
                .Where(x => x.Category.CategoryName == categoryTransfrom.name)
                .GroupBy(x => x.Category.SubCategoryName))
            {
                var categorySubTransform = categoryTransfrom.Find(subCategoryGroup.Key);

                if (categorySubTransform != null)
                {
                    var contentParent = FindSubcategoryContentParent(categorySubTransform);

                    BaseUnityPlugin lastOwner = contentParent.childCount > 1 ? this : null;
                    foreach (var customControl in subCategoryGroup)
                    {
                        if (lastOwner != customControl.Owner && lastOwner != null)
                            new MakerSeparator(new MakerCategory(null, null), this).CreateControl(contentParent);

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

        private void RemoveCustomControls()
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
        private void AddMissingSubCategories(UI_ToggleGroupCtrl mainCategory)
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

            StartCoroutine(FixCategoryContentOffsets(mainCategory));
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
        internal T AddControl<T>(T control) where T : BaseGuiEntry
        {
            if (control == null) throw new ArgumentNullException(nameof(control));
            if (control is MakerLoadToggle)
                throw new ArgumentException("Can't add a MakerLoadToggle as a control", nameof(control));

            _guiEntries.Add(control);
            return control;
        }

        /// <summary>
        /// Add custom sub categories. They need to be added before maker starts loading,
        /// or in the RegisterCustomSubCategories event.
        /// </summary>
        internal void AddSubCategory(MakerCategory category)
        {
            if (!_categories.Contains(category))
                _categories.Add(category);
            else
                Logger.Log(LogLevel.Info, $"[MakerAPI] Duplicate custom subcategory was added: {category} The duplicate will be ignored.");
        }

        /// <summary>
        /// 0 is male, 1 is female
        /// </summary>
        public int GetMakerSex() => GetMakerBase().modeSex;

        public CustomBase GetMakerBase() => CustomBase.Instance;

        public ChaControl GetCharacterControl() => InsideMaker ? GetMakerBase()?.chaCtrl : null;

        /// <summary>
        /// Currently selected maker coordinate
        /// </summary>
        public ChaFileDefine.CoordinateType GetCurrentCoordinateType() => (ChaFileDefine.CoordinateType)GetMakerBase().chaCtrl.fileStatus.coordinateType;

        /// <summary>
        /// Called at the very beginning of maker loading. This is the only chance to add custom sub categories.
        /// Warning: All custom subcategories and custom controls are cleared on maker exit and need to be re-added on next maker
        /// start.
        /// </summary>
        public event EventHandler<RegisterSubCategoriesEvent> RegisterCustomSubCategories;

        /// <summary>
        /// Early in the process of maker loading. Most game components are initialized and had their Start methods ran.
        /// Warning: Some components and objects might not be loaded or initialized yet, especially if they are mods.
        /// Warning: All custom subcategories and custom controls are cleared on maker exit and need to be re-added on next maker
        /// start.
        /// </summary>
        public event EventHandler<RegisterCustomControlsEvent> MakerStartedLoading;

        /// <summary>
        /// Maker is fully loaded. Use to load mods that rely on something that is loaded late, else use MakerStartedLoading.
        /// This is the last chance to add custom controls!
        /// Warning: All custom subcategories and custom controls are cleared on maker exit and need to be re-added on next maker
        /// start.
        /// </summary>
        public event EventHandler<RegisterCustomControlsEvent> MakerBaseLoaded;

        /// <summary>
        /// Maker is fully loaded and the user has control.
        /// Warning: Avoid loading mods or doing anything heavy in this event, use EarlyMakerFinishedLoading instead.
        /// </summary>
        public event EventHandler MakerFinishedLoading;

        public event EventHandler MakerExiting;

        private void OnRegisterCustomSubCategories()
        {
            MakerLoadToggle.Setup();

            //Logger.Log(LogLevel.Debug, "OnRegisterCustomSubCategories");
            RegisterCustomSubCategories?.Invoke(this, new RegisterSubCategoriesEvent(this));
        }

        private void OnMakerStartedLoading()
        {
            //Logger.Log(LogLevel.Debug, "Character Maker Started Loading");
            MakerStartedLoading?.Invoke(this, new RegisterCustomControlsEvent(this));
        }

        private void OnMakerFinishedLoading()
        {
            //Logger.Log(LogLevel.Debug, "Character Maker Finished Loading");
            MakerFinishedLoading?.Invoke(this, EventArgs.Empty);
        }

        private void OnMakerBaseLoaded()
        {
            //Logger.Log(LogLevel.Debug, "Character Maker Base Loaded");
            MakerBaseLoaded?.Invoke(this, new RegisterCustomControlsEvent(this));

            DebugControls();

            foreach (var baseGuiEntry in _guiEntries)
                baseGuiEntry.Initialize();
        }

        private void OnCreateCustomControls()
        {
            CreateCustomControls();
            MakerLoadToggle.CreateCustomToggles();
        }

        private void OnMakerExiting()
        {
            //Logger.Log(LogLevel.Debug, "Character Maker is exiting");
            MakerExiting?.Invoke(this, EventArgs.Empty);

            RemoveCustomControls();
            MakerLoadToggle.Reset();
        }

        [Conditional("DEBUG")]
        private void DebugControls()
        {
            var cat = MakerConstants.BuiltInCategories.First();
            AddControl(new MakerSeparator(cat, this));
            AddControl(new MakerSeparator(cat, this));
            AddControl(new MakerSeparator(cat, this));
            AddControl(new MakerToggle(cat, "test toggle", this))
                .ValueChanged.Subscribe(b => Logger.Log(LogLevel.Message, b));
            AddControl(new MakerButton("test btn", cat, this))
                .OnClick.AddListener(() => Logger.Log(LogLevel.Message, "Clicked"));
            AddControl(new MakerColor("test color", true, cat, Color.magenta, this))
                .ValueChanged.Subscribe(color => Logger.Log(LogLevel.Message, color));
            AddControl(new MakerDropdown("test toggle", new[] { "t0", "t1", "t2", "t3" }, cat, 1, this))
                .ValueChanged.Subscribe(b => Logger.Log(LogLevel.Message, b));
            AddControl(new MakerRadioButtons(cat, this, "radio btns", "b1", "b2"))
                .ValueChanged.Subscribe(b => Logger.Log(LogLevel.Message, b));
            AddControl(new MakerSlider(cat, "test slider", 0, 1, 1, this))
                .ValueChanged.Subscribe(b => Logger.Log(LogLevel.Message, b));
            AddControl(new MakerText("test text test text test text test text test text test " +
                                     "text test text test text test text test text", cat, this));

            MakerLoadToggle.AddLoadToggle(new MakerLoadToggle("Test toggle"))
                .ValueChanged.Subscribe(b => Logger.Log(LogLevel.Message, b));
            MakerLoadToggle.AddLoadToggle(new MakerLoadToggle("Test toggle 2"))
                .ValueChanged.Subscribe(b => Logger.Log(LogLevel.Message, b));
        }

        /// <summary>
        /// Use to avoid unnecessary processing cards when they are loaded to the character list.
        /// For example, don't load extended data for these characters since it's never used.
        /// </summary>
        public bool CharaListIsLoading { get; private set; }

        /// <summary>
        /// ChaFile of the character currently opened in maker. Do not use to save extended data, or it will be lost when saving the card.
        /// Use ChaFile from <code>ExtendedSave.CardBeingSaved</code> event to save extended data instead.
        /// </summary>
        public ChaFile LastLoadedChaFile => InsideMaker ? (Hooks.LastLoadedChaFile ?? GetCharacterControl()?.chaFile) : null;

        /// <summary>
        /// Fired when the current ChaFile in maker is changed by loading other cards or coordinates.
        /// Only fired when inside maker.
        /// </summary>
        public event EventHandler<ChaFileLoadedEventArgs> ChaFileLoaded;

        private void OnChaFileLoaded(ChaFileLoadedEventArgs chaFileLoadedEventArgs)
        {
            ChaFileLoaded?.Invoke(this, chaFileLoadedEventArgs);
        }

        public event EventHandler InsideMakerChanged;

        /// <summary>
        /// Maker is currently loaded and running
        /// </summary>
        public bool InsideMaker
        {
            get { return _insideMaker; }
            private set
            {
                if (_insideMaker != value)
                {
                    _insideMaker = value;
                    InsideMakerChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        /// <summary>
        /// If we are in the CharaStudio the API has very limited functionality
        /// </summary>
        public bool InsideStudio { get; private set; }
    }
}
