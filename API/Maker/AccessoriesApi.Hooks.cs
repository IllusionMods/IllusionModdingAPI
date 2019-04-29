using ChaCustom;
using Harmony;

namespace KKAPI.Maker
{
    public static partial class AccessoriesApi
    {
        private static class Hooks
        {
            [HarmonyPostfix]
            [HarmonyPatch(typeof(CustomAcsSelectKind), nameof(CustomAcsSelectKind.ChangeSlot))]
            public static void ChangeSlotPostfix(CustomAcsSelectKind __instance, int _no, bool open)
            {
                OnSelectedMakerSlotChanged(__instance, _no);
            }

            [HarmonyPostfix]
            [HarmonyPatch(typeof(CvsAccessory), nameof(CvsAccessory.UpdateSelectAccessoryKind))]
            public static void UpdateSelectAccessoryKindPostfix(CvsAccessory __instance)
            {
                OnAccessoryKindChanged(__instance, (int)__instance.slotNo);
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
            [HarmonyPatch(typeof(CvsAccessoryChange), "CopyAcs")]
            public static void ChangeCopyAcsPostfix(CvsAccessoryChange __instance)
            {
                OnChangeAcs(__instance);
            }
        }
    }
}
