using System;
using System.Collections;
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
    }
}
