using System;
using System.Collections;
using ActionGame;
using Harmony;

namespace KKAPI.MainGame
{
    public static partial class GameAPI
    {
        private class Hooks
        {
            public static void SetupHooks()
            {
                HarmonyInstance.Create(typeof(Hooks).FullName).PatchAll(typeof(Hooks));
            }

            [HarmonyPostfix]
            [HarmonyPatch(typeof(SaveData), nameof(SaveData.Load), new[] { typeof(string), typeof(string) })]
            public static void LoadHook(string path, string fileName)
            {
                OnGameBeingLoaded(path, fileName);
            }

            [HarmonyPrefix]
            [HarmonyPatch(typeof(SaveData), nameof(SaveData.Save), new[] { typeof(string), typeof(string) })]
            public static void SaveHook(string path, string fileName)
            {
                OnGameBeingSaved(path, fileName);
            }

            [HarmonyPostfix]
            [HarmonyPatch(typeof(HSceneProc), "Start")]
            public static void StartProcPost(HSceneProc __instance, ref IEnumerator __result)
            {
                var oldResult = __result;
                __result = new[] { oldResult, OnHStart(__instance) }.GetEnumerator();
            }

            [HarmonyPostfix]
            [HarmonyPatch(typeof(HSceneProc), "NewHeroineEndProc")]
            public static void NewHeroineEndProcPost(HSceneProc __instance)
            {
                OnHEnd(__instance);
            }

            [HarmonyPostfix]
            [HarmonyPatch(typeof(HSceneProc), "EndProc")]
            public static void EndProcPost(HSceneProc __instance)
            {
                OnHEnd(__instance);
            }

            [HarmonyPostfix]
            [HarmonyPatch(typeof(Cycle), nameof(Cycle.Change), new Type[]{typeof(Cycle.Type)})]
            public static void CycleChangeTypeHook(Cycle.Type type)
            {
                OnPeriodChange(type);
            }

            [HarmonyPostfix]
            [HarmonyPatch(typeof(Cycle), nameof(Cycle.Change), new Type[]{typeof(Cycle.Week) })]
            public static void CycleChangeWeekHook(Cycle.Week week)
            {
                OnDayChange(week);
            }
        }
    }
}
