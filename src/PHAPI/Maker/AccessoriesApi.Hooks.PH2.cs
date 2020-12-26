using HarmonyLib;
using UnityEngine.UI;

namespace KKAPI.Maker
{
    public static partial class AccessoriesApi
    {
        private static class Hooks
        {
            [HarmonyPostfix]
            [HarmonyPatch(typeof(AccessoryCustomEdit), "ChangeTab")]
            public static void ChangeSlotPostfix(AccessoryCustomEdit __instance, int newTab)
            {
                OnSelectedMakerSlotChanged(__instance, newTab);
            }

            [HarmonyPostfix]
            [HarmonyPatch(typeof(AccessoryCustomEdit), "OnChangeAcceItem")]
            public static void OnAccessoryChangedHook(AccessoryCustomEdit __instance, int slot, CustomSelectSet set)
            {
                OnAccessoryChanged(__instance, slot);
            }

#if KK
            [HarmonyPostfix]
            [HarmonyPatch(typeof(CvsAccessoryCopy), "CopyAcs")]
            public static void CopyCopyAcsPostfix(CvsAccessoryCopy __instance)
            {
                OnCopyAcs(__instance);
            }
#endif

            [HarmonyPostfix]
            [HarmonyPatch(typeof(AcceCopyHelperUI), "Button_CopySlot")]
            //todo These only copy positions, so don't trigger?
            //[HarmonyPatch(typeof(AcceCopyHelperUI), "Button_CopyPos")]
            //[HarmonyPatch(typeof(AcceCopyHelperUI), "Button_CopyPosRev_H")]
            //[HarmonyPatch(typeof(AcceCopyHelperUI), "Button_CopyPosRev_V")]
            public static void CopyAcsPostfix(AcceCopyHelperUI __instance, Toggle[] ___dstSlots, Toggle[] ___srcSlots)
            {
                var selDst = -1;
                var selSrc = -1;
                for (var i = 0; i < ___dstSlots.Length; i++)
                {
                    if (___srcSlots[i].isOn) selSrc = i;
                    if (___dstSlots[i].isOn) selDst = i;
                }
                OnChangeAcs(__instance, selSrc, selDst);
            }
        }
    }
}
