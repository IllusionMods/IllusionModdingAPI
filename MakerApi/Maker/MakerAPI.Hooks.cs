using System.Collections;
using System.IO;
using ChaCustom;
using Harmony;
using UnityEngine;
using UnityEngine.UI;

namespace MakerAPI
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
                        Instance.InsideMaker = true;
                        _studioStarting = true;
                        Instance.OnRegisterCustomSubCategories();
                        Instance.StartCoroutine(OnMakerLoadingCo());
                    }

                    // Have to add missing subcategories now, before UI_ToggleGroupCtrl.Start runs
                    Instance.AddMissingSubCategories(__instance);
                }
            }

            private static IEnumerator OnMakerLoadingCo()
            {
                // Let maker objects run their Start methods
                yield return new WaitForEndOfFrame();

                Instance.OnMakerStartedLoading();

                // Wait a few frames to give everything chance to properly initialize
                for (var i = 0; i < 3; i++)
                    yield return null;

                Instance.OnMakerBaseLoaded();

                yield return null;

                Instance.OnCreateCustomControls();

                for (var i = 0; i < 2; i++)
                    yield return null;

                _studioStarting = false;
                Instance.OnMakerFinishedLoading();
            }

            [HarmonyPrefix]
            [HarmonyPatch(typeof(CustomScene), "Start")]
            public static void CustomScene_Start()
            {
                Instance.InsideMaker = Singleton<CustomBase>.Instance != null;
            }

            [HarmonyPrefix]
            [HarmonyPatch(typeof(CustomScene), "OnDestroy")]
            public static void CustomScene_Destroy()
            {
                Instance.OnMakerExiting();
                Instance.InsideMaker = false;
                LastLoadedChaFile = null;
            }

            [HarmonyPrefix, HarmonyPatch(typeof(CustomCharaFile), "Initialize")]
            public static void CustomScenePrefix()
            {
                Instance.CharaListIsLoading = true;
            }

            [HarmonyPostfix, HarmonyPatch(typeof(CustomCharaFile), "Initialize")]
            public static void CustomScenePostfix()
            {
                Instance.CharaListIsLoading = false;
            }
            
            [HarmonyPrefix]
            [HarmonyPatch(typeof(ChaFile), "LoadFile", new[] { typeof(BinaryReader), typeof(bool), typeof(bool) })]
            public static void ChaFileLoadFilePreHook(ChaFile __instance, BinaryReader br, bool noLoadPNG, bool noLoadStatus)
            {
                if (!Instance.CharaListIsLoading && Instance.InsideMaker)
                    LastLoadedChaFile = __instance;
                else
                    LastLoadedChaFile = null;
            }

            public static ChaFile LastLoadedChaFile;

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
                if (!Instance.CharaListIsLoading && Instance.InsideMaker)
                    Instance.OnChaFileLoaded(new ChaFileLoadedEventArgs(filename, sex, face, body, hair, parameter, coordinate, __instance, LastLoadedChaFile));
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
        }
    }
}
