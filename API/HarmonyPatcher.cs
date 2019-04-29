using System;

namespace KKAPI
{
    internal static class HarmonyPatcher
    {
        public static void PatchAll(Type t)
        {
#if EC
            BepInEx.Harmony.HarmonyWrapper.PatchAll(t);
#elif KK
            Harmony.HarmonyInstance.Create(t.FullName).PatchAll(t);
#endif
        }
    }
}