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
                if (CustomAcs != null)
                    OnSelectedMakerSlotChanged(__instance, _no);
            }

            [HarmonyBefore(new string[] { "com.joan6694.kkplugins.moreaccessories" })]
            [HarmonyPrefix]
            [HarmonyPatch(typeof(CvsAccessory), nameof(CvsAccessory.UpdateSelectAccessoryKind))]
            public static void UpdateSelectAccessoryKindPrefix(CvsAccessory __instance, int index, ref bool __state)
            {
                // Check if the kind actually changed
                __state = __instance.accessory.parts[__instance.nSlotNo].id != index;
            }

            [HarmonyPostfix]
            [HarmonyPatch(typeof(CvsAccessory), nameof(CvsAccessory.UpdateSelectAccessoryKind))]
            public static void UpdateSelectAccessoryKindPostfix(CvsAccessory __instance, ref bool __state)
            {
                // Only send the event if the kind actually changed
                if (__state)
                    OnAccessoryKindChanged(__instance, (int)__instance.slotNo);

                AutomaticControlVisibility();
            }

            [HarmonyPrefix]
            [HarmonyPatch(typeof(CvsAccessory), nameof(CvsAccessory.UpdateSelectAccessoryType))]
            public static void UpdateSelectAccessoryTypePrefix(CvsAccessory __instance, int index, ref bool __state)
            {
                // Check if the type actually changed
                __state = __instance.accessory.parts[__instance.nSlotNo].type - 120 != index;
            }

            [HarmonyPostfix]
            [HarmonyPatch(typeof(CvsAccessory), nameof(CvsAccessory.UpdateSelectAccessoryType))]
            public static void UpdateSelectAccessoryTypePostfix(CvsAccessory __instance, ref bool __state)
            {
                // Only send the event if the type actually changed
                // Always fire the event when kind changes since item ID could stay the same but the item won't be the same
                if (__state)
                    OnAccessoryKindChanged(__instance, (int)__instance.slotNo);

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

            [HarmonyPostfix]
            [HarmonyPriority(Priority.First)]
            [HarmonyPatch(typeof(CustomAcsChangeSlot), nameof(CustomAcsChangeSlot.Start))]
            private static void CustomAcsChangeSlotPostfix(CustomAcsChangeSlot __instance)
            {
                CustomAcs = __instance;
            }
        }
    }
}
