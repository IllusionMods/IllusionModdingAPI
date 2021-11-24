using System;
using System.Collections;
using System.Reflection;
using HarmonyLib;
using Mono.Cecil;
using MonoMod.Cil;
using MonoMod.Utils;
using UnityEngine;

namespace KKAPI.Utilities
{
    /// <summary>
    /// Utility methods for working with coroutines.
    /// </summary>
    public static class CoroutineUtils
    {
        /// <summary>
        /// Create a coroutine that calls the appendCoroutine after baseCoroutine finishes
        /// </summary>
        public static IEnumerator AppendCo(this IEnumerator baseCoroutine, IEnumerator appendCoroutine)
        {
            return ComposeCoroutine(baseCoroutine, appendCoroutine);
        }

        /// <summary>
        /// Create a coroutine that calls the yieldInstruction after baseCoroutine finishes.
        /// Useless on its own, append further coroutines to run after this.
        /// </summary>
        public static IEnumerator AppendCo(this IEnumerator baseCoroutine, YieldInstruction yieldInstruction)
        {
            return new object[] { baseCoroutine, yieldInstruction }.GetEnumerator();
        }

        /// <summary>
        /// Create a coroutine that calls each of the actions in order after base coroutine finishes.
        /// One action is called per frame. First action is called right after the coroutine finishes.
        /// </summary>
        public static IEnumerator AppendCo(this IEnumerator baseCoroutine, params Action[] actions)
        {
            return ComposeCoroutine(baseCoroutine, CreateCoroutine(actions));
        }

        /// <summary>
        /// Create a coroutine that calls each of the action delegates on consecutive frames.
        /// One action is called per frame. First action is called right away. There is no frame skip after the last action.
        /// </summary>
        public static IEnumerator CreateCoroutine(params Action[] actions)
        {
            if (actions == null) throw new ArgumentNullException(nameof(actions));

            var first = true;
            foreach (var action in actions)
            {
                if (first)
                    first = false;
                else
                    yield return null;

                action();
            }
        }

        /// <summary>
        /// Create a coroutine that calls each of the action delegates on consecutive frames.
        /// One action is called per frame. First action is called right after the yieldInstruction. There is no frame skip after the last action.
        /// </summary>
        public static IEnumerator CreateCoroutine(YieldInstruction yieldInstruction, params Action[] actions)
        {
            if (yieldInstruction == null) throw new ArgumentNullException(nameof(yieldInstruction));
            if (actions == null) throw new ArgumentNullException(nameof(actions));

            yield return yieldInstruction;
            yield return CreateCoroutine(actions);
        }

        /// <summary>
        /// Create a coroutine that calls each of the supplied coroutines in order.
        /// </summary>
        public static IEnumerator ComposeCoroutine(params IEnumerator[] coroutine)
        {
            return coroutine.GetEnumerator();
        }

        /// <summary>
        /// Create a coroutine that is the same as the supplied coroutine, except every time it yields the onYieldAction is invoked.
        /// (i.e. onYieldAction is invoked after every yield return in the original coroutine)
        /// If the coroutine returns another coroutine, the action is not called for yields performed by the returned coroutine, only the topmost one. Use FlattenCo if that's an issue.
        /// </summary>
        public static IEnumerator AttachToYield(this IEnumerator coroutine, Action onYieldAction)
        {
            if (coroutine == null) throw new ArgumentNullException(nameof(coroutine));
            if (onYieldAction == null) throw new ArgumentNullException(nameof(onYieldAction));

            while (coroutine.MoveNext())
            {
                onYieldAction();
                yield return coroutine.Current;
            }
        }

        /// <summary>
        /// Flatten the coroutine to yield all values directly. Any coroutines yield returned by this coroutine will have their values directly returned by this new coroutine (this is recursive).
        /// For example if another coroutine is yielded by this coroutine, the yielded coroutine will not be returned and instead the values that it yields will be returned.
        /// If a yielded coroutine yields yet another coroutine, that second coroutine's values will be returned directly from the flattened coroutine.
        /// </summary>
        public static IEnumerator FlattenCo(this IEnumerator coroutine)
        {
            if (coroutine == null) throw new ArgumentNullException(nameof(coroutine));

            while (coroutine.MoveNext())
            {
                var current = coroutine.Current;

                if (current is IEnumerator subCo)
                {
                    var flattenedSubCo = FlattenCo(subCo);
                    while (flattenedSubCo.MoveNext())
                        yield return flattenedSubCo.Current;

                    continue;
                }

                yield return current;
            }
        }

        /// <summary>
        /// Remove yields from the coroutine, making its code run immediately.
        /// </summary>
        /// <param name="coroutine">Coroutine to strip</param>
        /// <param name="onlyStripNulls">Should only yield return null be stripped? If false, all yields are stripped</param>
        /// <param name="flatten">
        /// Should the coroutine be flattened before stripping it? 
        /// If this is false then yields from coroutines returned by this coroutine will not be stripped. 
        /// If this and onlyStripNulls are both false, coroutines returned by this coroutine will not be executed.
        /// </param>
        public static IEnumerator StripYields(this IEnumerator coroutine, bool onlyStripNulls = true, bool flatten = true)
        {
            if (coroutine == null) throw new ArgumentNullException(nameof(coroutine));

            if (flatten)
                coroutine = FlattenCo(coroutine);

            while (coroutine.MoveNext())
            {
                if (onlyStripNulls)
                {
                    var current = coroutine.Current;
                    if (current != null) yield return current;
                }
            }
        }

        /// <summary>
        /// Fully executes the coroutine synchronously (immediately run all of its code till completion).
        /// </summary>
        public static void RunImmediately(this IEnumerator coroutine)
        {
            coroutine = FlattenCo(coroutine);
            while (coroutine.MoveNext()) { }
        }

        /// <summary>
        /// Prevent a coroutine from getting stopped by exceptions. Exceptions are caught and logged.
        /// Code after the exception is thrown doesn't run up until the next yield. The coroutine continues after the yield then.
        /// </summary>
        public static IEnumerator PreventFromCrashing(this IEnumerator coroutine)
        {
            if (coroutine == null)
                throw new ArgumentNullException(nameof(coroutine));
            while (true)
            {
                try
                {
                    if (!coroutine.MoveNext())
                        break;
                }
                catch (Exception ex)
                {
                    UnityEngine.Debug.LogException(ex);
                    break;
                }
                yield return coroutine.Current;
            }
        }

        /// <summary>
        /// Find the compiler-generated MoveNext method that contains the Coroutine/UniTask code. It can be used to apply transpliers to Coroutines and UniTasks.
        /// Note: When writing transpliers for coroutines you might want to turn off the "Decompiler\Decompile enumerators" setting in DnSpy so that you can see the real code.
        /// UniTasks are considered "async/await" so you need to turn off the "Decompile async methods" setting instead.
        /// </summary>
        public static MethodInfo GetMoveNext(MethodBase targetMethod)
        {
            var ctx = new ILContext(new DynamicMethodDefinition(targetMethod).Definition);
            var il = new ILCursor(ctx);

            Type enumeratorType;
#if !PH && !KK
            if (il.Method.ReturnType.Name.StartsWith("UniTask"))
            {
                enumeratorType = il.Body.Variables[0].VariableType.ResolveReflection();
                if (!enumeratorType.Name.Contains(targetMethod.Name))
                    throw new ArgumentException($"Unexpected type name {enumeratorType.Name}, should contain {targetMethod.Name}");
            }
            else
#endif
            {
                MethodReference enumeratorCtor = null;
                il.GotoNext(instruction => instruction.MatchNewobj(out enumeratorCtor));
                if (enumeratorCtor == null) throw new ArgumentNullException(nameof(enumeratorCtor));
                if (enumeratorCtor.Name != ".ctor")
                    throw new ArgumentException($"Unexpected method name {enumeratorCtor.Name}, should be .ctor", nameof(enumeratorCtor));

                enumeratorType = enumeratorCtor.DeclaringType.ResolveReflection();
            }

            var movenext = enumeratorType.GetMethod("MoveNext", AccessTools.all);
            if (movenext == null) throw new ArgumentNullException(nameof(movenext));
            
            KoikatuAPI.Logger.LogDebug($"GetMoveNext found [{movenext.FullDescription()}] for [{targetMethod.FullDescription()}]");
            
            return movenext;
        }

        /// <summary>
        /// Use to patch coroutines/yielding methods.
        /// This will method automatically find the compiler-generated MoveNext method that contains the coroutine code and apply patches on that. The method you patch must return an IEnumerator.
        /// Warning: Postfix patches will not work as expected, they might be fired after every iteration. Prefix is practically the same as prefixing the entry method. It's best to only use transpliers with this method.
        /// Note: When writing transpliers for coroutines you might want to turn off the "Decompiler\DecoDecompile enumerators" setting in DnSpy so that you can see the real code.
        /// </summary>
        /// <inheritdoc cref="Harmony.Patch(MethodBase,HarmonyMethod,HarmonyMethod,HarmonyMethod,HarmonyMethod,HarmonyMethod)"/>
        public static MethodInfo PatchMoveNext(this Harmony harmonyInstance,
            MethodBase original,
            HarmonyMethod prefix = null,
            HarmonyMethod postfix = null,
            HarmonyMethod transpiler = null,
            HarmonyMethod finalizer = null,
            HarmonyMethod ilmanipulator = null)
        {
            var moveNext = GetMoveNext(original);
            return harmonyInstance.Patch(moveNext, prefix, postfix, transpiler, finalizer, ilmanipulator);
        }

        /// <summary>
        /// Cached WaitForEndOfFrame. Use instead of creating a new instance every time to reduce garbage production.
        /// </summary>
        public static readonly WaitForEndOfFrame WaitForEndOfFrame = new WaitForEndOfFrame();

        /// <summary>
        /// Create a coroutine that is the same as the supplied coroutine, but will stop early if <see cref="KoikatuAPI.IsQuitting"/> is <c>true</c>.
        /// If the coroutine returns another coroutine, the <see cref="KoikatuAPI.IsQuitting"/> check only runs on the topmost one. Use <see cref="FlattenCo" /> if that's an issue.
        /// </summary>
        public static IEnumerator StopCoOnQuit(this IEnumerator enumerator)
        {
            while (!KoikatuAPI.IsQuitting && enumerator.MoveNext())
            {
                yield return enumerator.Current;
            }
        }
    }
}
