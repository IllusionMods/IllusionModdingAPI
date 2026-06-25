using KKAPI.Chara;
using KKAPI.Studio.SaveLoad;
using KKAPI.Studio.UI;
using KKAPI.Studio.UI.Toolbars;
using Studio;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using KKAPI.Utilities;
using UniRx;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace KKAPI.Studio
{
    /// <summary>
    /// Provides a way to add custom menu items to CharaStudio, and gives useful methods for interfacing with the studio.
    /// </summary>
    public static partial class StudioAPI
    {
        private static global::Studio.Studio _studioInstance;
        internal static global::Studio.Studio StudioInstance => _studioInstance ? _studioInstance : _studioInstance = global::Studio.Studio.Instance;

        private static readonly List<CurrentStateCategory> _customCurrentStateCategories = new List<CurrentStateCategory>();
        private static GameObject _customStateRoot;

        /// <summary>
        /// Add a new custom category to the Anim > CurrentState tab in the studio top-left menu.
        /// Can use this at any point.
        /// </summary>
        [Obsolete("Use GetOrCreateCurrentStateCategory instead")]
        public static void CreateCurrentStateCategory(CurrentStateCategory category)
        {
            if (category == null) throw new ArgumentNullException(nameof(category));

            if (!InsideStudio)
            {
                KoikatuAPI.Logger.LogDebug("Tried to run StudioAPI.CreateCurrentStateCategory outside of studio!");
                return;
            }

            if (StudioLoaded)
                CreateCategory(category);

            _customCurrentStateCategories.Add(category);
        }

        /// <summary>
        /// Add a new custom category to the Anim > CurrentState tab in the studio top-left menu.
        /// Can use this at any point. Always returns null outside of studio.
        /// If the name is empty or null, the Misc/Other category is returned.
        /// </summary>
        public static CurrentStateCategory GetOrCreateCurrentStateCategory(string name)
        {
            if (!InsideStudio)
            {
                KoikatuAPI.Logger.LogDebug("Tried to run StudioAPI.CreateCurrentStateCategory outside of studio!");
                return null;
            }

            if (string.IsNullOrEmpty(name)) name = "Misc/Other";

            var existing = _customCurrentStateCategories.FirstOrDefault(x => x.CategoryName == name);
            if (existing != null) return existing;

            var newCategory = new CurrentStateCategory(name);

            if (StudioLoaded)
                CreateCategory(newCategory);

            _customCurrentStateCategories.Add(newCategory);

            return newCategory;
        }

        /// <summary>
        /// Get all instances of this controller that belong to characters that are selected in Studio's Workspace.
        /// </summary>
        public static IEnumerable<T> GetSelectedControllers<T>() where T : CharaCustomFunctionController
        {
            return GetSelectedCharacters().Select(x => x.GetChaControl()?.GetComponent<T>()).Where(x => x != null);
        }

        /// <summary>
        /// Get all character objects currently selected in Studio's Workspace.
        /// </summary>
        public static IEnumerable<OCIChar> GetSelectedCharacters()
        {
            return GetSelectedObjects().OfType<OCIChar>();
        }

        /// <summary>
        /// Get all objects (all types) currently selected in Studio's Workspace.
        /// </summary>
        public static IEnumerable<ObjectCtrlInfo> GetSelectedObjects()
        {
            if (!StudioLoaded) return Enumerable.Empty<ObjectCtrlInfo>();

            return GetObjectCtrlInfos(GetSelectedTreeNodes());
        }

        private static IEnumerable<ObjectCtrlInfo> GetObjectCtrlInfos(IList<TreeNodeObject> selectNodes)
        {
            return selectNodes.Select(node => StudioInstance.dicInfo.TryGetValue(node, out var objectCtrlInfo) ? objectCtrlInfo : null)
                              .Where(x => x != null);
        }

        /// <summary>
        /// Get all tree nodes currently selected in Studio's Workspace. Returns an empty array if called outside of studio or before studio finishes loading.
        /// </summary>
        public static TreeNodeObject[] GetSelectedTreeNodes()
        {
            return StudioLoaded ? StudioInstance.treeNodeCtrl.selectNodes : new TreeNodeObject[0];
        }

        private static void CreateCategory(CurrentStateCategory category)
        {
            if (category == null) throw new ArgumentNullException(nameof(category));

            if (_customStateRoot == null)
                _customStateRoot = GameObject.Find("StudioScene/Canvas Main Menu/02_Manipulate/00_Chara/01_State/Viewport/Content");

            category.CreateCategory(_customStateRoot);
        }

        internal static void Init(bool insideStudio)
        {
            InsideStudio = insideStudio;

            if (!insideStudio) return;

            Hooks.SetupHooks();
            StudioSaveLoadApi.Init();

            SceneManager.sceneLoaded += SceneLoaded;

            DebugControls();

            RegisterTreeNodeContextMenuItems(MenuOrder.BasicCommands, BasicTreeNodeCommands);

            ICollection<GlobalContextMenu.Entry> BasicTreeNodeCommands(TreeNodeClickedEventArgs args)
            {
                var results = new List<GlobalContextMenu.Entry>();

                var allNodes = args.SelectedInstances.SelectMany(x => x.Flatten()).ToList();
                var notSelected = allNodes.Except(args.SelectedInstances).ToList();
                if (notSelected.Count > 0)
                {
                    results.Add(GlobalContextMenu.Entry.Create(new GUIContent("Select children"), () =>
                    {
                        foreach (var tno in notSelected)
#if !PH
                            global::Studio.Studio.Instance.treeNodeCtrl.AddSelectNode(tno, true);
#else
                            global::Studio.Studio.Instance.treeNodeCtrl.AddSelectNode(tno);
#endif
                    }));
                }

                if (args.SelectedInstances.Count >= 2 && args.SelectedInstances.Any(x => x.enableChangeParent))
                    results.Add(GlobalContextMenu.Entry.Create(new GUIContent("Parent"), () => global::Studio.Studio.Instance.m_WorkspaceCtrl.OnClickParent()));
                if (args.SelectedInstances.Any(x => x.isParent))
                    results.Add(GlobalContextMenu.Entry.Create(new GUIContent("Unparent"), () => global::Studio.Studio.Instance.m_WorkspaceCtrl.OnClickRemove()));
                if (args.SelectedInstances.Any(x => x.enableDelete))
                    results.Add(GlobalContextMenu.Entry.Create(new GUIContent("Delete"), () => global::Studio.Studio.Instance.m_WorkspaceCtrl.OnClickDelete()));
                if (args.SelectedInstances.Any(x => x.enableCopy))
                    results.Add(GlobalContextMenu.Entry.Create(new GUIContent("Duplicate"), () => global::Studio.Studio.Instance.m_WorkspaceCtrl.OnClickDuplicate()));


                return results;
            }
        }

        /// <summary>
        /// True if we are currently inside CharaStudio.exe
        /// </summary>
        public static bool InsideStudio { get; private set; }

        /// <summary>
        /// True inside studio after it finishes loading the interface (when the starting loading screen finishes), 
        /// right before custom controls are created.
        /// </summary>
        public static bool StudioLoaded { get; private set; }

        /// <summary>
        /// Fires once after studio finished loading the interface, right before custom controls are created.
        /// </summary>
        public static event EventHandler StudioLoadedChanged;

        #region Workspace right click on items

        public sealed class TreeNodeClickedEventArgs : EventArgs
        {
            public TreeNodeObject ClickedInstance { get; }
            public ICollection<TreeNodeObject> SelectedInstances { get; }

            public IEnumerable<ObjectCtrlInfo> GetSelectedObjects() => StudioAPI.GetSelectedObjects();

            public TreeNodeClickedEventArgs(TreeNodeObject clickedInstance)
            {
                ClickedInstance = clickedInstance;
                SelectedInstances = StudioLoaded ? GetSelectedTreeNodes() : new TreeNodeObject[0];
            }
        }

        /// <summary>
        /// Defines the order in which right click handlers are called when a tree node is right clicked in the workspace.
        /// This is used to determine the order of menu items in the context menu. Lower values are called first and appear higher in the menu.
        /// </summary>
        public enum MenuOrder
        {
            /// <summary>
            /// Always be at the top of the menu, above all other items. Use with caution.
            /// </summary>
            Topmost = -100,
            /// <summary>
            /// Appear at the top of the menu.
            /// </summary>
            Top = -50,
            /// <summary>
            /// Appear above normal items in the menu.
            /// </summary>
            AboveNormal = -25,
            /// <summary>
            /// Default order for menu items. Appear in the middle of the menu.
            /// </summary>
            Normal = 0,
            /// <summary>
            /// Appear before normal items in the menu.
            /// </summary>
            BelowNormal = 25,
            /// <summary>
            /// Commands included by default in ModdingAPI.
            /// </summary>
            BasicCommands = 40,
            /// <summary>
            /// Appear at the bottom of the menu.
            /// </summary>
            Bottom = 50,
            /// <summary>
            /// Always be at the bottom of the menu, below all other items. Use with caution.
            /// </summary>
            Bottommost = 100
        }

        /// <summary>
        /// Callback used to get context menu items for when a tree node is right clicked in the workspace.
        /// </summary>
        /// <param name="args">Information about clicked tree node</param>
        /// <returns>Return a collection of menu items to add to the context menu</returns>
        public delegate ICollection<GlobalContextMenu.Entry> TreeNodeRightClickHandler(TreeNodeClickedEventArgs args);

        private static readonly List<KeyValuePair<MenuOrder, TreeNodeRightClickHandler>> _TreeNodeRightClickHandlers = new List<KeyValuePair<MenuOrder, TreeNodeRightClickHandler>>();
        private static bool _treeNodeRightClickHandlersDirty;


        /// <summary>
        /// Subscribe to workspace tree node right click events to add custom context menu items. Only one menu can be shown at a time.
        /// Warning: This is called for every tree node right click, so make sure your handler is fast and doesn't allocate memory unnecessarily. Consider caching the entries.
        /// </summary>
        /// <param name="priority">The order in which this handler is called relative to other handlers. Lower values are called first and appear higher in the menu.</param>
        /// <param name="handler">Callback function that returns a collection of menu entries to add to the context menu when a tree node is right clicked.</param>
        /// <returns>An <see cref="IDisposable"/> that can be used to unsubscribe from the event when disposed.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="handler"/> is null.</exception>
        public static IDisposable RegisterTreeNodeContextMenuItems(MenuOrder priority, TreeNodeRightClickHandler handler)
        {
            _TreeNodeRightClickHandlers.Add(new KeyValuePair<MenuOrder, TreeNodeRightClickHandler>(priority, handler ?? throw new ArgumentNullException(nameof(handler))));
            _treeNodeRightClickHandlersDirty = true;
            return Disposable.Create(() => _TreeNodeRightClickHandlers.RemoveAll(x => x.Value == handler));
        }

        private static void OnShowCustomContextMenu(TreeNodeObject clickedInstance)
        {
            if (!clickedInstance) return;

            var args = new TreeNodeClickedEventArgs(clickedInstance);
            // If the right clicked node is not in selected list, select it
            if (!args.SelectedInstances.Contains(clickedInstance))
            {
#if PH
                global::Studio.Studio.Instance.treeNodeCtrl.AddSelectNode(clickedInstance);
#else
                global::Studio.Studio.Instance.treeNodeCtrl.AddSelectNode(clickedInstance, false);
#endif
                args = new TreeNodeClickedEventArgs(clickedInstance);
            }

            if (args.SelectedInstances.Count == 0) return;

            var results = new List<GlobalContextMenu.Entry>();

            if (_treeNodeRightClickHandlersDirty)
            {
                _TreeNodeRightClickHandlers.Sort((x, y) => x.Key.CompareTo(y.Key));
                _treeNodeRightClickHandlersDirty = false;
            }

            foreach (var handler in _TreeNodeRightClickHandlers)
            {
                try
                {
                    var newEntries = handler.Value.Invoke(args);
                    if (newEntries != null && newEntries.Count > 0)
                    {
                        if (results.Count > 0)
                            results.Add(GlobalContextMenu.Entry.Separator);
                        results.AddRange(newEntries);
                    }
                }
                catch (Exception e)
                {
                    KoikatuAPI.Logger.LogError(e);
                }
            }

            var title = args.SelectedInstances.Count <= 1 ? clickedInstance.textName : $"{clickedInstance.textName} + {args.SelectedInstances.Count(x => x != clickedInstance)} selected";
            GlobalContextMenu.Show(title, results.ToArray());
        }

        #endregion

#if PH
        private static void SceneLoaded(UnityEngine.SceneManagement.Scene scene, LoadSceneMode loadSceneMode)
#else
        private static void SceneLoaded(Scene scene, LoadSceneMode loadSceneMode)
#endif
        {
            if (!StudioLoaded && scene.name == "Studio")
            {
                StudioLoaded = true;

                if (StudioLoadedChanged != null)
                {
                    foreach (var callback in StudioLoadedChanged.GetInvocationList())
                    {
                        try
                        {
                            ((EventHandler)callback)(KoikatuAPI.Instance, EventArgs.Empty);
                        }
                        catch (Exception e)
                        {
                            KoikatuAPI.Logger.LogError(e);
                        }
                    }
                }

                // Delay to let plugins not using the api create their toggles so we don't overlap
                KoikatuAPI.Instance.StartCoroutine(DelayedLoadCo());
                IEnumerator DelayedLoadCo()
                {
                    yield return null;
                    foreach (var cat in _customCurrentStateCategories)
                        CreateCategory(cat);

                    yield return null; // todo Only needed for compat with QAB. Can remove once QAB is using this API
                    ToolbarManager.OnStudioLoaded();
                }
            }
        }

        [Conditional("DEBUG")]
        private static void DebugControls()
        {
            var cat = GetOrCreateCurrentStateCategory("Control test category");
            cat.AddControl(
                    new CurrentStateCategoryToggle(
                        "Test 1", 2, c =>
                        {
#if PH
                            var charInfoHuman = c?.charInfo?.human;
                            KoikatuAPI.Logger.LogMessage((charInfoHuman == null ? "NULL" : charInfoHuman.GetCharacterName()) + " - updateValue");
#else
                            KoikatuAPI.Logger.LogMessage(c?.charInfo?.name + " - updateValue");
#endif
                            return 1;
                        }))
                .Value.Subscribe(val => KoikatuAPI.Logger.LogMessage(val));
            cat.AddControl(new CurrentStateCategoryToggle("Test 2", 3, c => 2)).Value.Subscribe(val => KoikatuAPI.Logger.LogMessage(val));
            cat.AddControl(new CurrentStateCategoryToggle("Test 3", 4, c => 3)).Value.Subscribe(val => KoikatuAPI.Logger.LogMessage(val));

            var cat2 = GetOrCreateCurrentStateCategory("Control test category");
            cat2.AddControl(new CurrentStateCategorySwitch("Test add", c => true)).Value.Subscribe(val => KoikatuAPI.Logger.LogMessage(val));
            cat2.AddControl(new CurrentStateCategorySlider("Test slider", c => 0.75f)).Value.Subscribe(val => KoikatuAPI.Logger.LogMessage(val));
            cat2.AddControl(new CurrentStateCategoryDropdown("dropdown test", new[] { "item 1", "i2", "test 3" }, c => 1)).Value.Subscribe(val => KoikatuAPI.Logger.LogMessage("dd " + val));
            cat2.AddControls(new CurrentStateCategoryColorPicker("Test Color Picker", c => Color.white, c => KoikatuAPI.Logger.LogMessage("color picker: " + c)));

            ToolbarManager.AddLeftToolbarControl(new SimpleToolbarButton("test", "test text", () => new Texture2D(32, 32), KoikatuAPI.Instance, btn => Console.WriteLine("Toolbar button click " + btn)));
            ToolbarManager.AddLeftToolbarControl(new SimpleToolbarToggle("test toggle", "test toggle text", () => new Texture2D(55, 32), true, KoikatuAPI.Instance, v => Console.WriteLine("Toolbar toggle change " + v)));
        }
    }
}
