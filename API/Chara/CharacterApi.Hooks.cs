using BepInEx.Logging;
using ChaCustom;
using Harmony;
using KKAPI.Maker;
#if KK
using System.Reflection;
using UnityEngine.UI;
#endif
using System;
using System.Collections;
using UnityEngine;

namespace KKAPI.Chara
{
    public static partial class CharacterApi
    {
        private static class Hooks
        {
            [HarmonyPostfix]
#if KK
            [HarmonyPatch(typeof(ChaControl), nameof(ChaControl.Initialize), new[]
            {
                typeof(byte),
                typeof(bool),
                typeof(GameObject),
                typeof(int),
                typeof(int),
                typeof(ChaFileControl)
            })]
#elif EC
            [HarmonyPatch(typeof(ChaControl), nameof(ChaControl.Initialize), typeof(byte), typeof(int), typeof(UnityEngine.GameObject), typeof(int), typeof(int), typeof(ChaFileControl))]
#endif
            public static void ChaControl_InitializePostHook(ChaControl __instance)
            {
                KoikatuAPI.Log(LogLevel.Debug, $"[KKAPI] Character card load: {GetLogName(__instance)} {(MakerAPI.CharaListIsLoading ? "inside CharaList" : string.Empty)}");

                ChaControls.Add(__instance);

                if (!MakerAPI.CharaListIsLoading)
                    CreateOrAddBehaviours(__instance);
            }

            [HarmonyPostfix]
            [HarmonyPatch(typeof(ChaControl), nameof(ChaControl.OnDestroy))]
            public static void ChaControl_OnDestroyHook(ChaControl __instance)
            {
                ChaControls.Remove(__instance);
            }

            /// <summary>
            /// Needed for saving in class maker, rest is handled by ExtendedSave.CardBeingSaved
            /// </summary>
            [HarmonyPrefix]
            [HarmonyPatch(typeof(CvsExit), nameof(CvsExit.ExitSceneRestoreStatus), new[]
            {
                typeof(string)
            })]
            public static void CvsExit_ExitSceneRestoreStatus(string strInput, CvsExit __instance)
            {
                OnCardBeingSaved(Singleton<CustomBase>.Instance.chaCtrl.chaFile);
            }

            /// <summary>
            /// Copy extended data when moving between class roster and main game data, and in free h
            /// (the character data gets transferred to predefined slots instead of creating new characters)
            /// </summary>
            [HarmonyPrefix]
            [HarmonyPatch(typeof(ChaFile), nameof(ChaFile.CopyChaFile), new[]
            {
                typeof(ChaFile),
                typeof(ChaFile)
            })]
            public static void ChaFile_CopyChaFileHook(ChaFile dst, ChaFile src)
            {
                foreach (var handler in _registeredHandlers)
                {
                    if (handler.ExtendedDataCopier == null)
                        continue;

                    try
                    {
                        handler.ExtendedDataCopier(dst, src);
                    }
                    catch (Exception e)
                    {
                        KoikatuAPI.Log(LogLevel.Error, e);
                    }
                }
            }

#if KK
            private static readonly FieldInfo IdolBackButton = typeof(LiveCharaSelectSprite).GetField("btnIdolBack", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

            /// <summary>
            /// Update changes when selecting live mode character
            /// </summary>
            [HarmonyPostfix]
            [HarmonyPatch(typeof(LiveCharaSelectSprite), "Start")]
            public static void LiveCharaSelectSprite_StartPostHook(LiveCharaSelectSprite __instance)
            {
                var button = IdolBackButton?.GetValue(__instance) as Button;
                button?.onClick.AddListener(() => __instance.StartCoroutine(DelayedReloadChara(__instance.heroine.chaCtrl)));
            }

            /// <summary>
            /// Force reload when going to next day in school
            /// It's needed because after 1st day since loading the characters are reset but not reloaded, and can cause issues
            /// </summary>
            [HarmonyPrefix]
            [HarmonyPatch(typeof(ActionScene), nameof(ActionScene.NPCLoadAll))]
            public static void ActionScene_NPCLoadAllPreHook(ActionScene __instance)
            {
                __instance.StartCoroutine(DelayedReloadChara(null));
            }
#endif

            /// <summary>
            /// Needed for some edge cases, replacing characters in scene maker in EC
            /// </summary>
            [HarmonyPostfix]
            [HarmonyPatch(typeof(ChaControl), nameof(ChaControl.ReloadAsync), new[] { typeof(bool), typeof(bool), typeof(bool), typeof(bool), typeof(bool) })]
            public static void ReloadAsyncPostHook(bool noChangeClothes, bool noChangeHead, bool noChangeHair, bool noChangeBody, ChaControl __instance, ref IEnumerator __result)
            {
                if (noChangeClothes || noChangeHead || noChangeHair || noChangeBody) return;
                if (IsCurrentlyReloading(__instance)) return;

                var original = __result;
                __result = new[] { original, DelayedReloadChara(__instance) }.GetEnumerator();
            }

            /// <summary>
            /// Prevents firing coordinate load events when the coordinate window is populating
            /// </summary>
            public static bool ClothesFileControlLoading;

            [HarmonyPrefix]
            [HarmonyPatch(typeof(clothesFileControl), "Initialize")]
            public static void clothesFileControl_InitializePreHook()
            {
                ClothesFileControlLoading = true;
            }

            [HarmonyPostfix]
            [HarmonyPatch(typeof(clothesFileControl), "Initialize")]
            public static void clothesFileControl_InitializePostHook()
            {
                ClothesFileControlLoading = false;
            }
        }
    }
}
