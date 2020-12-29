using System;
using System.Collections.Generic;
using System.IO;
using BepInEx;
using HarmonyLib;
using KKAPI.Maker;

namespace KKAPI.Chara
{
    public static partial class CharacterApi
    {
        private static class Hooks
        {
            public static void InitHooks()
            {
                var i = Harmony.CreateAndPatchAll(typeof(Hooks));
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

            [HarmonyPostfix]
            [HarmonyPatch(typeof(Human), nameof(Human.LoadCoordinate), typeof(BinaryReader), typeof(bool), typeof(bool), typeof(int))]
            public static void ChaControl_ApplyPostHook(Human __instance)
            {
                OnCoordinateBeingLoaded(__instance, __instance.customParam);
            }

            [HarmonyPrefix]
            [HarmonyPatch(typeof(EditScene), "RecordCustomData")]
            public static void RecordCustomDataHook(Human ___human)
            {
                OnCardBeingSaved(___human.customParam);
            }

            [HarmonyPrefix]
            [HarmonyPatch(typeof(EditMode), "RecordCustomData")]
            public static void RecordCustomDataHook2(Human ___human)
            {
                OnCardBeingSaved(___human.customParam);
            }

            internal static readonly Dictionary<Human, string> LastLoadedCardPaths = new Dictionary<Human, string>();

            [HarmonyPrefix]
            [HarmonyPatch(typeof(Human), "Load", typeof(string), typeof(bool), typeof(bool), typeof(int))]
            public static void RecordCustomDataHook2(Human __instance, string file)
            {
                if (!Path.IsPathRooted(file))
                {
                    var fullPath = Path.Combine(Paths.GameRootPath, file);
                    if (File.Exists(fullPath))
                    {
                        file = fullPath;
                    }
                    else
                    {
                        fullPath = Path.GetFullPath(file);
                        if (File.Exists(fullPath))
                            file = fullPath;
                    }
                }

                Console.WriteLine("Card loading from " + file);

                LastLoadedCardPaths[__instance] = file;
            }
        }
    }
}
