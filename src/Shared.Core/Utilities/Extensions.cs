using System;
using System.Collections.Generic;
using System.Text;
using HarmonyLib;
using KKAPI.Chara;
using UnityEngine;
using UnityEngine.Events;

namespace KKAPI.Utilities
{
    /// <summary>
    /// General utility extensions that don't fit in other categories.
    /// </summary>
    public static class Extensions
    {
        /// <summary>
        /// Wrap this dictionary in a read-only wrapper that will prevent any changes to it. 
        /// Warning: Any reference types inside the dictionary can still be modified.
        /// </summary>
        public static ReadOnlyDictionary<TKey, TValue> ToReadOnlyDictionary<TKey, TValue>(this IDictionary<TKey, TValue> original)
        {
            return new ReadOnlyDictionary<TKey, TValue>(original);
        }

        /// <summary>
        /// Mark GameObject of this Component as ignored by AutoTranslator. Prevents AutoTranslator from trying to translate custom UI elements.
        /// </summary>
        public static void MarkXuaIgnored(this Component target)
        {
            if (target == null) throw new ArgumentNullException(nameof(target));
            target.gameObject.name += "(XUAIGNORE)";
        }

        /// <summary>
        /// Same as RemoveAllListeners but also disables all PersistentListeners.
        /// To avoid frustration always use this instead of RemoveAllListeners, unless you want to keep the PersistentListeners.
        /// </summary>
        public static void ActuallyRemoveAllListeners(this UnityEventBase evt)
        {
            evt.m_Calls.Clear();
            evt.m_Calls.ClearPersistent();
            evt.m_PersistentCalls.Clear();
            evt.m_CallsDirty = false;
        }

        /// <summary>
        /// Attempt to project each element of the sequence into a new form (Select but ignore exceptions).
        /// Exceptions thrown while doing this are ignored and any elements that fail to be converted are silently skipped.
        /// </summary>
        public static IEnumerable<T2> Attempt<T, T2>(this IEnumerable<T> items, Func<T, T2> action)
        {
            return Attempt(items, action, null);
        }

        /// <inheritdoc cref="Attempt{T,T2}(IEnumerable{T},Func{T,T2})"/>
        public static IEnumerable<T2> Attempt<T, T2>(this IEnumerable<T> items, Func<T, T2> action, Action<Exception> onError)
        {
            foreach (var item in items)
            {
                T2 result;
                try
                {
                    result = action(item);
                }
                catch (Exception ex)
                {
                    onError?.Invoke(ex);
                    continue;
                }
                yield return result;
            }
        }

        /// <summary>
        /// Destroy this GameObject. Safe to use on null objects.
        /// </summary>
        /// <param name="self">Object to destroy</param>
        /// <param name="useDestroyImmediate">Use DestroyImmediate instead of Destroy</param>
        /// <param name="detachParent">Set parent of the object to null before destroying it (useful when doing FindInChildren and such immediately after destroying to make sure that this destroyed object isn't included in the results)</param>
        public static void FancyDestroy(this GameObject self, bool useDestroyImmediate = false, bool detachParent = false)
        {
            if (self == null) return;
            if (detachParent) self.transform.SetParent(null);
            if (useDestroyImmediate) UnityEngine.Object.DestroyImmediate(self);
            else UnityEngine.Object.Destroy(self);
        }

        /// <summary>
        /// Get full GameObject "path" to this GameObject.
        /// Example: RootObject\ChildObject1\ChildObject2
        /// </summary>
        public static string GetFullPath(this GameObject self)
        {
            if (self == null) return string.Empty;

            var result = new StringBuilder();
            var first = true;
            var transform = self.transform;
            while (transform != null)
            {
                if (first) first = false;
                else result.Insert(0, "/");
                result.Insert(0, transform.gameObject.name);
                transform = transform.parent;
            }
            return result.ToString();
        }

        /// <summary>
        /// Get full GameObject "path" to this Component.
        /// Example: RootObject\ChildObject1\ChildObject2 [Renderer]
        /// </summary>
        public static string GetFullPath(this Component self)
        {
            if (self == null) return string.Empty;
            return $"{self.gameObject.GetFullPath()} [{self.GetType().Name}]";
        }

        /// <summary>
        /// Get the topmost parent of Transform that this this Component is attached to.
        /// </summary>
        public static Transform GetTopmostParent(this Component src)
        {
            return GetTopmostParent(src.transform);
        }
        /// <summary>
        /// Get the topmost parent of this GameObject.
        /// </summary>
        public static Transform GetTopmostParent(this GameObject src)
        {
            return GetTopmostParent(src.transform);
        }
        /// <summary>
        /// Get the topmost parent of this Transform.
        /// </summary>
        public static Transform GetTopmostParent(this Transform src)
        {
            while (src.parent)
                src = src.parent;
            return src;
        }

        /// <summary>
        /// Return true if the object is a "fake" null (i.e. it was destroyed).
        /// </summary>
        public static bool IsDestroyed(this UnityEngine.Object obj)
        {
            return !ReferenceEquals(obj, null) && !obj;
        }

#if KK || KKS||EC
        /// <summary>
        /// Get value of the aaWeightsBody field
        /// </summary>
        public static AssignedAnotherWeights GetAaWeightsBody(this ChaControl ctrl)
        {
            return ctrl.aaWeightsBody;
        }
#endif

        /// <summary>
        /// Get value of a property through reflection
        /// </summary>
        /// <param name="self">Object that has the property</param>
        /// <param name="name">Name of the property</param>
        /// <param name="value">Value returned by the property</param>
        /// <returns>True if the property exists, flase if it doesn't</returns>
        public static bool GetPropertyValue(this object self, string name, out object value)
        {
            var property = Traverse.Create(self).Property(name);
            if (property.PropertyExists())
            {
                value = property.GetValue();
                return true;
            }
            else
            {
                UnityEngine.Debug.LogWarning($"Property {name} doesn't exist on {self?.GetType().FullName}");
                value = null;
                return false;
            }
        }

        /// <summary>
        /// Set value of a property through reflection
        /// </summary>
        /// <param name="self">Object that has the property</param>
        /// <param name="name">Name of the property</param>
        /// <param name="value">Value to be set to the property</param>
        /// <returns>True if the property exists, flase if it doesn't</returns>
        public static bool SetPropertyValue(this object self, string name, object value)
        {
            return Traverse.Create(self).Property(name).SetValue(value).PropertyExists();
        }

        /// <summary>
        /// Get value of a field through reflection
        /// </summary>
        /// <param name="self">Object that has the field</param>
        /// <param name="name">Name of the field</param>
        /// <param name="value">Value returned by the field</param>
        /// <returns>True if the field exists, flase if it doesn't</returns>
        public static bool GetFieldValue(this object self, string name, out object value)
        {
            var field = Traverse.Create(self).Field(name);
            if (field.FieldExists())
            {
                value = field.GetValue();
                return true;
            }
            else
            {
                UnityEngine.Debug.LogWarning($"Field {name} doesn't exist on {self?.GetType().FullName}");
                value = null;
                return false;
            }
        }

        /// <summary>
        /// Set value of a field through reflection
        /// </summary>
        /// <param name="self">Object that has the field</param>
        /// <param name="name">Name of the property</param>
        /// <param name="value">Value to be set to the field</param>
        /// <returns>True if the field exists, flase if it doesn't</returns>
        public static bool SetFieldValue(this object self, string name, object value)
        {
            return Traverse.Create(self).Field(name).SetValue(value).FieldExists();
        }
        
        internal static void SafeInvoke<T>(this T handler, Action<T> invokeCallback) where T : Delegate
        {
            if (handler == null) return;

            try
            {
                foreach (var singleHandler in handler.GetInvocationList())
                {
                    try
                    {
                        invokeCallback((T)singleHandler);
                    }
                    catch (Exception e)
                    {
                        KoikatuAPI.Logger.LogError("Event handler crashed with exception: " + e);
                    }
                }
            }
            catch (Exception e)
            {
                KoikatuAPI.Logger.LogError("Unexpected crash when running events, some events might have been skipped! Reason: " + e);
            }
        }
        internal static void SafeInvokeWithLogging<T>(this T handler, Action<T> invokeCallback, string handlerName, ApiEventExecutionLogger eventLogger) where T : Delegate
        {
            if (handler == null) return;

            try
            {
                eventLogger?.EventHandlerBegin(handlerName);

                foreach (var singleHandler in handler.GetInvocationList())
                {
                    eventLogger?.EventHandlerStart();
                    try
                    {
                        invokeCallback((T)singleHandler);
                    }
                    catch (Exception e)
                    {
                        KoikatuAPI.Logger.LogError("Event handler crashed with exception: " + e);
                    }
                    eventLogger?.EventHandlerEnd(singleHandler.Method);
                }
            }
            catch (Exception e)
            {
                KoikatuAPI.Logger.LogError("Unexpected crash when running events, some events might have been skipped! Reason: " + e);
            }
        }
    }
}
