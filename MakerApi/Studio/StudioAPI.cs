using System.Collections.Generic;
using BepInEx.Logging;
using Harmony;
using Studio;
using UnityEngine;
using UnityEngine.SceneManagement;
using Logger = BepInEx.Logger;

namespace MakerAPI.Studio
{
    public static class StudioAPI
    {
        private static readonly List<CurrentStateCategory> CustomCurrentStateCategories = new List<CurrentStateCategory>();
        private static bool _studioLoaded;

        public static void CreateCurrentStateCategory(CurrentStateCategory category)
        {
            if (!MakerAPI.Instance.InsideStudio)
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

        internal static void Init()
        {
            var harmony = HarmonyInstance.Create($"{MakerAPI.GUID}_{nameof(StudioAPI)}");
            harmony.PatchAll(typeof(Hooks));

            SceneManager.sceneLoaded += SceneLoaded;

            /*CreateCurrentStateCategory(new CurrentStateCategory("Additional skin effects",
                new CurrentStateCategoryToggle[]
                {
                    new CurrentStateCategoryToggle("Sweat", 3, c =>
                    {
                        Logger.Log(LogLevel.Message, c?.charInfo?.name + " 1");
                        return 2;
                    }),
                    new CurrentStateCategoryToggle("Bukkake", 4, c =>
                    {
                        Logger.Log(LogLevel.Message, c?.charInfo?.name + " 2");
                        return 1;
                    }),
                    new CurrentStateCategoryToggle("Virgin blood", 4, c =>
                    {
                        Logger.Log(LogLevel.Message, c?.charInfo?.name + " 2");
                        return 1;
                    })

                }));*/
        }

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
    }
}
