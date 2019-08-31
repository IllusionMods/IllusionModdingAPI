using System.Collections;
using System.IO;
using System.Linq;
#if KK || EC
using ChaCustom;
#elif AI
using AIChara;
using CharaCustom;
#endif
using HarmonyLib;
using KKAPI.Maker.UI;
using UnityEngine;
using UnityEngine.UI;
#if AI
using CustomScene = CharaCustom.CharaCustom;
#endif

namespace KKAPI.Maker
{
    public partial class MakerAPI
    {
        private static class Hooks
        {
            private static bool _studioStarting;

            [HarmonyPrefix]
            [HarmonyPatch(typeof(UI_ToggleGroupCtrl), "Start")]
            public static void HBeforeToggleGroupStart(UI_ToggleGroupCtrl __instance)
            {
                var categoryTransfrom = __instance.transform;

                if (categoryTransfrom?.parent != null && categoryTransfrom.parent.name == "CvsMenuTree")
                {
                    if (!_studioStarting)
                    {
                        InsideMaker = true;
                        _studioStarting = true;
                        OnRegisterCustomSubCategories();
                        KoikatuAPI.Instance.StartCoroutine(OnMakerLoadingCo());
                    }

                    // Have to add missing subcategories now, before UI_ToggleGroupCtrl.Start runs
                    AddMissingSubCategories(__instance);
                }
            }

            private static IEnumerator OnMakerLoadingCo()
            {
                // Let maker objects run their Start methods
                yield return new WaitForEndOfFrame();

                OnMakerStartedLoading();

                // Wait a few frames to give everything chance to properly initialize
                for (var i = 0; i < 3; i++)
                    yield return null;

                OnMakerBaseLoaded();

                yield return null;

                OnCreateCustomControls();

                for (var i = 0; i < 2; i++)
                    yield return null;

                _studioStarting = false;
                OnMakerFinishedLoading();
            }

            [HarmonyPrefix]
            [HarmonyPatch(typeof(CustomScene), "Start")]
            public static void CustomScene_Start()
            {
                InsideMaker = Singleton<CustomBase>.Instance != null;
            }

            [HarmonyPrefix]
            [HarmonyPatch(typeof(CustomScene), "OnDestroy")]
            public static void CustomScene_Destroy()
            {
                OnMakerExiting();
                InsideMaker = false;
                InternalLastLoadedChaFile = null;
            }

            [HarmonyPrefix, HarmonyPatch(typeof(CustomCharaFile), "Initialize")]
            public static void CustomScenePrefix()
            {
                CharaListIsLoading = true;
            }

            [HarmonyPostfix, HarmonyPatch(typeof(CustomCharaFile), "Initialize")]
            public static void CustomScenePostfix()
            {
                CharaListIsLoading = false;
            }

            public static void ChaFileLoadFilePreHook(ChaFile __instance)
            {
                if (!CharaListIsLoading && InsideMaker)
                    InternalLastLoadedChaFile = __instance;
                else
                    InternalLastLoadedChaFile = null;
            }

            public static ChaFile InternalLastLoadedChaFile;

            [HarmonyPostfix]
            [HarmonyPatch(typeof(ChaFileControl), "LoadFileLimited", new[]
            {
                typeof(string),
                typeof(byte),
                typeof(bool),
                typeof(bool),
                typeof(bool),
                typeof(bool),
                typeof(bool)
            })]
            public static void ChaFileControl_LoadLimitedPostHook(string filename, byte sex, bool face, bool body,
                bool hair, bool parameter, bool coordinate, ChaFileControl __instance)
            {
                if (!CharaListIsLoading && InsideMaker)
                    OnChaFileLoaded(new ChaFileLoadedEventArgs(filename, sex, face, body, hair, parameter, coordinate, __instance, InternalLastLoadedChaFile));
            }

            /// <summary>
            /// Keep Load button in maker character load list enabled if any of the extra toggles are enabled, but none of the stock ones are. 
            /// </summary>
            [HarmonyPrefix]
            [HarmonyPatch(typeof(Selectable), "set_interactable")]
            public static void LoadButtonOverride(Selectable __instance, ref bool value)
            {
                if (!value)
                {
                    if (ReferenceEquals(__instance, MakerLoadToggle.LoadButton))
                        value = MakerLoadToggle.AnyEnabled;
                    else if (ReferenceEquals(__instance, MakerCoordinateLoadToggle.LoadButton))
                        value = MakerCoordinateLoadToggle.AnyEnabled;
                }
            }

            public static void Init()
            {
                var hi = BepInEx.Harmony.HarmonyWrapper.PatchAll(typeof(Hooks));

                // KK LoadFile(BinaryReader br, bool noLoadPNG, bool noLoadStatus)
                // EC LoadFile(BinaryReader br, int lang, bool noLoadPNG, bool noLoadStatus)
                // AI LoadFile(BinaryReader br, int lang, bool noLoadPNG = false, bool noLoadStatus = true)
                var target = AccessTools.GetDeclaredMethods(typeof(ChaFile))
                    .Where(x => x.Name == "LoadFile" && x.GetParameters().Any(p => p.ParameterType == typeof(BinaryReader)))
                    .OrderByDescending(x => x.GetParameters().Length)
                    .First();
                KoikatuAPI.Logger.LogDebug("Hooking " + target.FullDescription());
                hi.Patch(target, new HarmonyMethod(typeof(Hooks), nameof(ChaFileLoadFilePreHook)));
            }
        }
    }
}
