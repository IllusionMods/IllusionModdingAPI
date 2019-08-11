using System;
#if KK
using Harmony;
#else
using HarmonyLib;
using BepInEx.Harmony;
#endif

namespace KKAPI
{
    internal static class HarmonyPatcher
    {
#if KK
        public static HarmonyInstance PatchAll(Type t)
        {
            var harmonyInstance = Harmony.HarmonyInstance.Create(t.FullName);
            harmonyInstance.PatchAll(t);
            return harmonyInstance;
        }
#elif EC
        public static Harmony PatchAll(Type t)
        {
            return HarmonyWrapper.PatchAll(t);
        }
#endif
    }
}
