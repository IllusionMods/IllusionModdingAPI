using System.Collections;
using System.IO;
using System.Linq;
using AIChara;
using CharaCustom;
using HarmonyLib;
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

                _makerStarting = false;
                OnMakerFinishedLoading();
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

            public static void ChaFileLoadFilePreHook(ChaFile __instance)
            {
                if (!CharaListIsLoading && InsideMaker)
                    InternalLastLoadedChaFile = __instance;
                else
                    InternalLastLoadedChaFile = null;
            }

            private static ChaFile _internalLastLoadedChaFile;
            public static ChaFile InternalLastLoadedChaFile
            {
                get => !InsideAndLoaded && !CharaCustom.CharaCustom.modeNew ? Singleton<CustomBase>.Instance.defChaCtrl : _internalLastLoadedChaFile;
                private set => _internalLastLoadedChaFile = value;
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

            /* todo useful?
             * /// <summary>
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
            }*/

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
            [HarmonyPostfix]
            [HarmonyPatch(typeof(CustomClothesWindow), "Start")]
            public static void CustomClothesWindow_ButtonClickedHook(CustomClothesWindow __instance)
            {
                __instance.onClick01 = (info => CoordinateButtonClicked = 1) + __instance.onClick01;
                __instance.onClick02 = (info => CoordinateButtonClicked = 2) + __instance.onClick02;
                __instance.onClick03 = (info => CoordinateButtonClicked = 3) + __instance.onClick03;
            }
            internal static int CoordinateButtonClicked;
        }
    }
}
