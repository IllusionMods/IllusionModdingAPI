using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ActionGame;
using BepInEx.Bootstrap;
using HarmonyLib;
using Illusion.Component;
using KKAPI.Studio;
using UniRx.Triggers;
using UnityEngine;
using UnityEngine.SceneManagement;

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

        // navigating these scenes in order triggers OnNewGame
        private static readonly IList<string> _newGameDetectionScenes =
            new List<string>(new[] { "Title", "EntryPlayer", "ClassRoomSelect", "Action" });

        private static int _newGameDetectionIndex = -1;

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

        /// <summary>
        /// Register a new action icon in roaming mode (like the icons for training/studying, club report screen, peeping).
        /// Icon templates can be found here https://github.com/IllusionMods/IllusionModdingAPI/tree/master/src/KKAPI/MainGame/ActionIcons
        /// </summary>
        /// <param name="mapNo">Identification number of the map the icon should be spawned on</param>
        /// <param name="position">Position of the icon. All default icons are spawned at y=0, but different heights work fine to a degree.
        /// You can figure out the position by walking to it and getting the player position with RUE.</param>
        /// <param name="iconOn">Icon shown when player is in range to click it (excited state).</param>
        /// <param name="iconOff">Icon shown when player is out of range.</param>
        /// <param name="onOpen">Action triggered when player clicks the icon (If you want to open your own menu, use <see cref="GameExtensions.SetIsCursorLock"/>
        /// to enable mouse cursor and hide the action icon to prevent it from being clicked again.).</param>
        /// <param name="onCreated">Optional action to run after the icon is created.
        /// Use to attach extra code to the icon, e.g. by using <see cref="ObservableTriggerExtensions.UpdateAsObservable(Component)"/> and similar methods.</param>
        public static void AddActionIcon(int mapNo, Vector3 position, Sprite iconOn, Sprite iconOff, Action onOpen, Action<TriggerEnterExitEvent> onCreated = null)
        {
            if (StudioAPI.InsideStudio) return;
            CustomActionIcon.AddActionIcon(mapNo, position, iconOn, iconOff, onOpen, onCreated);
        }

        internal static void Init(bool insideStudio)
        {
            if (insideStudio) return;

            var hi = new Harmony(typeof(GameAPI).FullName);
            hi.PatchAll(typeof(Hooks));
            hi.PatchAll(typeof(CustomActionIcon));

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
                            KoikatuAPI.Logger.LogError(e);
                        }
                    }
                }
            };

            SceneManager.activeSceneChanged += (scene1, scene2) =>
            {
                var index = _newGameDetectionScenes.IndexOf(scene2.name);
                // detect forward and backward navigation
                if (index != -1 && scene1.name.IsNullOrWhiteSpace() && (
                    index == _newGameDetectionIndex + 1 || index == _newGameDetectionIndex - 1))
                {
                    _newGameDetectionIndex = index;
                    if (_newGameDetectionIndex + 1 == _newGameDetectionScenes.Count)
                    {
                        _newGameDetectionIndex = -1;
                        OnNewGame();
                        return;
                    }
                }
                else
                {
                    _newGameDetectionIndex = -1;
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
                    KoikatuAPI.Logger.LogError(e);
                }
            }

            try
            {
                GameLoad?.Invoke(KoikatuAPI.Instance, args);
            }
            catch (Exception e)
            {
                KoikatuAPI.Logger.LogError(e);
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
                    KoikatuAPI.Logger.LogError(e);
                }
            }

            try
            {
                GameSave?.Invoke(KoikatuAPI.Instance, args);
            }
            catch (Exception e)
            {
                KoikatuAPI.Logger.LogError(e);
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
                    KoikatuAPI.Logger.LogError(e);
                }
            }

            try
            {
                EndH?.Invoke(KoikatuAPI.Instance, EventArgs.Empty);
            }
            catch (Exception e)
            {
                KoikatuAPI.Logger.LogError(e);
            }

            InsideHScene = false;
        }

        private static IEnumerator OnHStart(HSceneProc proc)
        {
            InsideHScene = true;
            yield return null;
            foreach (var behaviour in _registeredHandlers)
            {
                try
                {
                    behaviour.Key.OnStartH(proc, proc.flags.isFreeH);
                }
                catch (Exception e)
                {
                    KoikatuAPI.Logger.LogError(e);
                }
            }

            try
            {
                StartH?.Invoke(KoikatuAPI.Instance, EventArgs.Empty);
            }
            catch (Exception e)
            {
                KoikatuAPI.Logger.LogError(e);
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
                    KoikatuAPI.Logger.LogError(e);
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
                    KoikatuAPI.Logger.LogError(e);
                }
            }
        }

        private static void OnNewGame()
        {
            foreach (var behaviour in _registeredHandlers)
            {
                try
                {
                    behaviour.Key.OnNewGame();
                }
                catch (Exception e)
                {
                    KoikatuAPI.Logger.LogError(e);
                }
            }
        }
    }
}
