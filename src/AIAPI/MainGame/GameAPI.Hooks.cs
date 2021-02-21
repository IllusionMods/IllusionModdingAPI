using System;
using System.Collections;
using HarmonyLib;
using AIProject.SaveData;
using AIProject;
using Sirenix.Serialization;
using AIProject.Scene;
using Manager;

namespace KKAPI.MainGame
{
    public static partial class GameAPI
    {
        private class Hooks
        {
            public static int lastCurrentDay = 1;//Always starts at 1
            public static bool isNewGame = false;

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

            [HarmonyPrefix]
            [HarmonyPatch(typeof(TitleLoadScene), "SetWorldData", typeof(WorldData), typeof(bool))]
            public static void TitleLoadScene_SetWorldData(WorldData _worldData, bool isAuto)
            {
                isNewGame = _worldData?.SaveTime.Millisecond == 0;
            } 

            [HarmonyPostfix]
            [HarmonyPatch(typeof(MapScene), "OnLoaded")]
            public static void MapScene_OnLoaded(MapScene __instance)
            {
                if (isNewGame) OnNewGame();
            } 

            [HarmonyPostfix]
            [HarmonyPatch(typeof(HScene), "SetStartVoice")]
            public static void HScene_SetStartVoice(HScene __instance)
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
