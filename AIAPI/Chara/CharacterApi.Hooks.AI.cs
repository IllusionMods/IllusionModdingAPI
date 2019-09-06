using AIChara;
using KKAPI.Maker;
using System;
using System.Collections;
using System.Linq;
using HarmonyLib;

namespace KKAPI.Chara
{
    public static partial class CharacterApi
    {
        private static class Hooks
        {
            public static void InitHooks()
            {
                var i = BepInEx.Harmony.HarmonyWrapper.PatchAll(typeof(Hooks));

                var target = typeof(ChaControl).GetMethods().Single(info => info.Name == nameof(ChaControl.Initialize) && info.GetParameters().Length >= 5);
                i.Patch(target, null, new HarmonyMethod(typeof(Hooks), nameof(ChaControl_InitializePostHook)));
                
                var target3 = typeof(ChaFile).GetMethods().Single(info => info.Name == nameof(ChaFile.CopyAll));
                i.Patch(target3, null, new HarmonyMethod(typeof(Hooks), nameof(ChaFile_CopyChaFileHook)));
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
                    if (handler.ExtendedDataCopier == null)
                        continue;

                    try
                    {
                        handler.ExtendedDataCopier(destination, source);
                    }
                    catch (Exception e)
                    {
                        KoikatuAPI.Logger.LogError(e);
                    }
                }
            }

            public static bool ClothesFileControlLoading => false;
        }
    }
}
