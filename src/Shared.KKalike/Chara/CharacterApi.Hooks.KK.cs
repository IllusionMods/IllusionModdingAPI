using ADV;
using ChaCustom;
using HarmonyLib;
using KKAPI.Maker;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace KKAPI.Chara
{
    public static partial class CharacterApi
    {
        private static class Hooks
        {
            public static void InitHooks()
            {
                var i = Harmony.CreateAndPatchAll(typeof(Hooks));

                // Fuzzy argument lengths are needed for darkness compatibility
                var target = typeof(ChaControl).GetMethods().Single(info => info.Name == nameof(ChaControl.Initialize) && info.GetParameters().Length >= 5);
                i.Patch(target, null, new HarmonyMethod(typeof(Hooks), nameof(ChaControl_InitializePostHook)));

                var target2 = typeof(ChaControl).GetMethods().Single(info => info.Name == nameof(ChaControl.ReloadAsync) && info.GetParameters().Length >= 5);
                i.Patch(target2, null, new HarmonyMethod(typeof(Hooks), nameof(ReloadAsyncPostHook)));

                var target3 = typeof(ChaFile).GetMethods().Single(info => info.Name == nameof(ChaFile.CopyAll));
                i.Patch(target3, null, new HarmonyMethod(typeof(Hooks), nameof(ChaFile_CopyChaFileHook)));

#if KK
                // Find the ADV character Start lambda to hook for extended data copying. The inner type names change between versions so try them all.
                var transpiler = new HarmonyMethod(typeof(Hooks), nameof(FixEventSceneLambdaTpl));
                var lambdaOuter = AccessTools.Inner(typeof(FixEventSceneEx), "<Start>c__AnonStorey1");
                foreach (var nestedType in lambdaOuter.GetNestedTypes(AccessTools.all))
                {
                    if (!nestedType.IsClass) continue;

                    var lambdaMethod = AccessTools.Method(nestedType, "<>m__0");
                    if (lambdaMethod != null)
                        i.Patch(lambdaMethod, null, null, transpiler);
                }
#endif
            }

            public static void ChaControl_InitializePostHook(ChaControl __instance)
            {
                KoikatuAPI.Logger.LogDebug($"Character card load: {GetLogName(__instance)} {(MakerAPI.CharaListIsLoading ? "inside CharaList" : string.Empty)}");

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
            public static void ChaFile_CopyChaFileHook(ChaFile __instance, ChaFile _chafile)
            {
                OnCopyChaFile(__instance, _chafile);
            }

            private static void OnCopyChaFile(ChaFile destination, ChaFile source)
            {
                foreach (var handler in _registeredHandlers)
                {
                    if (handler.Value.ExtendedDataCopier == null)
                        continue;

                    try
                    {
                        handler.Value.ExtendedDataCopier(destination, source);
                    }
                    catch (Exception e)
                    {
                        KoikatuAPI.Logger.LogError(e);
                    }
                }
            }

#if KK
            /// <summary>
            /// Fix extended data being lost in ADV by copying it over when chara data is copied
            /// </summary>
            public static IEnumerable<CodeInstruction> FixEventSceneLambdaTpl(MethodBase original, IEnumerable<CodeInstruction> instructions)
            {
                /* 1 is the source ChaFileControl to copy from, 2 is CharaData that has the destination ChaFile. Below code after which we hook in
                 60	00B8	ldloc.2
                 61	00B9	callvirt	instance class ChaFileControl SaveData/CharaData::get_charFile()
                 62	00BE	ldloc.1
                 63	00BF	ldfld	uint8[] ChaFile::pngData
                 64	00C4	stfld	uint8[] ChaFile::pngData
                 */

				var il = instructions.ToList();

                var target = AccessTools.Field(typeof(ChaFile), nameof(ChaFile.pngData));
                if (target == null) throw new ArgumentNullException(nameof(target));

                // This can get called on lambdas that are not the one we need to patch, only one of them uses Stfld on ChaFile.pngData
                var targetIndex = il.FindIndex(instruction => instruction.opcode == OpCodes.Stfld && instruction.operand == target);
                if (targetIndex > 0)
                {
                    il.InsertRange(
                        targetIndex + 1, new[]
                        {
                            // Target
                            new CodeInstruction(OpCodes.Ldloc_2),
                            new CodeInstruction(OpCodes.Callvirt, AccessTools.Property(typeof(SaveData.CharaData), nameof(SaveData.CharaData.charFile)).GetGetMethod()),
                            // Source
                            new CodeInstruction(OpCodes.Ldloc_1),
                            // Call the data copy
                            new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(Hooks), nameof(OnCopyChaFile))),
                        });

                    if (KoikatuAPI.EnableDebugLogging)
                        KoikatuAPI.Logger.LogDebug("FixEventSceneLambdaTpl success on " + original.FullDescription());
                }

                return il;
            }

            /// <summary>
            /// Needed to update controllers after ADV scene finishes loading since characters get loaded async so other events fire too early
            /// </summary>
            [HarmonyPostfix]
            [HarmonyPatch(typeof(CharaData), nameof(CharaData.Initialize))]
            public static void CharaData_InitializePost(CharaData __instance)
            {
                // Only run reload if the character was newly created
                if (Traverse.Create(__instance).Property("isADVCreateChara").GetValue<bool>())
                    ReloadChara(__instance.chaCtrl);
            }

            /// <summary>
            /// Update controllers after selecting live mode character
            /// </summary>
            [HarmonyPostfix]
            [HarmonyPatch(typeof(LiveCharaSelectSprite), "Start")]
            public static void LiveCharaSelectSprite_StartPostHook(LiveCharaSelectSprite __instance)
            {
                var button = (UnityEngine.UI.Button)Traverse.Create(__instance).Field("btnIdolBack").GetValue();
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
