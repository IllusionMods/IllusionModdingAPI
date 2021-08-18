using ChaCustom;
using HarmonyLib;
#pragma warning disable 612

namespace KKAPI.Maker
{
    public static partial class AccessoriesApi
    {
        private static class Hooks
        {
            [HarmonyPostfix]
            [HarmonyPatch(typeof(CustomAcsSelectKind), nameof(CustomAcsSelectKind.ChangeSlot))]
            public static void ChangeSlotPostfix(CustomAcsSelectKind __instance, int _no)
            {
                OnSelectedMakerSlotChanged(__instance, _no);
            }

            [HarmonyBefore(new string[] { "com.joan6694.kkplugins.moreaccessories" })]
            [HarmonyPrefix]
            [HarmonyPatch(typeof(CvsAccessory), nameof(CvsAccessory.UpdateSelectAccessoryKind))]
            public static void UpdateSelectAccessoryKindPrefix(CvsAccessory __instance, ref int __state)
            {
                // Used to see if the kind actually changed
                __state = GetPartsInfo((int)__instance.slotNo).id;
            }

            [HarmonyPostfix]
            [HarmonyPatch(typeof(CvsAccessory), nameof(CvsAccessory.UpdateSelectAccessoryKind))]
            public static void UpdateSelectAccessoryKindPostfix(CvsAccessory __instance, ref int __state)
            {
                // Only send the event if the kind actually changed
                if (__state != GetPartsInfo((int)__instance.slotNo).id)
                    OnAccessoryKindChanged(__instance, (int)__instance.slotNo);
                AutomaticControlVisibility();//used to tell non-automated plugins that accessory kind has changed
            }

            [HarmonyPostfix]
            [HarmonyPatch(typeof(CvsAccessory), nameof(CvsAccessory.UpdateSelectAccessoryType))]
            public static void UpdateSelectAccessoryTypePostfix()
            {
                AutomaticControlVisibility();
            }

#if KK || KKS
            [HarmonyPostfix]
            [HarmonyPatch(typeof(CvsAccessoryCopy), "CopyAcs")]
            public static void CopyCopyAcsPostfix(CvsAccessoryCopy __instance)
            {
                OnCopyAcs(__instance);
            }

            [HarmonyPostfix]
            [HarmonyPatch(typeof(ChaControl), nameof(ChaControl.ChangeCoordinateType), typeof(ChaFileDefine.CoordinateType), typeof(bool))]
            public static void ChangeCoordinateTypePostfix()
            {
                AutomaticControlVisibility();
            }
#endif

            [HarmonyPostfix]
            [HarmonyPatch(typeof(CvsAccessoryChange), "CopyAcs")]
            public static void ChangeCopyAcsPostfix(CvsAccessoryChange __instance)
            {
                OnChangeAcs(__instance, __instance.selSrc, __instance.selDst);
            }
        }
    }
}
