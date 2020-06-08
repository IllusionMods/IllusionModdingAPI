using System.Collections;
using System.Diagnostics;
using System.IO;
using System.Linq;
using AIChara;
using CharaCustom;
using HarmonyLib;
using KKAPI.Utilities;
using UnityEngine;

namespace KKAPI.Maker
{
    public partial class MakerAPI
    {
        internal static class Hooks
        {
            private static bool _makerStarting;

            [HarmonyPrefix]
            [HarmonyPatch(typeof(CvsSelectWindow), "Start")]
            public static void CvsSelectWindowStart(CvsSelectWindow __instance)
            {
                if (!_makerStarting)
                {
                    CoordinateButtonClicked = 3;
                    InsideMaker = true;
                    _makerStarting = true;
                    OnRegisterCustomSubCategories();
                    KoikatuAPI.Instance.StartCoroutine(OnMakerLoadingCo());
                }

                // Have to add missing subcategories now, before UI_ToggleGroupCtrl.Start runs
                MakerInterfaceCreator.AddMissingSubCategories(__instance);
            }

            private static IEnumerator OnMakerLoadingCo()
            {
                var sw = Stopwatch.StartNew();

                // Let maker objects run their Start methods
                yield return new WaitForEndOfFrame();
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
            [HarmonyPatch(typeof(CharaCustom.CharaCustom), "Start")]
            public static void CustomScene_Start()
            {
                InsideMaker = Singleton<CustomBase>.Instance != null;
            }

            [HarmonyPrefix]
            [HarmonyPatch(typeof(CharaCustom.CharaCustom), "OnDestroy")]
            public static void CustomScene_Destroy()
            {
                OnMakerExiting();
                InsideMaker = false;
                InternalLastLoadedChaFile = null;
            }

            [HarmonyPrefix]
            [HarmonyPatch(typeof(CustomCharaFileInfoAssist), nameof(CustomCharaFileInfoAssist.CreateCharaFileInfoList))]
            public static void CreateCharaFileInfoList_Start()
            {
                CharaListIsLoading = true;
            }

            [HarmonyPostfix]
            [HarmonyPatch(typeof(CustomCharaFileInfoAssist), nameof(CustomCharaFileInfoAssist.CreateCharaFileInfoList))]
            public static void CreateCharaFileInfoList_End()
            {
                CharaListIsLoading = false;
            }

            public static void ChaFileLoadFilePreHook(ChaFile __instance)
            {
                if (InsideMaker)
                {
                    if (!CharaListIsLoading)
                        InternalLastLoadedChaFile = __instance;
                }
                else
                {
                    InternalLastLoadedChaFile = null;
                }
            }

            private static ChaFile _internalLastLoadedChaFile;
            public static ChaFile InternalLastLoadedChaFile
            {
                get => !InsideAndLoaded && !CharaCustom.CharaCustom.modeNew ? Singleton<CustomBase>.Instance.defChaCtrl : _internalLastLoadedChaFile;
                internal set => _internalLastLoadedChaFile = value;
            }

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

            public static void Init()
            {
                var hi = BepInEx.Harmony.HarmonyWrapper.PatchAll(typeof(Hooks));

                // AI LoadFile(BinaryReader br, int lang, bool noLoadPNG = false, bool noLoadStatus = true)
                var target = AccessTools.GetDeclaredMethods(typeof(ChaFile))
                    .Where(x => x.Name == "LoadFile" && x.GetParameters().Any(p => p.ParameterType == typeof(BinaryReader)))
                    .OrderByDescending(x => x.GetParameters().Length)
                    .First();
                KoikatuAPI.Logger.LogDebug("Hooking " + target.FullDescription());
                hi.Patch(target, new HarmonyMethod(typeof(Hooks), nameof(ChaFileLoadFilePreHook)));
            }

            /// <summary>
            /// Store which coord load button was pressed before running the stock game code
            /// </summary>
#if AI
            [HarmonyPostfix]
            [HarmonyPatch(typeof(CvsC_ClothesLoad), "Start")]
            public static void CustomClothesWindow_ButtonClickedHook(CustomClothesWindow ___clothesLoadWin)
            {
                ___clothesLoadWin.onClick01 = (info => CoordinateButtonClicked = 1) + ___clothesLoadWin.onClick01;
                ___clothesLoadWin.onClick02 = (info => CoordinateButtonClicked = 2) + ___clothesLoadWin.onClick02;
                ___clothesLoadWin.onClick03 = (info => CoordinateButtonClicked = 3) + ___clothesLoadWin.onClick03;
            }
#elif HS2
            // Changed to coroutine
            [HarmonyPostfix]
            [HarmonyPatch(typeof(CvsC_ClothesLoad), "Start")]
            public static void CustomClothesWindow_ButtonClickedHook(CustomClothesWindow ___clothesLoadWin, ref IEnumerator __result)
            {
                __result = __result.AppendCo(() =>
                {
                    ___clothesLoadWin.onClick01 = (info => CoordinateButtonClicked = 1) + ___clothesLoadWin.onClick01;
                    ___clothesLoadWin.onClick02 = (info => CoordinateButtonClicked = 2) + ___clothesLoadWin.onClick02;
                    ___clothesLoadWin.onClick03 = (info => CoordinateButtonClicked = 3) + ___clothesLoadWin.onClick03;
                });
            }
#endif
            internal static int CoordinateButtonClicked;
        }
    }
}
