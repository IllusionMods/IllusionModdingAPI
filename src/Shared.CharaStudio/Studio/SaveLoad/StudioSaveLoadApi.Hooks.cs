using ExtensibleSaveFormat;
using HarmonyLib;
using Studio;
using System.Collections;
using System.Collections.Generic;
using System.Reflection.Emit;
using UnityEngine;

namespace KKAPI.Studio.SaveLoad
{
    public static partial class StudioSaveLoadApi
    {
        private static class Hooks
        {
            /// <summary>
            /// A lookup for original dicKey IDs. It is generated on scene import, and only useful then.
            /// Key is the new ID in the scene (needs to be used currently), while Value is the old ID
            /// in the save file (same as at the time scene was saved).
            /// </summary>
            public static readonly Dictionary<int, int> ImportDictionary = new Dictionary<int, int>();

            private static bool _loadOrImportSuccess;

            private static int _newIndex;

            public static void SetupHooks()
            {
                BepInEx.Harmony.HarmonyWrapper.PatchAll(typeof(Hooks));
                ExtendedSave.SceneBeingImported += path => _loadOrImportSuccess = true;
                ExtendedSave.SceneBeingLoaded += path => _loadOrImportSuccess = true;
            }

            [HarmonyPostfix]
            [HarmonyPatch(typeof(global::Studio.Studio), nameof(global::Studio.Studio.GetNewIndex))]
            public static void GetNewIndex(int __result)
            {
                _newIndex = __result;
            }

            /// <summary>
            /// The original code reads the dicKey of an object on import and does nothing with it. Capture that variable and use it to
            /// construct an import dictionary.
            /// GetNewIndex is called before this and will be used to get the dicKey used in the scene.
            /// </summary>
            [HarmonyTranspiler]
            [HarmonyPatch(typeof(ObjectInfo), nameof(ObjectInfo.Load))]
            public static IEnumerable<CodeInstruction> ObjectInfoLoadTranspiler(IEnumerable<CodeInstruction> instructions)
            {
                foreach (var x in instructions)
                {
                    if (x.opcode == OpCodes.Pop)
                    {
                        x.opcode = OpCodes.Call;
                        x.operand = typeof(Hooks).GetMethod(nameof(SetImportDictionary), AccessTools.all);
                    }
                    yield return x;
                }
            }

            private static void SetImportDictionary(int originalDicKey)
            {
                ImportDictionary[_newIndex] = originalDicKey;
            }

            [HarmonyPrefix]
            [HarmonyPatch(typeof(global::Studio.Studio), nameof(global::Studio.Studio.InitScene))]
            public static void InitScenePrefix()
            {
                ImportDictionary.Clear();
            }

            [HarmonyPostfix]
            [HarmonyPatch(typeof(global::Studio.Studio), nameof(global::Studio.Studio.InitScene))]
            public static void InitScenePostfix()
            {
                if (!LoadInProgress && !ImportInProgress)
                    SceneLoadComplete(SceneOperationKind.Clear);
            }

            [HarmonyPostfix]
            [HarmonyPatch(typeof(global::Studio.Studio), nameof(global::Studio.Studio.ImportScene))]
            public static void ImportScenePostfix()
            {
                SceneLoadComplete(SceneOperationKind.Import);
                ImportDictionary.Clear();
            }

            [HarmonyPrefix]
            [HarmonyPatch(typeof(global::Studio.Studio), nameof(global::Studio.Studio.ImportScene))]
            public static void ImportScenePrefix()
            {
                ImportDictionary.Clear();
                ImportInProgress = true;
            }

#if !PH
            [HarmonyPostfix]
            [HarmonyPatch(typeof(global::Studio.Studio), nameof(global::Studio.Studio.LoadSceneCoroutine))]
            public static void LoadSceneCoroutinePostfix(ref IEnumerator __result)
            {
                // Setup a coroutine postfix
                var original = __result;
                __result = new[]
                {
                    original,
                    LoadCoPostfixCo()
                }.GetEnumerator();
            }

            private static IEnumerator LoadCoPostfixCo()
            {
                SceneLoadComplete(SceneOperationKind.Load);
                yield break;
            }

            [HarmonyPrefix]
            [HarmonyPatch(typeof(global::Studio.Studio), nameof(global::Studio.Studio.LoadSceneCoroutine))]
            public static void LoadSceneCoroutinePrefix()
            {
                ImportDictionary.Clear();
                LoadInProgress = true;
            }
#endif

            [HarmonyPostfix]
            [HarmonyPatch(typeof(global::Studio.Studio), nameof(global::Studio.Studio.LoadScene))]
            public static void LoadScenePostfix()
            {
                SceneLoadComplete(SceneOperationKind.Load);
            }

            [HarmonyPrefix]
            [HarmonyPatch(typeof(global::Studio.Studio), nameof(global::Studio.Studio.LoadScene))]
            public static void LoadScenePrefix()
            {
                ImportDictionary.Clear();
                LoadInProgress = true;
            }

            [HarmonyPostfix, HarmonyPatch(typeof(global::Studio.Studio), nameof(global::Studio.Studio.Duplicate))]
            public static void DuplicatePostfix()
            {
                OnObjectsBeingCopied();
                ImportDictionary.Clear();
            }

            private static void SceneLoadComplete(SceneOperationKind operation)
            {
                LoadInProgress = false;
                ImportInProgress = false;
                if (_loadOrImportSuccess || operation == SceneOperationKind.Clear)
                {
                    _loadOrImportSuccess = false;

                    OnSceneBeingLoaded(operation);
                }
            }

            [HarmonyPrefix]
            [HarmonyPatch(typeof(OCIItem), nameof(OCIItem.OnDelete))]
            [HarmonyPatch(typeof(OCIChar), nameof(OCIChar.OnDelete))]
            [HarmonyPatch(typeof(OCIFolder), nameof(OCIFolder.OnDelete))]
            [HarmonyPatch(typeof(OCILight), nameof(OCILight.OnDelete))]
#if !PH
            [HarmonyPatch(typeof(OCICamera), nameof(OCICamera.OnDelete))]
            [HarmonyPatch(typeof(OCIRoute), nameof(OCIRoute.OnDelete))]
#endif
            private static void OCI_OnDelete(ObjectCtrlInfo __instance) => OnObjectBeingDeleted(__instance);

            [HarmonyPostfix]
            [HarmonyPatch(typeof(OCIItem), nameof(OCIItem.OnVisible))]
            [HarmonyPatch(typeof(OCIChar), nameof(OCIChar.OnVisible))]
            [HarmonyPatch(typeof(OCIFolder), nameof(OCIFolder.OnVisible))]
            [HarmonyPatch(typeof(OCILight), nameof(OCILight.OnVisible))]
#if !PH
            [HarmonyPatch(typeof(OCICamera), nameof(OCICamera.OnVisible))]
            [HarmonyPatch(typeof(OCIRoute), nameof(OCIRoute.OnVisible))]
#endif
            private static void OCI_OnVisible(ObjectCtrlInfo __instance, bool _visible) => OnObjectVisibilityToggled(__instance, _visible);

            [HarmonyPostfix, HarmonyPatch(typeof(TreeNodeCtrl), "SelectSingle")]
            private static void TreeNodeCtrl_SelectSingle(TreeNodeObject _node)
            {
                ObjectCtrlInfo ctrlInfo = global::Studio.Studio.GetCtrlInfo(_node);
                if (ctrlInfo != null)
                    OnObjectsSelected(new List<ObjectCtrlInfo> { ctrlInfo });
            }

#if !PH
            [HarmonyPostfix, HarmonyPatch(typeof(TreeNodeCtrl), "SelectMultiple")]
            private static void TreeNodeCtrl_SelectMultiple()
            {
                List<ObjectCtrlInfo> selectedObjects = new List<ObjectCtrlInfo>();
                foreach (var node in Singleton<global::Studio.Studio>.Instance.treeNodeCtrl.selectNodes)
                    selectedObjects.Add(global::Studio.Studio.GetCtrlInfo(node));
                OnObjectsSelected(selectedObjects);
            }
#endif

            [HarmonyPostfix, HarmonyPatch(typeof(TreeNodeCtrl), "SetSelectNode")]
            private static void TreeNodeCtrl_SetSelectNode()
            {
                if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
                {
                    List<ObjectCtrlInfo> selectedObjects = new List<ObjectCtrlInfo>();
                    foreach (var node in Singleton<global::Studio.Studio>.Instance.treeNodeCtrl.selectNodes)
                        selectedObjects.Add(global::Studio.Studio.GetCtrlInfo(node));
                    OnObjectsSelected(selectedObjects);
                }
            }
        }
    }
}
