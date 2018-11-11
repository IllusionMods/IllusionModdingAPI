using System.Collections;
using Harmony;
using UnityEngine;

namespace MakerAPI
{
    public partial class MakerAPI
    {
        // ReSharper disable UnusedMember.Local
        private static class Hooks
        {
            private static bool _studioStarting;

            [HarmonyPrefix]
            [HarmonyPatch(typeof(UI_ToggleGroupCtrl), "Start")]
            public static void HBeforeToggleGroupStart(UI_ToggleGroupCtrl __instance)
            {
                var categoryTransfrom = __instance.transform;

                //Logger.Log(LogLevel.Info, categoryTransfrom.name + "\n" + string.Join("\n  ", categoryTransfrom.Cast<Transform>().Select(x=>x.name).ToArray()));

                if (categoryTransfrom?.parent != null && categoryTransfrom.parent.name == "CvsMenuTree")
                {
                    if (!_studioStarting)
                    {
                        Instance.OnRegisterCustomSubCategories();
                        Instance.StartCoroutine(OnMakerLoadingCo());
                        _studioStarting = true;
                    }

                    // Have to add missing subcategories now, before UI_ToggleGroupCtrl.Start runs
                    Instance.AddMissingSubCategories(__instance);
                }
            }

            [HarmonyPrefix]
            [HarmonyPatch(typeof(CustomScene), "OnDestroy")]
            public static void CustomScene_Destroy()
            {
                Instance.OnMakerExiting();
            }

            /*[HarmonyPrefix]
            [HarmonyPatch(typeof(BaseLoader), "Awake")]
            public static void CustomScene_Awake(BaseLoader __instance)
            {
                Instance.CurrentCustomScene = __instance as CustomScene;
            }*/

            private static IEnumerator OnMakerLoadingCo()
            {
                // Let maker objects run their Start methods
                yield return new WaitForEndOfFrame();

                Instance.OnMakerStartedLoading();
                
                // Wait a few frames to give everything chance to properly initialize
                for (var i = 0; i < 3; i++)
                    yield return null;

                Instance.OnMakerBaseLoaded();

                for (var i = 0; i < 3; i++)
                    yield return null;

                _studioStarting = false;
                Instance.OnMakerFinishedLoading();
            }
        }
    }
}
