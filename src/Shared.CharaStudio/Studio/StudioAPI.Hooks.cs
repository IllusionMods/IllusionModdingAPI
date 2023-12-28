using HarmonyLib;
using KKAPI.Utilities;
using Studio;
using System.Collections;

namespace KKAPI.Studio
{
    public static partial class StudioAPI
    {
        private static class Hooks
        {
            private static global::Studio.Studio Studio => global::Studio.Studio.Instance;

            public static void SetupHooks()
            {
                Harmony.CreateAndPatchAll(typeof(Hooks));
            }

            [HarmonyPostfix]
            [HarmonyPatch(typeof(MPCharCtrl), nameof(MPCharCtrl.OnClickRoot), typeof(int))]
            public static void OnClickRoot(int _idx, OCIChar ___m_OCIChar)
            {
                IEnumerator DelayedUpdateTrigger()
                {
                    // Need to wait for the selected character to change or we risk overwriting current character with new character's data
                    yield return CoroutineUtils.WaitForEndOfFrame;

                    if (_idx == 0)
                    {
                        foreach (var stateCategory in _customCurrentStateCategories)
                            stateCategory.UpdateInfo(___m_OCIChar);
                    }
                }

                KoikatuAPI.Instance.StartCoroutine(DelayedUpdateTrigger());
            }

            [HarmonyPrefix]
            [HarmonyPatch(typeof(global::Studio.Studio), nameof(Studio.DeleteInfo), new System.Type[] { typeof(ObjectInfo), typeof(bool) })]
            public static void DeleteInfo(ObjectInfo _info, bool _delKey = true)
            {
                if (!_delKey || !Studio.dicObjectCtrl.TryGetValue(_info.dicKey, out ObjectCtrlInfo oci)) return;
                if (StudioObjectExtensions.dicTreeNodeOCI.ContainsKey(oci.treeNodeObject))
                    StudioObjectExtensions.dicTreeNodeOCI.Remove(oci.treeNodeObject);
            }

            [HarmonyPostfix]
            [HarmonyPatch(typeof(global::Studio.Studio), nameof(Studio.AddInfo), new System.Type[] { typeof(ObjectInfo), typeof(ObjectCtrlInfo) })]
            public static void AddInfo(ObjectInfo _info, ObjectCtrlInfo _ctrlInfo)
            {
                IEnumerator DelayedUpdateTrigger()
                {
                    // Need to wait for the selected character to change or we risk overwriting current character with new character's data
                    yield return CoroutineUtils.WaitForEndOfFrame;

                    if (Singleton<global::Studio.Studio>.IsInstance() && _info != null && _ctrlInfo != null)
                        if (!StudioObjectExtensions.dicTreeNodeOCI.ContainsKey(_ctrlInfo.treeNodeObject))
                            StudioObjectExtensions.dicTreeNodeOCI.Add(_ctrlInfo.treeNodeObject, _ctrlInfo);
                }
                
                KoikatuAPI.Instance.StartCoroutine(DelayedUpdateTrigger());
            }
        }
    }
}
