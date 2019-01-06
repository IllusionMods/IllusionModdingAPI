using System.Collections;
using System.Reflection;
using ChaCustom;
using Harmony;
using Studio;
using UnityEngine;
using UnityEngine.UI;
// ReSharper disable MemberCanBePrivate.Global

namespace MakerAPI
{


    internal static partial class CharacterApi
    {
        private static class Hooks
        {
            public static void InstallHook()
            {
                HarmonyInstance.Create(typeof(Hooks).FullName).PatchAll(typeof(Hooks));
            }

            // Prevent reading ABM data when loading the list of characters
            private static bool MakerListIsLoading => MakerAPI.Instance?.CharaListIsLoading ?? false;

            [HarmonyPostfix]
            [HarmonyPatch(typeof(ChaControl), nameof(ChaControl.Initialize), new[]
            {
                typeof(byte),
                typeof(bool),
                typeof(GameObject),
                typeof(int),
                typeof(int),
                typeof(ChaFileControl)
            })]
            public static void ChaControl_InitializePostHook(byte _sex, bool _hiPoly, GameObject _objRoot, int _id, int _no,
                ChaFileControl _chaFile, ChaControl __instance)
            {
                if (!MakerListIsLoading)
                {
                    CreateOrAddBehaviours(__instance);

                    ChaControls.Add(__instance);
                }
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
            /// Copy extended data when moving between class roster and main game data
            /// (the character data gets transferred to predefined slots instead of creating new characters)
            /// </summary>
            [HarmonyPrefix]
            [HarmonyPatch(typeof(ChaFile), nameof(ChaFile.CopyChaFile), new[]
            {
                typeof(ChaFile),
                typeof(ChaFile)
            })]
            public static void ChaFile_CopyChaFilePostHook(ChaFile dst, ChaFile src)
            {
                if (dst is ChaFileControl dstCfc && src is ChaFileControl srcCfc)
                {
                    foreach (var behaviour in GetBehaviours(FileControlToChaControl(srcCfc)))
                    {
                        behaviour.OnCopyExtendedData(dstCfc);
                    }
                }
            }

            /// <summary>
            /// Needed for studio
            /// </summary>
            [HarmonyPostfix]
            [HarmonyPatch(typeof(OCIChar), nameof(OCIChar.ChangeChara), new[]
            {
                typeof(string)
            })]
            public static void OCIChar_ChangeCharaPostHook(string _path, OCIChar __instance)
            {
                var component = __instance.charInfo;
                if (component != null)
                    component.StartCoroutine(DelayedLoad(component));
            }

            private static IEnumerator DelayedLoad(ChaControl controller)
            {
                yield return null;
                ReloadChara(controller);
            }

            private static readonly FieldInfo IdolBackButton = typeof(LiveCharaSelectSprite).GetField("btnIdolBack", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

            /// <summary>
            /// Update changes when selecting live mode character
            /// </summary>
            [HarmonyPostfix]
            [HarmonyPatch(typeof(LiveCharaSelectSprite), "Start")]
            public static void LiveCharaSelectSprite_StartPostHook(LiveCharaSelectSprite __instance)
            {
                var button = IdolBackButton?.GetValue(__instance) as Button;
                button?.onClick.AddListener(
                    () =>
                    {
                        __instance.StartCoroutine(DelayedLoad(__instance.heroine.chaCtrl));
                    });
            }

            /// <summary>
            /// Force reload when going to next day in school
            /// It's needed because after 1st day since loading the characters are reset but not reloaded, and can cause issues
            /// </summary>
            [HarmonyPrefix]
            [HarmonyPatch(typeof(ActionScene), nameof(ActionScene.NPCLoadAll))]
            public static void ActionScene_NPCLoadAllPreHook(ActionScene __instance)
            {
                __instance.StartCoroutine(DelayedLoad(null));
            }
        }
    }

}
