using System;
using System.Collections;
using System.Collections.Generic;
using ActionGame;
using BepInEx.Bootstrap;
using BepInEx.Logging;
using UnityEngine;
using UnityEngine.SceneManagement;
using Logger = BepInEx.Logger;

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
            if (_functionControllerContainer != null)
            {
                var newBehaviour = _functionControllerContainer.AddComponent<T>();
                newBehaviour.ExtendedDataId = extendedDataId;
                _registeredHandlers.Add(newBehaviour, extendedDataId);
            }
        }

        /// <summary>
        /// Fired after an H scene is loaded. Can be both in the main game and in free h.
        /// Runs immediately after all <see cref="GameCustomFunctionController"/> objects trigger their events.
        /// </summary>
        public static event EventHandler StartH;

        internal static void Init(bool insideStudio)
        {
            if (insideStudio) return;

            Hooks.SetupHooks();

            _functionControllerContainer = new GameObject("GameCustomFunctionController Zoo");
            _functionControllerContainer.transform.SetParent(Chainloader.ManagerObject.transform, false);

            SceneManager.sceneLoaded += (arg0, mode) =>
            {
                if (arg0.name == "MyRoom" && Manager.Scene.Instance.LoadSceneName != "H")
                {
                    foreach (var registeredHandler in _registeredHandlers)
                    {
                        try
                        {
                            registeredHandler.Key.OnEnterNightMenu();
                        }
                        catch (Exception e)
                        {
                            Logger.Log(LogLevel.Error, e);
                        }
                    }
                }
            };

            if (KoikatuAPI.EnableDebugLogging)
                RegisterExtraBehaviour<TestGameFunctionController>(null);
        }

        private static void OnGameBeingLoaded(string path, string fileName)
        {
            var args = new GameSaveLoadEventArgs(path, fileName);
            foreach (var behaviour in _registeredHandlers)
            {
                try
                {
                    behaviour.Key.OnGameLoad(args);
                }
                catch (Exception e)
                {
                    Logger.Log(LogLevel.Error, e);
                }
            }

            try
            {
                GameLoad?.Invoke(KoikatuAPI.Instance, args);
            }
            catch (Exception e)
            {
                Logger.Log(LogLevel.Error, e);
            }
        }

        private static void OnGameBeingSaved(string path, string fileName)
        {
            var args = new GameSaveLoadEventArgs(path, fileName);
            foreach (var behaviour in _registeredHandlers)
            {
                try
                {
                    behaviour.Key.OnGameSave(args);
                }
                catch (Exception e)
                {
                    Logger.Log(LogLevel.Error, e);
                }
            }

            try
            {
                GameSave?.Invoke(KoikatuAPI.Instance, args);
            }
            catch (Exception e)
            {
                Logger.Log(LogLevel.Error, e);
            }
        }

        private static void OnHEnd(HSceneProc proc)
        {
            foreach (var behaviour in _registeredHandlers)
            {
                try
                {
                    behaviour.Key.OnEndH(proc, proc.flags.isFreeH);
                }
                catch (Exception e)
                {
                    Logger.Log(LogLevel.Error, e);
                }
            }

            try
            {
                EndH?.Invoke(KoikatuAPI.Instance, EventArgs.Empty);
            }
            catch (Exception e)
            {
                Logger.Log(LogLevel.Error, e);
            }
        }

        private static IEnumerator OnHStart(HSceneProc proc)
        {
            yield return null;
            foreach (var behaviour in _registeredHandlers)
            {
                try
                {
                    behaviour.Key.OnStartH(proc, proc.flags.isFreeH);
                }
                catch (Exception e)
                {
                    Logger.Log(LogLevel.Error, e);
                }
            }

            try
            {
                StartH?.Invoke(KoikatuAPI.Instance, EventArgs.Empty);
            }
            catch (Exception e)
            {
                Logger.Log(LogLevel.Error, e);
            }
        }

        private static void OnDayChange(Cycle.Week day)
        {
            foreach (var behaviour in _registeredHandlers)
            {
                try
                {
                    behaviour.Key.OnDayChange(day);
                }
                catch (Exception e)
                {
                    Logger.Log(LogLevel.Error, e);
                }
            }
        }

        private static void OnPeriodChange(Cycle.Type period)
        {
            foreach (var behaviour in _registeredHandlers)
            {
                try
                {
                    behaviour.Key.OnPeriodChange(period);
                }
                catch (Exception e)
                {
                    Logger.Log(LogLevel.Error, e);
                }
            }
        }
    }
}
