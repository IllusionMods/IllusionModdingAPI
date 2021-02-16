using System;
using System.Collections;
// using ActionGame;
using HarmonyLib;
using AIProject.SaveData;
using Manager;
using AIProject;
using Sirenix.Serialization;

namespace KKAPI.MainGame
{
    public static partial class GameAPI
    {
        private class Hooks
        {
            public static int currentDay = 1;//Always starts at 1

            public static void SetupHooks()
            {
                Harmony.CreateAndPatchAll(typeof(Hooks));
            }

            //TODO SaveData.Load only has 1 param in AI and it's the path, does not include filename like KK
            [HarmonyPostfix]
            [HarmonyPatch(typeof(SaveData), nameof(SaveData.Load), new[] { typeof(string) })]
            public static void LoadHook(string fileName)
            {
                OnGameBeingLoaded("", fileName);
            }

            [HarmonyPrefix]
            [HarmonyPatch(typeof(SaveData), nameof(SaveData.SaveFile), new[] { typeof(string) })]
            public static void SaveHook(string path)
            {
                OnGameBeingSaved(path, "");
            }

            [HarmonyPostfix]
            [HarmonyPatch(typeof(HSceneManager), "HsceneInit", typeof(AgentActor[]))]
            public static void StartProcPostAgent(HSceneManager __instance)
            {
                OnHStart(__instance);
            }
            
            [HarmonyPostfix]
            [HarmonyPatch(typeof(HSceneManager), "HsceneInit", typeof(MerchantActor), typeof(AgentActor))]
            public static void StartProcPostMerchant(HSceneManager __instance)
            {
                OnHStart(__instance);
            }

            // [HarmonyPostfix]
            // [HarmonyPatch(typeof(HSceneManager), "NewHeroineEndProc")]
            // public static void NewHeroineEndProcPost(HSceneManager __instance)
            // {
            //     OnHEnd(__instance);
            // }

            [HarmonyPostfix]
            [HarmonyPatch(typeof(HSceneManager), nameof(HSceneManager.EndHScene))]
            public static void EndProcPost(HSceneManager __instance)
            {
                OnHEnd(__instance);
            }

            [HarmonyPostfix]
            [HarmonyPatch(typeof(EnvironmentSimulator), nameof(EnvironmentSimulator.SetTimeZone), typeof(AIProject.TimeZone))]
            public static void CycleChangeTypeHook(AIProject.TimeZone zone)
            {
                OnPeriodChange(zone);
            }

            [HarmonyPostfix]
            [HarmonyPatch(typeof(EnviroSky), "SetGameTime")]
            public static void CycleChangeDayHook(EnviroSky __instance)
            {
                var curDay = (int)__instance.currentDay;
                if (currentDay != curDay)
                {
                    currentDay = curDay;
                    OnDayChange(curDay);
                }                
            }
        }
    }
}
