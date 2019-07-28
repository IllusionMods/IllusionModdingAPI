using System;
using System.Linq;
using System.Threading;
using BepInEx.Logging;
using UnityEngine;

namespace KKAPI.Utilities
{
    /// <summary>
    /// Provides methods for running code on other threads and synchronizing with the main thread.
    /// </summary>
    public sealed class ThreadingHelper : MonoBehaviour
    {
        private static readonly object _invokeLock = new object();
        private static Action _invokeList;

        /// <summary>
        /// Queue the delegate to be invoked on the main unity thread. Use to synchronize your threads.
        /// </summary>
        public static void StartSyncInvoke(Action action)
        {
            if (action == null) throw new ArgumentNullException(nameof(action));

            lock (_invokeLock) _invokeList += action;
        }

        private void Update()
        {
            // Safe to do outside of lock because nothing can remove callbacks, at worst we execute with 1 frame delay
            if (_invokeList == null) return;

            Action toRun;
            lock (_invokeLock)
            {
                toRun = _invokeList;
                _invokeList = null;
            }

            // Need to execute outside of the lock in case the callback itself calls Invoke we could deadlock
            // The invocation would also block any threads that call Invoke
            foreach (var action in toRun.GetInvocationList().Cast<Action>())
            {
                try
                {
                    action();
                }
                catch (Exception ex)
                {
                    LogInvocationException(ex);
                }
            }
        }

        /// <summary>
        /// Queue the delegate to be invoked on a background thread. Use this to run slow tasks without affecting the game.
        /// NOTE: Most of Unity API can not be accessed while running on another thread!
        /// </summary>
        /// <param name="action">
        /// Task to be executed on another thread. Can optionally return an Action that will be executed on the main thread.
        /// You can use this action to return results of your work safely. Return null if this is not needed.
        /// </param>
        public static void StartAsyncInvoke(Func<Action> action)
        {
            void DoWork(object _)
            {
                try
                {
                    var result = action();

                    if (result != null)
                        StartSyncInvoke(result);
                }
                catch (Exception ex)
                {
                    LogInvocationException(ex);
                }
            }

            if (!ThreadPool.QueueUserWorkItem(DoWork))
                throw new NotSupportedException("Failed to queue the action on ThreadPool");
        }

        private static void LogInvocationException(Exception ex)
        {
            KoikatuAPI.Log(LogLevel.Error, ex);
            if (ex.InnerException != null) KoikatuAPI.Log(LogLevel.Error, "INNER: " + ex.InnerException);
        }
    }
}