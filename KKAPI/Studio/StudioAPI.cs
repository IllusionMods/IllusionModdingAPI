using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using BepInEx.Logging;
using KKAPI.Studio.UI;
using UniRx;
using UnityEngine;
using UnityEngine.SceneManagement;
using Logger = BepInEx.Logger;

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
        [Obsolete]
        public static void CreateCurrentStateCategory(CurrentStateCategory category)
        {
            if (category == null) throw new ArgumentNullException(nameof(category));

            if (!InsideStudio)
            {
                Logger.Log(LogLevel.Debug, "[StudioAPI] Tried to run CreateCurrentStateCategory outside of studio!");
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
                Logger.Log(LogLevel.Debug, "[StudioAPI] Tried to run CreateCurrentStateCategory outside of studio!");
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

        private static void SceneLoaded(Scene scene, LoadSceneMode loadSceneMode)
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
                            Logger.Log(LogLevel.Error, e);
                        }
                    }
                }

                foreach (var cat in _customCurrentStateCategories)
                    CreateCategory(cat);
            }
        }

        [Conditional("DEBUG")]
        private static void DebugControls()
        {
            var cat = GetOrCreateCurrentStateCategory("Control test category");
            cat.AddControl(new CurrentStateCategoryToggle("Test 1", 2, c => { Logger.Log(LogLevel.Message, c?.charInfo?.name + " - updateValue"); return 1; }))
                .Value.Subscribe(val => Logger.Log(LogLevel.Message, val));
            cat.AddControl(new CurrentStateCategoryToggle("Test 2", 3, c => 2)).Value.Subscribe(val => Logger.Log(LogLevel.Message, val));
            cat.AddControl(new CurrentStateCategoryToggle("Test 3", 4, c => 3)).Value.Subscribe(val => Logger.Log(LogLevel.Message, val));

            var cat2 = GetOrCreateCurrentStateCategory("Control test category");
            cat2.AddControl(new CurrentStateCategorySwitch("Test add", c => true)).Value.Subscribe(val => Logger.Log(LogLevel.Message, val));
            cat2.AddControl(new CurrentStateCategorySlider("Test slider", c => 0.75f)).Value.Subscribe(val => Logger.Log(LogLevel.Message, val));
        }
    }
}
