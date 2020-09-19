using System.Collections;
using HarmonyLib;
using Studio;
using UnityEngine;

namespace KKAPI.Studio
{
    public static partial class StudioAPI
    {
        private static class Hooks
        {
            public static void SetupHooks()
            {
                BepInEx.Harmony.HarmonyWrapper.PatchAll(typeof(Hooks));
            }

            //[HarmonyPostfix]
            //[HarmonyPatch(typeof(MPCharCtrl), nameof(MPCharCtrl.OnClickRoot), typeof(int))]
            //public static void OnClickRoot(OCIChar ___m_OCIChar, int _idx)
            //{
            //    IEnumerator DelayedUpdateTrigger()
            //    {
            //        // Need to wait for the selected character to change or we risk overwriting current character with new character's data
            //        yield return CoroutineUtils.WaitForEndOfFrame;
            //
            //        if (_idx == 0)
            //        {
            //            foreach (var stateCategory in _customCurrentStateCategories)
            //                stateCategory.UpdateInfo(___m_OCIChar);
            //        }
            //    }
            //
            //    KoikatuAPI.Instance.StartCoroutine(DelayedUpdateTrigger());
            //}
        }
    }
}
