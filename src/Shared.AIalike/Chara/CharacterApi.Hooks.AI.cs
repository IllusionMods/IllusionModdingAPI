using ADV;
using AIChara;
using ExtensibleSaveFormat;
using HarmonyLib;
using KKAPI.Maker;
using System;
using System.Collections;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace KKAPI.Chara
{
    public static partial class CharacterApi
    {
        private static class Hooks
        {
            public static void InitHooks()
            {
                var i = Harmony.CreateAndPatchAll(typeof(Hooks));

                var target = typeof(ChaControl).GetMethods().Single(info => info.Name == nameof(ChaControl.Initialize) && info.GetParameters().Length >= 5);
                i.Patch(target, null, new HarmonyMethod(typeof(Hooks), nameof(ChaControl_InitializePostHook)));

                var target2 = typeof(ChaControl).GetMethods().Single(info => info.Name == nameof(ChaControl.ReloadAsync) && info.GetParameters().Length >= 5);
                i.Patch(target2, null, new HarmonyMethod(typeof(Hooks), nameof(ReloadAsyncPostHook)));

                var target3 = typeof(ChaFile).GetMethods().Single(info => info.Name == nameof(ChaFile.CopyAll));
                i.Patch(target3, null, new HarmonyMethod(typeof(Hooks), nameof(ChaFile_CopyChaFileHook)));

                i.Patch(original: AccessTools.FirstMethod(typeof(ChaFile), info => info.Name == nameof(ChaFile.LoadFile) && info.GetParameters().FirstOrDefault()?.ParameterType == typeof(BinaryReader)),
                        prefix: new HarmonyMethod(typeof(Hooks), nameof(Hooks.ChaFileLoadHook)) { wrapTryCatch = true }); //todo ai hs2
            }

            private static void ChaFileLoadHook(ChaFile __instance, BinaryReader br)
            {
                // Keep track of what filenames cards get loaded from
                // Doesn't handle studio scenes and files loaded from memory but it doesn't matter here
                if (br.BaseStream is FileStream fs)
                {
                    // .Name should already be the full path, but it usually has a bunch of ../ in it, GetFullPath will clean it up
                    var fullPath = Path.GetFullPath(fs.Name);
                    CharacterExtensions.ChaFileFullPathLookup[__instance] = fullPath;
#if DEBUG
                    KoikatuAPI.Logger.LogDebug($"FullName for {__instance} is {fullPath}");
#endif
                }
#if DEBUG
                else
                    KoikatuAPI.Logger.LogWarning($"Failed to get FullName for {__instance}, BaseStream is {br.BaseStream} in {new System.Diagnostics.StackTrace()}");
#endif
            }

            public static void ChaControl_InitializePostHook(ChaControl __instance)
            {
                KoikatuAPI.Logger.LogDebug($"Character card load: {GetLogName(__instance?.chaFile)} {(MakerAPI.CharaListIsLoading ? "inside CharaList" : string.Empty)}");

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

            /// <summary>
            /// Copy extended data when current coordinate is about to be swapped. Should be a prefix to make sure it completes before any events are fired.
            /// Fixes losing ext data when character takes a bath, possibly in more instances.
            /// </summary>
            [HarmonyPrefix]
            // ChangeNowCoordinate(ChaFileCoordinate srcCoorde, bool reload = false, bool forceChange = true)
            [HarmonyPatch(typeof(ChaControl), nameof(ChaControl.ChangeNowCoordinate), typeof(ChaFileCoordinate), typeof(bool), typeof(bool))]
            public static void ChaControl_ChangeNowCoordinatePreHook(ChaControl __instance, ChaFileCoordinate srcCoorde)
            {
                CopyCoordExtData(srcCoorde, __instance.nowCoordinate);
            }

            [HarmonyPrefix]
            //public bool AssignCoordinate(ChaFileCoordinate srcCoorde)
            [HarmonyPatch(typeof(ChaControl), nameof(ChaControl.AssignCoordinate), typeof(ChaFileCoordinate))]
            public static void ChaControl_AssignCoordinateHook(ChaControl __instance, ChaFileCoordinate srcCoorde)
            {
                CopyCoordExtData(srcCoorde, __instance.chaFile.coordinate);//todo still issue with the reload button
            }

            [HarmonyPrefix]
            //public bool AssignCoordinate()
            [HarmonyPatch(typeof(ChaControl), nameof(ChaControl.AssignCoordinate), new Type[0], new ArgumentType[0])]
            public static void ChaControl_AssignCoordinateHook2(ChaControl __instance)
            {
                CopyCoordExtData(__instance.nowCoordinate, __instance.chaFile.coordinate);
            }

            [HarmonyPrefix]
            //CopyCoordinate(ChaFileCoordinate _coordinate)
            [HarmonyPatch(typeof(ChaFile), nameof(ChaFile.CopyCoordinate), typeof(ChaFileCoordinate))]
            public static void ChaFile_CopyCoordinateHook(ChaFile __instance, ChaFileCoordinate _coordinate)
            {
                CopyCoordExtData(_coordinate, __instance.coordinate);
            }

            private static void CopyCoordExtData(ChaFileCoordinate fromCoord, ChaFileCoordinate toCoord)
            {
                // Clear old ext data
                var oldData = ExtendedSave.GetAllExtendedData(toCoord);
                if (oldData != null)
                {
                    foreach (var data in oldData.ToList())
                        ExtendedSave.SetExtendedDataById(toCoord, data.Key, null);
                }

                // Copy new ext data from the coordinate that is about to be swapped in
                var newData = ExtendedSave.GetAllExtendedData(fromCoord);
                if (newData != null)
                {
                    foreach (var data in newData.ToList())
                        ExtendedSave.SetExtendedDataById(toCoord, data.Key, data.Value);
                }
            }

            public static bool ClothesFileControlLoading => false;

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

#if AI // todo not necessary for AI? needed for HS2?
            /// <summary>
            /// Needed to update controllers after ADV scene finishes loading since characters get loaded async so other events fire too early
            /// </summary>
            [HarmonyPostfix]
            [HarmonyPatch(typeof(CharaData), nameof(CharaData.Initialize))]
            public static void CharaData_InitializePost(CharaData __instance)
            {
                // Only run reload if the character was newly created
                if (__instance.isADVCreateChara)
                    ReloadChara(__instance.chaCtrl);
            }
#endif

            /// <summary>
            /// Needed to catch coordinate updates in studio, h scene and other places
            /// </summary>
            [HarmonyPostfix]
            // public bool ChangeNowCoordinate(ChaFileCoordinate srcCoorde, bool reload = false, bool forceChange = true)
            [HarmonyPatch(typeof(ChaControl), nameof(ChaControl.ChangeNowCoordinate), typeof(ChaFileCoordinate), typeof(bool), typeof(bool))]
            public static void CharaData_InitializePost(ChaControl __instance, ChaFileCoordinate srcCoorde, bool reload, bool forceChange)
            {
                if (srcCoorde == __instance.chaFile.coordinate)
                {
                    if (reload)
                        ReloadChara(__instance);
                    return;
                }

                // Make sure we were called by the correct methods to avoid triggering this when a character is being fully reloaded
                // Need to inspect whole stack trace and grab the earliest methods in the stack to avoid MoreAccessories hooks
                var isCoordinateLoad = new StackTrace().GetFrames()?.Any(f =>
                {
                    var method = f.GetMethod();
                    return method.Name == "UpdateClothEvent"
                           || method.Name == "LoadClothesFile"
                           || method.DeclaringType?.Name == "ADVMainScene"
                           || method.DeclaringType?.Name == "HSceneSpriteCoordinatesCard";
                });

                if (isCoordinateLoad != false)
                    OnCoordinateBeingLoaded(__instance, __instance.nowCoordinate);
            }
        }
    }
}
