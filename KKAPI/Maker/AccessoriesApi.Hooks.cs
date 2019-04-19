using System;
using BepInEx.Logging;
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
        }
    }
}
