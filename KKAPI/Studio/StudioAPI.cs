using System.Collections.Generic;
using System.Diagnostics;
using BepInEx.Logging;
using Harmony;
using KKAPI.Studio.UI;
using Studio;
using UnityEngine;
using UnityEngine.SceneManagement;
using Logger = BepInEx.Logger;

namespace KKAPI.Studio
{
    public static class StudioAPI
    {
        private static readonly List<CurrentStateCategory> CustomCurrentStateCategories = new List<CurrentStateCategory>();
        private static bool _studioLoaded;

        public static void CreateCurrentStateCategory(CurrentStateCategory category)
        {
            if (!InsideStudio)
            {
                Logger.Log(LogLevel.Warning, "[StudioAPI] Tried to run CreateCurrentStateCategory outside of studio!");
                return;
            }

            if (_studioLoaded)
                CreateCategory(category);

            CustomCurrentStateCategories.Add(category);
        }

        private static void CreateCategory(CurrentStateCategory category)
        {
            category.CreateCategory(GameObject.Find("StudioScene/Canvas Main Menu/02_Manipulate/00_Chara/01_State/Viewport/Content"));
        }

        internal static void Init(bool insideStudio)
        {
            InsideStudio = insideStudio;

            if (!insideStudio) return;

            HarmonyInstance.Create(typeof(Hooks).FullName).PatchAll(typeof(Hooks));

            SceneManager.sceneLoaded += SceneLoaded;

            DebugControls();
        }
        
        public static bool InsideStudio { get; private set; }

        private static void SceneLoaded(Scene arg0, LoadSceneMode arg1)
        {
            if (!_studioLoaded && arg0.name == "Studio")
            {
                _studioLoaded = true;
                foreach (var cat in CustomCurrentStateCategories)
                    CreateCategory(cat);
            }
        }

        private static class Hooks
        {
            [HarmonyPostfix]
            [HarmonyPatch(typeof(MPCharCtrl), nameof(MPCharCtrl.OnClickRoot), new[] { typeof(int) })]
            public static void OnClickRoot(MPCharCtrl __instance, int _idx)
            {
                if (_idx == 0)
                {
                    foreach (var stateCategory in CustomCurrentStateCategories)
                        stateCategory.UpdateInfo(__instance.ociChar);
                }
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
