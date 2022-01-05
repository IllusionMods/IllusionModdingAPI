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

            // Need to patch at this late point because GameCustomFunctionController.GetExtendedData uses Manager.Game.Instance.saveData to get ext data
            // info, and this method is where Manager.Game.Instance.saveData is set. If any methods inside WorldData are hooked instead, the controller
            // will pull ext data from the previously loaded WorldData because the new WorldData didn't get set to this property yet.
            // This is not a problem in KK because in KK there is only one SaveData instance, and the Save/Load methods are instance methods not static methods.
            [HarmonyPostfix]
            [HarmonyPatch(typeof(Manager.Game), nameof(Manager.Game.Load), new[] { typeof(string) })]
            public static void LoadHook(string fileName)
            {
                OnGameBeingLoaded(SaveData.WorldData.Path + "/", fileName);
            }

            [HarmonyPrefix]
            [HarmonyPatch(typeof(SaveData.WorldData), nameof(SaveData.WorldData.Save), new[] { typeof(string), typeof(string) })]
            public static void SaveHook(string path, string fileName)
            {
                GameBeingSaved = true;
                OnGameBeingSaved(path, fileName);
            }

            [HarmonyFinalizer]
            [HarmonyPatch(typeof(SaveData.WorldData), nameof(SaveData.WorldData.Save), new[] { typeof(string), typeof(string) })]
            public static void SaveHookPost()
            {
                GameBeingSaved = false;
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
