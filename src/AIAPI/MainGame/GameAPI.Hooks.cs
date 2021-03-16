using System;
using HarmonyLib;
using AIProject.SaveData;
using AIProject;
using AIProject.Scene;

namespace KKAPI.MainGame
{
    public static partial class GameAPI
    {
        private class Hooks
        {
            private static bool _isNewGame;
            private static int _day = 0;

            public static void SetupHooks()
            {
                Harmony.CreateAndPatchAll(typeof(Hooks));
            }

            [HarmonyPostfix]
            [HarmonyPatch(typeof(SaveData), nameof(SaveData.Load), typeof(string))]
            public static void LoadHook(string fileName)
            {
                OnGameBeingLoaded("", fileName);
            }

            [HarmonyPrefix]
            [HarmonyPatch(typeof(SaveData), nameof(SaveData.SaveFile), typeof(string))]
            public static void SaveHook(string path)
            {
                OnGameBeingSaved(path, "");
            }

            [HarmonyPrefix]
            [HarmonyPatch(typeof(TitleLoadScene), "SetWorldData", typeof(WorldData), typeof(bool))]
            public static void TitleLoadScene_SetWorldData(WorldData _worldData, bool isAuto)
            {
                _isNewGame = _worldData?.SaveTime == new DateTime(0);
            }

            [HarmonyPostfix]
            [HarmonyPatch(typeof(MapScene), "OnLoaded")]
            public static void MapScene_OnLoaded(MapScene __instance)
            {
                if (_isNewGame) OnNewGame();
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
                    OnDayChange(_day);
                    _day++;
                }

                OnPeriodChange(zone);//morning, day, evening
            }

        }
    }
}
