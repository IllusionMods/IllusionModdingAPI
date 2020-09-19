using System.Collections;
using System.Diagnostics;
using System.IO;
using System.Linq;
using ChaCustom;
using HarmonyLib;
using KKAPI.Maker.UI;
using KKAPI.Utilities;
using UnityEngine.UI;

namespace KKAPI.Maker
{
    public partial class MakerAPI
    {
        private static class Hooks
        {
            private static bool _makerStarting;

            [HarmonyPrefix]
            [HarmonyPatch(typeof(UI_ToggleGroupCtrl), "Start")]
            public static void HBeforeToggleGroupStart(UI_ToggleGroupCtrl __instance)
            {
                var categoryTransfrom = __instance.transform;

                if (categoryTransfrom?.parent != null && categoryTransfrom.parent.name == "CvsMenuTree")
                {
                    if (!_makerStarting)
                    {
                        InsideMaker = true;
                        _makerStarting = true;
                        OnRegisterCustomSubCategories();
                        KoikatuAPI.Instance.StartCoroutine(OnMakerLoadingCo());
                    }

                    // Have to add missing subcategories now, before UI_ToggleGroupCtrl.Start runs
                    MakerInterfaceCreator.AddMissingSubCategories(__instance);
                }
            }

            private static IEnumerator OnMakerLoadingCo()
            {
                var sw = Stopwatch.StartNew();

                // Let maker objects run their Start methods
                yield return CoroutineUtils.WaitForEndOfFrame;
                var sw1 = sw.ElapsedMilliseconds;

                OnMakerStartedLoading();

                // Wait a few frames to give everything chance to properly initialize
                for (var i = 0; i < 3; i++)
                    yield return null;
                var sw2 = sw.ElapsedMilliseconds - sw1;

                OnMakerBaseLoaded();

                yield return null;

                var sw3 = sw.ElapsedMilliseconds - sw1 - sw2;
                OnCreateCustomControls();
                var sw4 = sw.ElapsedMilliseconds - sw1 - sw2 - sw3;

                for (var i = 0; i < 2; i++)
                    yield return null;

                _makerStarting = false;
                OnMakerFinishedLoading();

                KoikatuAPI.Logger.LogDebug($"Maker loaded in {sw.ElapsedMilliseconds}ms");
                KoikatuAPI.Logger.LogDebug($"1st frame:{sw1}ms; Maker base:{sw2}ms; Custom controls:{sw4}ms");
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
                // Make sure this is an actually useful chafile, not e.g. the default character used to grab defaults from (defChaInfo)
                if (!CharaListIsLoading && InsideMaker && CustomBase.Instance.defChaInfo != __instance)
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
