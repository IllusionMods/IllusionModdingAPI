using KKAPI.Chara;
using KKAPI.Studio.SaveLoad;
using KKAPI.Studio.UI;
using Studio;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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
            if (!StudioLoaded)
                yield break;

            TreeNodeObject[] selectNodes = Singleton<global::Studio.Studio>.Instance.treeNodeCtrl.selectNodes;
            for (int i = 0; i < selectNodes.Length; i++)
                if (global::Studio.Studio.Instance.dicInfo.TryGetValue(selectNodes[i], out ObjectCtrlInfo objectCtrlInfo))
                    yield return objectCtrlInfo;
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
                    CustomToolbarButtons.OnStudioLoaded();
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

            CustomToolbarButtons.AddLeftToolbarButton(new Texture2D(32, 32), () => Console.WriteLine("click"));
            CustomToolbarButtons.AddLeftToolbarToggle(new Texture2D(55, 32), true, v => Console.WriteLine("click " + v));
        }
    }
}
