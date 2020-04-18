using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using KKAPI.Maker;
using Studio;

namespace KKAPI.Chara
{
    public static partial class CharacterApi
    {
        private static class Hooks
        {
            public static void InitHooks()
            {
                var i = BepInEx.Harmony.HarmonyWrapper.PatchAll(typeof(Hooks));
            }

            [HarmonyPostfix]
            [HarmonyPatch(typeof(Female), "Awake")]
            public static void ChaControl_InitializePostHook(Female __instance)
            {
                KoikatuAPI.Logger.LogDebug($"Character card load: {__instance.HeroineID}");

                ChaControls.Add(__instance);

                if (!MakerAPI.CharaListIsLoading)
                    CreateOrAddBehaviours(__instance);
            }

            [HarmonyPostfix]
            [HarmonyPatch(typeof(Male), "Awake")]
            public static void ChaControl_InitializePostHook(Male __instance)
            {
                KoikatuAPI.Logger.LogDebug($"Character card load: {__instance.MaleID}");

                ChaControls.Add(__instance);

                if (!MakerAPI.CharaListIsLoading)
                    CreateOrAddBehaviours(__instance);
            }

            [HarmonyPostfix]
            [HarmonyPatch(typeof(Female), nameof(Female.Apply))]
            public static void ChaControl_ApplyPostHook(Female __instance)
            {
                ReloadChara(__instance);
            }

            [HarmonyPostfix]
            [HarmonyPatch(typeof(Male), nameof(Male.Apply))]
            public static void ChaControl_ApplyPostHook(Male __instance)
            {
                ReloadChara(__instance);
            }

            //protected LOAD_MSG LoadCoordinate(BinaryReader reader, bool female, bool male, int filter = -1)
            [HarmonyPostfix]
            [HarmonyPatch(typeof(Human), nameof(Human.LoadCoordinate), typeof(BinaryReader), typeof(bool), typeof(bool), typeof(int))]
            public static void ChaControl_ApplyPostHook(Human __instance)
            {
                OnCoordinateBeingLoaded(__instance, __instance.customParam);
            }
        }
    }
}
