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
            public static bool isNewGame = false;
            public static int day = 0;

            public static void SetupHooks()
            {
                Harmony.CreateAndPatchAll(typeof(Hooks));
            }

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
                isNewGame = _worldData?.SaveTime == new DateTime(0);
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
                if (zone == AIProject.TimeZone.Morning)
                {         
                    OnDayChange(day);
                    day++;
                }

                OnPeriodChange(zone);//morning, day, evening
            }

        }
    }
}
