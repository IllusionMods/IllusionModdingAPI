using KKAPI.Utilities;
using Studio;
using System;
using System.Collections.Generic;
using System.Linq;
using KKAPI.Chara;
using UnityEngine;

namespace KKAPI.Studio.SaveLoad
{
    /// <summary>
    /// Provides API for loading and saving scenes, as well as a convenient way for registering custom studio functions.
    /// </summary>
    public static partial class StudioSaveLoadApi
    {
        internal static void Init()
        {
            Hooks.SetupHooks();
            ExtensibleSaveFormat.ExtendedSave.SceneBeingSaved += path => OnSceneBeingSaved();

            _functionControllerContainer = new GameObject("SceneCustomFunctionController Zoo");
            _functionControllerContainer.transform.SetParent(BepInEx.Bootstrap.Chainloader.ManagerObject.transform, false);

            if (KoikatuAPI.EnableDebugLogging)
                RegisterExtraBehaviour<TestSceneFunctionController>(null);
        }

        private static GameObject _functionControllerContainer;

        private static readonly Dictionary<SceneCustomFunctionController, string> _registeredHandlers = new Dictionary<SceneCustomFunctionController, string>();

        /// <summary>
        /// Register new functionality that will be added to studio. Offers easy API for saving and loading extended data.
        /// All necessary hooking and event subscribing is done for you. Importing scenes is also handled for you.
        /// All you have to do is create a type that inherits from <see cref="SceneCustomFunctionController"/>>
        /// (don't make instances, the API will make them for you). Warning: The custom controller is immediately
        /// created when it's registered, but its OnSceneLoad method is not called until a scene actually loads.
        /// This might mean that if the registration happens too late you will potentially miss some load events. 
        /// </summary>
        /// <typeparam name="T">Type with your custom logic to add to a character</typeparam>
        /// <param name="extendedDataId">Extended data ID used by this behaviour. Set to null if not used.</param>
        public static void RegisterExtraBehaviour<T>(string extendedDataId) where T : SceneCustomFunctionController, new()
        {
            if (_functionControllerContainer != null)
            {
                var newBehaviour = _functionControllerContainer.AddComponent<T>();
                newBehaviour.ExtendedDataId = extendedDataId;
                _registeredHandlers.Add(newBehaviour, extendedDataId);
            }
        }

        private static void OnSceneBeingSaved()
        {
            var eLogger = ApiEventExecutionLogger.GetEventLogger();
            eLogger.Begin(nameof(OnSceneBeingSaved), null);

            foreach (var behaviour in _registeredHandlers)
            {
                eLogger.PluginStart();
                try
                {
                    behaviour.Key.OnSceneSave();
                }
                catch (Exception e)
                {
                    KoikatuAPI.Logger.LogError(e);
                }
                eLogger.PluginEnd(behaviour.Key);
            }

            SceneSave.SafeInvokeWithLogging(handler => handler.Invoke(KoikatuAPI.Instance, EventArgs.Empty), nameof(SceneSave), eLogger);

            eLogger.End();
        }

        private static void OnSceneBeingLoaded(SceneOperationKind operation)
        {
            var eLogger = ApiEventExecutionLogger.GetEventLogger();
            eLogger.Begin(nameof(OnSceneBeingLoaded), operation.ToString());

            var readonlyDict = GetLoadedObjects(operation).ToReadOnlyDictionary();

            foreach (var behaviour in _registeredHandlers)
            {
                eLogger.PluginStart();
                try
                {
                    behaviour.Key.OnSceneLoad(operation, readonlyDict);
                }
                catch (Exception e)
                {
                    KoikatuAPI.Logger.LogError(e);
                }
                eLogger.PluginEnd(behaviour.Key);
            }

            var args = new SceneLoadEventArgs(operation, readonlyDict);
            SceneLoad.SafeInvokeWithLogging(handler => handler.Invoke(KoikatuAPI.Instance, args), nameof(SceneLoad), eLogger);

            eLogger.End();
        }

        private static void OnObjectsBeingCopied()
        {
            var eLogger = ApiEventExecutionLogger.GetEventLogger();
            eLogger.Begin(nameof(OnObjectsBeingCopied), null);

            var readonlyDict = GetLoadedObjects(SceneOperationKind.Import).ToReadOnlyDictionary();

            foreach (var behaviour in _registeredHandlers)
            {
                eLogger.PluginStart();
                try
                {
                    behaviour.Key.OnObjectsCopied(readonlyDict);
                }
                catch (Exception e)
                {
                    KoikatuAPI.Logger.LogError(e);
                }
                eLogger.PluginEnd(behaviour.Key);
            }

            var args = new ObjectsCopiedEventArgs(readonlyDict);
            ObjectsCopied.SafeInvokeWithLogging(handler => handler.Invoke(KoikatuAPI.Instance, args), nameof(ObjectsCopied), eLogger);

            eLogger.End();
        }

        private static void OnObjectBeingDeleted(ObjectCtrlInfo objectCtrlInfo)
        {
            var eLogger = ApiEventExecutionLogger.GetEventLogger();
            eLogger.Begin(nameof(OnObjectBeingDeleted), objectCtrlInfo?.treeNodeObject?.textName);

            foreach (var behaviour in _registeredHandlers)
            {
                eLogger.PluginStart();
                try
                {
                    behaviour.Key.OnObjectDeleted(objectCtrlInfo);
                }
                catch (Exception e)
                {
                    KoikatuAPI.Logger.LogError(e);
                }
                eLogger.PluginEnd(behaviour.Key);
            }

            var args = new ObjectDeletedEventArgs(objectCtrlInfo);
            ObjectDeleted.SafeInvokeWithLogging(handler => handler.Invoke(KoikatuAPI.Instance, args), nameof(ObjectDeleted), eLogger);

            eLogger.End();
        }

        private static void OnObjectVisibilityToggled(ObjectCtrlInfo objectCtrlInfo, bool visible)
        {
            var eLogger = ApiEventExecutionLogger.GetEventLogger();
            eLogger.Begin(nameof(OnObjectVisibilityToggled), objectCtrlInfo?.treeNodeObject?.textName);

            foreach (var behaviour in _registeredHandlers)
            {
                eLogger.PluginStart();
                try
                {
                    behaviour.Key.OnObjectVisibilityToggled(objectCtrlInfo, visible);
                }
                catch (Exception e)
                {
                    KoikatuAPI.Logger.LogError(e);
                }
                eLogger.PluginEnd(behaviour.Key);
            }

            var args = new ObjectVisibilityToggledEventArgs(objectCtrlInfo, visible);
            ObjectVisibilityToggled.SafeInvokeWithLogging(handler => handler.Invoke(KoikatuAPI.Instance, args), nameof(ObjectVisibilityToggled), eLogger);

            eLogger.End();
        }

        private static void OnObjectsSelected(List<ObjectCtrlInfo> objectCtrlInfos)
        {
            var eLogger = ApiEventExecutionLogger.GetEventLogger();
            eLogger.Begin(nameof(OnObjectsSelected), null);

            foreach (var behaviour in _registeredHandlers)
            {
                eLogger.PluginStart();
                try
                {
                    behaviour.Key.OnObjectsSelected(objectCtrlInfos);
                }
                catch (Exception e)
                {
                    KoikatuAPI.Logger.LogError(e);
                }
                eLogger.PluginEnd(behaviour.Key);
            }

            var args = new ObjectsSelectedEventArgs(objectCtrlInfos);
            ObjectsSelected.SafeInvokeWithLogging(handler => handler.Invoke(KoikatuAPI.Instance, args), nameof(ObjectsSelected), eLogger);
            
            eLogger.End();
        }

        private static Dictionary<int, ObjectCtrlInfo> GetLoadedObjects(SceneOperationKind operation)
        {
            Dictionary<int, ObjectCtrlInfo> results;
            switch (operation)
            {
                case SceneOperationKind.Load:
                    results = global::Studio.Studio.Instance.dicObjectCtrl;
                    break;
                case SceneOperationKind.Import:
                    results = global::Studio.Studio.Instance.dicObjectCtrl
                        .Join(Hooks.ImportDictionary, pair => pair.Key, pair => pair.Key, (current, idLookup) => new { originalId = idLookup.Value, obj = current.Value })
                        .ToDictionary(x => x.originalId, x => x.obj);
                    break;
                case SceneOperationKind.Clear:
                    results = new Dictionary<int, ObjectCtrlInfo>();
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(operation), operation, null);
            }

            return results;
        }

        /// <summary>
        /// Fired right after a scene is succesfully imported, loaded or cleared. 
        /// Runs immediately after all <see cref="SceneCustomFunctionController"/> objects trigger their events.
        /// </summary>
        public static event EventHandler<SceneLoadEventArgs> SceneLoad;

        /// <summary>
        /// Fired right before a scene is saved to file. 
        /// Runs immediately after all <see cref="SceneCustomFunctionController"/> objects trigger their events.
        /// </summary>
        public static event EventHandler SceneSave;

        /// <summary>
        /// Fired when objects in the scene are copied
        /// </summary>
        public static event EventHandler<ObjectsCopiedEventArgs> ObjectsCopied;

        /// <summary>
        /// Fired when an object in the scene is being deleted
        /// </summary>
        public static event EventHandler<ObjectDeletedEventArgs> ObjectDeleted;

        /// <summary>
        /// Fired when an object in the scene had its visibility toggled
        /// </summary>
        public static event EventHandler<ObjectVisibilityToggledEventArgs> ObjectVisibilityToggled;

        /// <summary>
        /// Fired when an object in the scene is selected
        /// </summary>
        public static event EventHandler<ObjectsSelectedEventArgs> ObjectsSelected;

        /// <summary>
        /// A scene is currently being imported
        /// </summary>
        public static bool ImportInProgress { get; private set; }

        /// <summary>
        /// A scene is currently being loaded (not imported or cleared)
        /// </summary>
        public static bool LoadInProgress { get; private set; }
    }
}
