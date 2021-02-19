using System;
using System.Collections;
using HarmonyLib;
using AIProject.SaveData;
using AIProject;

namespace KKAPI.MainGame
{
    public static partial class GameAPI
    {
        private class Hooks
        {
            public static int lastCurrentDay = 1;//Always starts at 1

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
            [HarmonyPatch(typeof(HScene), "InitCoroutine")]
            public static void HScene_InitCoroutine(HScene __instance)
            {
                OnHStart(__instance);
            }      

            [HarmonyPostfix]
            [HarmonyPatch(typeof(HScene), "EndProc")]
            public static void HScene_EndProc(HScene __instance)
            {
                OnHEnd(__instance);
            }

            [HarmonyPostfix]
            [HarmonyPatch(typeof(EnvironmentSimulator), nameof(EnvironmentSimulator.SetTimeZone), typeof(AIProject.TimeZone))]
            public static void EnvironmentChangeTypeHook(AIProject.TimeZone zone)
            {
                OnPeriodChange(zone);//morning, day, evening
            }

            [HarmonyPostfix]
            [HarmonyPatch(typeof(EnviroSky), "SetGameTime")]
            public static void EnvironmentChangeDayHook(EnviroSky __instance)
            {
                var currentDay = (int)__instance.currentDay;
                if (lastCurrentDay < currentDay)
                {
                    lastCurrentDay = currentDay;
                    OnDayChange(currentDay);
                }                
            }
        }
    }
}
