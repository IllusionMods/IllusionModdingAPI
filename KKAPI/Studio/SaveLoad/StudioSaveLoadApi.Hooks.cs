using System.Collections;
using System.Collections.Generic;
using System.Reflection.Emit;
using ExtensibleSaveFormat;
using Harmony;
using Studio;

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
                HarmonyInstance.Create(typeof(Hooks).FullName).PatchAll(typeof(Hooks));
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
            public static IEnumerable<CodeInstruction> InitBaseCustomTextureBodyTranspiler(IEnumerable<CodeInstruction> instructions)
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
            }

            [HarmonyPrefix]
            [HarmonyPatch(typeof(global::Studio.Studio), nameof(global::Studio.Studio.ImportScene))]
            public static void ImportScenePrefix()
            {
                ImportDictionary.Clear();
                ImportInProgress = true;
            }

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
        }
    }
}
