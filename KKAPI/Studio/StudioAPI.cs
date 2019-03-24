using System.Collections.Generic;
using System.Diagnostics;
using BepInEx.Logging;
using KKAPI.Studio.UI;
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
        private static bool _studioLoaded;

        /// <summary>
        /// Add a new custom category to the Anim > CurrentState tab in the studio top-left menu.
        /// Can use this at any point.
        /// </summary>
        public static void CreateCurrentStateCategory(CurrentStateCategory category)
        {
            if (!InsideStudio)
            {
                Logger.Log(LogLevel.Warning, "[StudioAPI] Tried to run CreateCurrentStateCategory outside of studio!");
                return;
            }

            if (_studioLoaded)
                CreateCategory(category);

            _customCurrentStateCategories.Add(category);
        }

        private static void CreateCategory(CurrentStateCategory category)
        {
            category.CreateCategory(GameObject.Find("StudioScene/Canvas Main Menu/02_Manipulate/00_Chara/01_State/Viewport/Content"));
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

        private static void SceneLoaded(Scene arg0, LoadSceneMode arg1)
        {
            if (!_studioLoaded && arg0.name == "Studio")
            {
                _studioLoaded = true;
                foreach (var cat in _customCurrentStateCategories)
                    CreateCategory(cat);
            }
        }

        [Conditional("DEBUG")]
        private static void DebugControls()
        {
            CreateCurrentStateCategory(new CurrentStateCategory("Control test category",
                new[]
                {
                    new CurrentStateCategoryToggle("Test 1", 2, c =>
                    {
                        Logger.Log(LogLevel.Message, c?.charInfo?.name + " 1");
                        return 1;
                    }),
                    new CurrentStateCategoryToggle("Test 2", 3, c =>
                    {
                        Logger.Log(LogLevel.Message, c?.charInfo?.name + " 2");
                        return 2;
                    }),
                    new CurrentStateCategoryToggle("Test 3", 4, c =>
                    {
                        Logger.Log(LogLevel.Message, c?.charInfo?.name + " 3");
                        return 3;
                    })
                }));
        }
    }
}
