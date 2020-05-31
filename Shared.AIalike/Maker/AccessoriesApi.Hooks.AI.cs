using CharaCustom;
using HarmonyLib;

namespace KKAPI.Maker
{
    public static partial class AccessoriesApi
    {
        private static class Hooks
        {
            [HarmonyPostfix]
            [HarmonyPatch(typeof(CvsA_Slot), nameof(CvsA_Slot.ChangeMenuFunc))]
            public static void ChangeMenuFuncPostfix(CvsA_Slot __instance)
            {
                OnSelectedMakerSlotChanged(__instance, __instance.SNo);
            }

            [HarmonyPostfix]
            [HarmonyPatch(typeof(CvsA_Slot), nameof(CvsA_Slot.ChangeAcsId), typeof(int))]
            public static void ChangeAcsIdPostfix(CvsA_Slot __instance, int id)
            {
                OnAccessoryKindChanged(__instance, id);
            }

            [HarmonyPostfix]
            [HarmonyPatch(typeof(CvsA_Slot), nameof(CvsA_Slot.ChangeAcsType), typeof(int))]
            public static void ChangeAcsTypePostfix(CvsA_Slot __instance)
            {
                OnAccessoryKindChanged(__instance, __instance.SNo);
            }
        }
    }
}
