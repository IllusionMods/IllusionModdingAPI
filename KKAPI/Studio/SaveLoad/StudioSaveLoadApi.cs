using System;
using System.Collections.Generic;
using System.Linq;
using BepInEx.Logging;
using KKAPI.Utilities;
using Studio;
using UnityEngine;
using Logger = BepInEx.Logger;

namespace KKAPI.Studio.SaveLoad
{
    /// <summary>
    /// Provides API for loading and saving scenes, as well as a convenient way for registering custom studio functions.
    /// </summary>
    public static partial class StudioSaveLoadApi
    {
        internal static void Init(bool insideStudio)
        {
            if (!insideStudio) return;

            Hooks.SetupHooks();
            ExtensibleSaveFormat.ExtendedSave.SceneBeingSaved += path => OnSceneBeingSaved();

            _functionControllerContainer = new GameObject("SceneCustomFunctionController Zoo");
            _functionControllerContainer.transform.SetParent(BepInEx.Bootstrap.Chainloader.ManagerObject.transform, false);

#if DEBUG
            RegisterExtraBehaviour<TestSceneFunctionController>(null);
#endif
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
            foreach (var behaviour in _registeredHandlers)
            {
                try
                {
                    behaviour.Key.OnSceneSave();
                }
                catch (Exception e)
                {
                    Logger.Log(LogLevel.Error, e);
                }
            }

            try
            {
                SceneSave?.Invoke(KoikatuAPI.Instance, EventArgs.Empty);
            }
            catch (Exception e)
            {
                Logger.Log(LogLevel.Error, e);
            }
        }

        private static void OnSceneBeingLoaded(SceneOperationKind operation)
        {
            var readonlyDict = GetLoadedObjects(operation).ToReadOnlyDictionary();

            foreach (var behaviour in _registeredHandlers)
            {
                try
                {
                    behaviour.Key.OnSceneLoad(operation, readonlyDict);
                }
                catch (Exception e)
                {
                    Logger.Log(LogLevel.Error, e);
                }
            }

            try
            {
                SceneLoad?.Invoke(KoikatuAPI.Instance, new SceneLoadEventArgs(operation, readonlyDict));
            }
            catch (Exception e)
            {
                Logger.Log(LogLevel.Error, e);
            }
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
        /// A scene is currently being imported
        /// </summary>
        public static bool ImportInProgress { get; private set; }

        /// <summary>
        /// A scene is currently being loaded (not imported or cleared)
        /// </summary>
        public static bool LoadInProgress { get; private set; }
    }
}