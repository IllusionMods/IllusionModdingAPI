using System;
using System.Collections.Generic;
using System.Linq;
using BepInEx.Bootstrap;
using KKAPI.Chara;
using KKAPI.Studio;
using KKAPI.Utilities;
using UnityEngine;

namespace KKAPI.MainGame
{
    /// <summary>
    /// Provides API for interfacing with the main game. It is useful mostly in the actual game, but some
    /// functions will work outside of it (for example in FreeH).
    /// </summary>
    public static partial class GameAPI
    {
        private static readonly Dictionary<GameCustomFunctionController, string> _registeredHandlers = new Dictionary<GameCustomFunctionController, string>();

        private static GameObject _functionControllerContainer;

        /// <summary>
        /// Fired after an H scene is loaded. Can be both in the main game and in free h.
        /// Runs immediately after all <see cref="GameCustomFunctionController"/> objects trigger their events.
        /// </summary>
        public static event EventHandler StartH;

        /// <summary>
        /// Fired after an H scene is ended, but before it is unloaded. Can be both in the main game and in free h.
        /// Runs immediately after all <see cref="GameCustomFunctionController"/> objects trigger their events.
        /// </summary>
        public static event EventHandler EndH;

        /// <summary>
        /// Fired right after a game save is succesfully loaded.
        /// Runs immediately after all <see cref="GameCustomFunctionController"/> objects trigger their events.
        /// </summary>
        public static event EventHandler<GameSaveLoadEventArgs> GameLoad;

        /// <summary>
        /// Fired right before the game state is saved to file.
        /// Runs immediately after all <see cref="GameCustomFunctionController"/> objects trigger their events.
        /// </summary>
        public static event EventHandler<GameSaveLoadEventArgs> GameSave;

        /// <summary>
        /// True if any sort of H scene is currently loaded.
        /// </summary>
        public static bool InsideHScene { get; private set; }

        /// <summary>
        /// True if the game is in process of being saved.
        /// </summary>
        public static bool GameBeingSaved { get; private set; }

        /// <summary>
        /// Get all registered behaviours for the game.
        /// </summary>
        public static IEnumerable<GameCustomFunctionController> GetBehaviours()
        {
            return _registeredHandlers.Keys.AsEnumerable();
        }

        /// <summary>
        /// Get the first controller that was registered with the specified extendedDataId.
        /// </summary>
        public static GameCustomFunctionController GetRegisteredBehaviour(string extendedDataId)
        {
            return _registeredHandlers
                .Where(registration => string.Equals(registration.Value, extendedDataId, StringComparison.Ordinal))
                .Select(registration => registration.Key).FirstOrDefault();
        }

        /// <summary>
        /// Get the first controller of the specified type that was registered. The type has to be an exact match.
        /// </summary>
        public static GameCustomFunctionController GetRegisteredBehaviour(Type controllerType)
        {
            return _registeredHandlers.Where(registration => registration.Key.GetType() == controllerType)
                .Select(registration => registration.Key).FirstOrDefault();
        }

        /// <summary>
        /// Get the first controller of the specified type that was registered with the specified extendedDataId. The type has to be an exact match.
        /// </summary>
        public static GameCustomFunctionController GetRegisteredBehaviour(Type controllerType, string extendedDataId)
        {
            return _registeredHandlers
                .Where(registration => string.Equals(registration.Value, extendedDataId, StringComparison.Ordinal) &&
                                       registration.Key.GetType() == controllerType)
                .Select(registration => registration.Key).FirstOrDefault();
        }

        /// <summary>
        /// Register new functionality that will be added to main game. Offers easy API for custom main game logic.
        /// All you have to do is create a type that inherits from <see cref="GameCustomFunctionController"/>>
        /// (don't make instances, the API will make them for you). Warning: The custom controller is immediately
        /// created when it's registered, but its OnGameLoad method is not called until a game actually loads.
        /// This might mean that if the registration happens too late you will potentially miss some load events.
        /// </summary>
        /// <typeparam name="T">Type with your custom logic to add to a character</typeparam>
        /// <param name="extendedDataId">Extended data ID used by this behaviour. Set to null if not used.</param>
        public static void RegisterExtraBehaviour<T>(string extendedDataId) where T : GameCustomFunctionController, new()
        {
            if (StudioAPI.InsideStudio) return;

            var newBehaviour = _functionControllerContainer.AddComponent<T>();
            newBehaviour.ExtendedDataId = extendedDataId;
            _registeredHandlers.Add(newBehaviour, extendedDataId);
        }

        internal static void Init(bool insideStudio)
        {
            if (insideStudio) return;

            Hooks.SetupHooks();

            _functionControllerContainer = new GameObject("GameCustomFunctionController Zoo");
            _functionControllerContainer.transform.SetParent(Chainloader.ManagerObject.transform, false);

            if (KoikatuAPI.EnableDebugLogging)
                RegisterExtraBehaviour<TestGameFunctionController>(null);
        }

        private static void OnGameBeingLoaded(string path, string fileName)
        {
            var eLogger = ApiEventExecutionLogger.GetEventLogger();
            eLogger.Begin(nameof(OnGameBeingLoaded), fileName);

            var args = new GameSaveLoadEventArgs(path, fileName);
            foreach (var behaviour in _registeredHandlers)
            {
                eLogger.PluginStart();
                try
                {
                    behaviour.Key.OnGameLoad(args);
                }
                catch (Exception e)
                {
                    KoikatuAPI.Logger.LogError(e);
                }
                eLogger.PluginEnd(behaviour.Key);
            }

            GameLoad.SafeInvokeWithLogging(handler => handler.Invoke(KoikatuAPI.Instance, args), nameof(GameLoad), eLogger);

            eLogger.End();
        }

        private static void OnGameBeingSaved(string path, string fileName)
        {
            var eLogger = ApiEventExecutionLogger.GetEventLogger();
            eLogger.Begin(nameof(OnGameBeingSaved), fileName);

            var args = new GameSaveLoadEventArgs(path, fileName);
            foreach (var behaviour in _registeredHandlers)
            {
                eLogger.PluginStart();
                try
                {
                    behaviour.Key.OnGameSave(args);
                }
                catch (Exception e)
                {
                    KoikatuAPI.Logger.LogError(e);
                }
                eLogger.PluginEnd(behaviour.Key);
            }

            GameSave.SafeInvokeWithLogging(handler => handler.Invoke(KoikatuAPI.Instance, args), nameof(GameSave), eLogger);
            
            eLogger.End();
        }

        private static void OnHEnd(HScene proc)
        {
            var eLogger = ApiEventExecutionLogger.GetEventLogger();
            eLogger.Begin(nameof(OnHEnd), null);

            foreach (var behaviour in _registeredHandlers)
            {
                eLogger.PluginStart();
                try
                {
                    behaviour.Key.OnEndH(proc, false);
                }
                catch (Exception e)
                {
                    KoikatuAPI.Logger.LogError(e);
                }
                eLogger.PluginEnd(behaviour.Key);
            }

            EndH.SafeInvokeWithLogging(handler => handler.Invoke(KoikatuAPI.Instance, EventArgs.Empty), nameof(EndH), eLogger);
            
            InsideHScene = false;

            eLogger.End();
        }

        private static void OnHStart(HScene proc)
        {
            var eLogger = ApiEventExecutionLogger.GetEventLogger();
            eLogger.Begin(nameof(OnHStart), null);

            InsideHScene = true;
            foreach (var behaviour in _registeredHandlers)
            {
                eLogger.PluginStart();
                try
                {
                    behaviour.Key.OnStartH(proc, false);
                }
                catch (Exception e)
                {
                    KoikatuAPI.Logger.LogError(e);
                }
                eLogger.PluginEnd(behaviour.Key);
            }

            StartH.SafeInvokeWithLogging(handler => handler.Invoke(KoikatuAPI.Instance, EventArgs.Empty), nameof(StartH), eLogger);

            eLogger.End();
        }

        private static void OnDayChange(int day)
        {
            var eLogger = ApiEventExecutionLogger.GetEventLogger();
            eLogger.Begin(nameof(OnDayChange), day.ToString());

            foreach (var behaviour in _registeredHandlers)
            {
                eLogger.PluginStart();
                try
                {
                    behaviour.Key.OnDayChange(day);
                }
                catch (Exception e)
                {
                    KoikatuAPI.Logger.LogError(e);
                }
                eLogger.PluginEnd(behaviour.Key);
            }

            eLogger.End();
        }

        private static void OnPeriodChange(AIProject.TimeZone period)
        {
            var eLogger = ApiEventExecutionLogger.GetEventLogger();
            eLogger.Begin(nameof(OnPeriodChange), period.ToString());

            foreach (var behaviour in _registeredHandlers)
            {
                eLogger.PluginStart();
                try
                {
                    behaviour.Key.OnPeriodChange(period);
                }
                catch (Exception e)
                {
                    KoikatuAPI.Logger.LogError(e);
                }
                eLogger.PluginEnd(behaviour.Key);
            }

            eLogger.End();
        }

        private static void OnNewGame()
        {
            var eLogger = ApiEventExecutionLogger.GetEventLogger();
            eLogger.Begin(nameof(OnNewGame), null);

            foreach (var behaviour in _registeredHandlers)
            {
                eLogger.PluginStart();
                try
                {
                    behaviour.Key.OnNewGame();
                }
                catch (Exception e)
                {
                    KoikatuAPI.Logger.LogError(e);
                }
                eLogger.PluginEnd(behaviour.Key);
            }

            eLogger.End();
        }
    }
}
