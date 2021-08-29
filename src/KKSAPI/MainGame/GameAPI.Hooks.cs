using System;
using System.Collections;
using ActionGame;
using HarmonyLib;
using UnityEngine;

namespace KKAPI.MainGame
{
    public static partial class GameAPI
    {
        private class Hooks
        {
            public static void SetupHooks(Harmony hi)
            {
                hi.PatchAll(typeof(Hooks));
                //Patch the VR version of these methods via reflection since they don't exist in normal assembly
                var vrHSceneType = Type.GetType("VRHScene, Assembly-CSharp");
                if (vrHSceneType != null)
                {
                    hi.Patch(AccessTools.Method(vrHSceneType, "Start"), postfix: new HarmonyMethod(AccessTools.Method(typeof(Hooks), nameof(Hooks.StartProcPost))));
                    hi.Patch(AccessTools.Method(vrHSceneType, "EndProc"), postfix: new HarmonyMethod(AccessTools.Method(typeof(Hooks), nameof(Hooks.EndProcPost))));
                    hi.Patch(AccessTools.Method(vrHSceneType, "OnBack"), postfix: new HarmonyMethod(AccessTools.Method(typeof(Hooks), nameof(Hooks.EndProcPost))));
                }
            }

            [HarmonyPostfix]
            [HarmonyPatch(typeof(SaveData.WorldData), nameof(SaveData.WorldData.Load), new[] { typeof(string), typeof(string) })] //todo change to use get/setbytes methods instead?
            public static void LoadHook(string path, string fileName)
            {
                OnGameBeingLoaded(path, fileName);
            }

            [HarmonyPrefix]
            [HarmonyPatch(typeof(SaveData.WorldData), nameof(SaveData.WorldData.Save), new[] { typeof(string), typeof(string) })]
            public static void SaveHook(string path, string fileName)
            {
                OnGameBeingSaved(path, fileName);
            }

            [HarmonyPostfix]
            [HarmonyPatch(typeof(HSceneProc), "Start")]
            public static void StartProcPost(MonoBehaviour __instance, ref IEnumerator __result)
            {
                var oldResult = __result;
                __result = new[] { oldResult, OnHStart(__instance) }.GetEnumerator();
            }

            [HarmonyPostfix]
            [HarmonyPatch(typeof(HSceneProc), "NewHeroineEndProc")]
            [HarmonyPatch(typeof(HSceneProc), "EndProc")]
            public static void EndProcPost(MonoBehaviour __instance)
            {
                OnHEnd(__instance);
            }

            [HarmonyPostfix]
            [HarmonyPatch(typeof(Cycle), nameof(Cycle.Change), new Type[] { typeof(Cycle.Type) })]
            public static void CycleChangeTypeHook(Cycle.Type type)
            {
                OnPeriodChange(type);
            }

            [HarmonyPostfix]
            [HarmonyPatch(typeof(Cycle), nameof(Cycle.Change), new Type[] { typeof(Cycle.Week) })]
            public static void CycleChangeWeekHook(Cycle.Week week)
            {
                OnDayChange(week);
            }
        }
    }
}
