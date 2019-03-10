using Harmony;
using Studio;

namespace KKAPI.Studio
{
    public static partial class StudioAPI
    {
        private static class Hooks
        {
            [HarmonyPostfix]
            [HarmonyPatch(typeof(MPCharCtrl), nameof(MPCharCtrl.OnClickRoot), new[] { typeof(int) })]
            public static void OnClickRoot(MPCharCtrl __instance, int _idx)
            {
                if (_idx == 0)
                {
                    foreach (var stateCategory in _customCurrentStateCategories)
                        stateCategory.UpdateInfo(__instance.ociChar);
                }
            }
        }
    }
}
