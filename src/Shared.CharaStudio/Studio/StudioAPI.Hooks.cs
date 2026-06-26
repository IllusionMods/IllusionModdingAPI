using HarmonyLib;
using KKAPI.Utilities;
using Studio;
using System.Collections;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using UnityEngine.EventSystems;

namespace KKAPI.Studio
{
    public static partial class StudioAPI
    {
        private static class Hooks
        {
            public static void SetupHooks()
            {
                Harmony.CreateAndPatchAll(typeof(Hooks));
            }

            [HarmonyPostfix]
            [HarmonyPatch(typeof(MPCharCtrl), nameof(MPCharCtrl.OnClickRoot), typeof(int))]
            private static void OnClickRoot(int _idx, OCIChar ___m_OCIChar)
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

            [HarmonyPostfix]
            [HarmonyPatch(typeof(TreeNodeObject), nameof(TreeNodeObject.Start))]
            private static void OnTreeNodeObjectStart(TreeNodeObject __instance)
            {
                if (!InsideStudio) return;

                __instance.m_ButtonSelect.GetOrAddComponent<PointerDown>().Instance = __instance;

                //// Create a right click handler for the tree node object to allow for custom context menu items
                //__instance.m_ButtonSelect.gameObject.OnMouseDownAsObservable().Subscribe(_ =>
                //{
                //    if (Input.GetMouseButtonDown(1))
                //        OnShowWorkspaceContextMenu(__instance);
                //});
            }

            private sealed class PointerDown : MonoBehaviour, IPointerClickHandler
            {
                internal TreeNodeObject Instance;
                public void OnPointerClick(PointerEventData eventData)
                {
                    if (Instance && eventData.button == PointerEventData.InputButton.Right)
                        StudioContextMenus.OnShowWorkspaceContextMenu(Instance);
                }
            }
        }
    }
}
