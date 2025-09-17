using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ActionGame;
using ADV;
using BepInEx.Bootstrap;
using HarmonyLib;
using Illusion.Component;
using KKAPI.Studio;
using KKAPI.Utilities;
using Manager;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

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
        /// Triggered when the current day changes in story mode.
        /// Runs immediately after all <see cref="GameCustomFunctionController"/> objects trigger their events.
        /// </summary>
        public static event EventHandler<DayChangeEventArgs> DayChange;

        /// <summary>
        /// Triggered when the current time of the day changes in story mode.
        /// Runs immediately after all <see cref="GameCustomFunctionController"/> objects trigger their events.
        /// </summary>
        public static event EventHandler<PeriodChangeEventArgs> PeriodChange;

        /// <summary>
        /// Triggered when a new game is started in story mode.
        /// Runs immediately after all <see cref="GameCustomFunctionController"/> objects trigger their events.
        /// </summary>
        public static event EventHandler NewGame;

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

        /// <summary>
        /// Register a new action icon / action point in roaming mode (like the icons for training/studying, club report screen, peeping).
        /// Dispose the return value to remove the icon.
        /// Icon templates can be found here https://github.com/IllusionMods/IllusionModdingAPI/tree/master/src/KKSAPI/MainGame/ActionIcons
        /// </summary>
        /// <param name="mapNo">Identification number of the map the icon should be spawned on</param>
        /// <param name="position">Position of the icon. All default icons are spawned at y=0, but different heights work fine to a degree.
        /// You can figure out the position by walking to it and getting the player position with RUE.</param>
        /// <param name="icon">Icon shown at the top of the action point. It should be pure white, use the <paramref name="color"/> parameter to change the color instead.</param>
        /// <param name="color">Color of the action point.</param>
        /// <param name="popupText">Text shown at the bottom of the screen when player is close to the action point.</param>
        /// <param name="onOpen">Action triggered when player clicks the icon (If you want to open your own menu, use <see cref="GameExtensions.SetIsCursorLock"/>
        /// to enable mouse cursor and hide the action icon to prevent it from being clicked again.).</param>
        /// <param name="onCreated">Optional action to run after the icon is created.
        /// Use to attach extra code to the icon, e.g. by using <see cref="ObservableTriggerExtensions.UpdateAsObservable(Component)"/> and similar methods.</param>
        /// <param name="delayed">Should the icon be spawned every time the map is entered</param>
        /// <param name="immediate">Should the icon be spawned immediately if the player is on the correct map</param>
        public static IDisposable AddActionIcon(int mapNo, Vector3 position, Texture icon, Color color, string popupText, Action onOpen, Action<TriggerEnterExitEvent> onCreated = null, bool delayed = true, bool immediate = false)
        {
            if (StudioAPI.InsideStudio) return Disposable.Empty;
            return CustomActionIcon.AddActionIcon(mapNo, position, icon, color, popupText, onOpen, onCreated, delayed, immediate);
        }

        /// <summary>
        /// Register a new touch icon in talk scenes in roaming mode (like the touch and look buttons on top right when talking to a character).
        /// Dispose the return value to remove the icon.
        /// Icon templates can be found here https://github.com/IllusionMods/IllusionModdingAPI/tree/master/src/KKAPI/MainGame/TouchIcons
        /// By default this functions as a simple button. If you want to turn this into a toggle you have to manually switch button.image.sprite as needed.
        /// </summary>
        /// <param name="icon">Icon shown by default</param>
        /// <param name="onCreated">Action to run after the icon is created.
        /// Use to subscribe to the onClick event and/or attach extra code to the button, e.g. by using <see cref="ObservableTriggerExtensions.UpdateAsObservable(Component)"/> and similar methods.</param>
        /// <param name="row">Row of the button, counted from top at 0. Buttons are added from right to left. Row has to be between 0 and 5, but 0 to 2 are recommended.</param>
        /// <param name="order">Order of the buttons in a row. Lower value is placed more to the right. By default order of adding the icons is used.</param>
        public static IDisposable AddTouchIcon(Sprite icon, Action<Button> onCreated, int row = 1, int order = 0)
        {
            if (StudioAPI.InsideStudio) return Disposable.Empty;
            return CustomTalkSceneTouchIcon.AddTouchIcon(icon, onCreated, row, order);
        }
        /// <summary>
        /// Register a new conversation button in talk scenes in roaming mode.
        /// (the white buttons you get when you click on the buttons on the right when talking to someone, for example "Confess", "Excercise together")
        /// Dispose the return value to remove the button (It will not be created anymore and have to be re-added. If inside a talk scene the button will be removed immediately).
        /// </summary>
        /// <param name="text">Text of the new button.</param>
        /// <param name="onCreated">Action to run after the icon is created.
        /// Use to subscribe to the onClick event and/or attach extra code to the button, e.g. by using <see cref="ObservableTriggerExtensions.UpdateAsObservable(Component)"/> and similar methods.</param>
        /// <param name="targetMenu">Which submenu to put your button under (from the 3 buttons on right, only top and bottom buttons can be used here).</param>
        [Obsolete("Not implemented yet")]
        public static IDisposable AddTalkButton(string text, Action<Button> onCreated, TalkSceneActionKind targetMenu)
        {
            throw new NotImplementedException("Not implemented yet");
            //if (StudioAPI.InsideStudio) return Disposable.Empty;
            //return TalkSceneCustomButtons.AddTalkButton(text, onCreated, targetMenu);
        }

        internal static void Init(bool insideStudio)
        {
            if (insideStudio) return;

            var hi = new Harmony(typeof(GameAPI).FullName);
            Hooks.SetupHooks(hi);
            hi.PatchAll(typeof(CustomActionIcon));
            hi.PatchAll(typeof(CustomTalkSceneTouchIcon));
            //todo hi.PatchAll(typeof(TalkSceneCustomButtons));

            _functionControllerContainer = new GameObject("GameCustomFunctionController Zoo");
            _functionControllerContainer.transform.SetParent(Chainloader.ManagerObject.transform, false);

            SceneManager.sceneLoaded += (arg0, mode) =>
            {
                if (arg0.name == "HotelMyroom" && Manager.Scene.LoadSceneName != "H")
                {
                    foreach (var registeredHandler in _registeredHandlers)
                    {
                        try
                        {
                            registeredHandler.Key.OnEnterHotelMyroomMenu();
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

#if DEBUG
            //todo
            //AddTalkButton("talk button1", button => button.onClick.AddListener(() => KoikatuAPI.Logger.LogMessage("Hello world talk")), TalkSceneActionKind.Talk);
            //AddTalkButton("event button1", button => button.onClick.AddListener(() => KoikatuAPI.Logger.LogMessage("Hello world event1")), TalkSceneActionKind.Event);
            //AddTalkButton("event button2", button => button.onClick.AddListener(() => KoikatuAPI.Logger.LogMessage("Hello world event2")), TalkSceneActionKind.Event);
            //AddTalkButton("event button3", button => button.onClick.AddListener(() => KoikatuAPI.Logger.LogMessage("Hello world event3")), TalkSceneActionKind.Event);
            //IDisposable disp = null;
            //disp = AddTalkButton("commit sudoku", button => button.onClick.AddListener(disp.Dispose), TalkSceneActionKind.Talk);
#endif
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

        private static void OnHEnd(MonoBehaviour baseLoader)
        {
            var proc = baseLoader as HSceneProc;
            var flags = proc?.flags ?? UnityEngine.Object.FindObjectOfType<HFlag>();
            foreach (var behaviour in _registeredHandlers)
            {
                try
                {
                    behaviour.Key.OnEndH(baseLoader, flags, ReferenceEquals(proc, null));
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

        private static IEnumerator OnHStart(MonoBehaviour baseLoader)
        {
            InsideHScene = true;
            yield return null;
            var proc = baseLoader as HSceneProc;
            var flags = proc?.flags ?? UnityEngine.Object.FindObjectOfType<HFlag>();
            foreach (var behaviour in _registeredHandlers)
            {
                try
                {
                    behaviour.Key.OnStartH(baseLoader, flags, ReferenceEquals(proc, null));
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

            try
            {
                DayChange?.Invoke(KoikatuAPI.Instance, new DayChangeEventArgs(day));
            }
            catch (Exception e)
            {
                KoikatuAPI.Logger.LogError(e);
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

            try
            {
                PeriodChange?.Invoke(KoikatuAPI.Instance, new PeriodChangeEventArgs(period));
            }
            catch (Exception e)
            {
                KoikatuAPI.Logger.LogError(e);
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

            try
            {
                NewGame?.Invoke(KoikatuAPI.Instance, EventArgs.Empty);
            }
            catch (Exception e)
            {
                KoikatuAPI.Logger.LogError(e);
            }
        }

        /// <summary>
        /// Event data for day changes in main game.
        /// </summary>
        public class DayChangeEventArgs : EventArgs
        {
            /// <summary>
            /// Create a new instance
            /// </summary>
            public DayChangeEventArgs(Cycle.Week newDay)
            {
                NewDay = newDay;
            }
            /// <summary>
            /// Day that was changed to.
            /// </summary>
            public Cycle.Week NewDay { get; }
        }

        /// <summary>
        /// Event data for period changes within a day in main game.
        /// </summary>
        public class PeriodChangeEventArgs : EventArgs
        {
            /// <summary>
            /// Create a new instance
            /// </summary>
            public PeriodChangeEventArgs(Cycle.Type period)
            {
                NewPeriod = period;
            }
            /// <summary>
            /// Period that was changed to.
            /// </summary>
            public Cycle.Type NewPeriod { get; }
        }

        /// <summary>
        /// Gets the ActionControl instance if it's initialized, null otherwise
        /// </summary>
        public static ActionControl GetActionControl()
        {
#if KK
            return Manager.Game.IsInstance() ? Manager.Game.Instance.actScene?.actCtrl : null;
#elif KKS
            return ActionControl.initialized ? ActionControl.instance : null;
#endif
        }

        /// <summary>
        /// Gets the ADVScene instance if it's initialized, null otherwise
        /// </summary>
        public static ADVScene GetADVScene()
        {
#if KK
            return Manager.Game.IsInstance() ? Manager.Game.Instance.actScene?.advScene : null;
#elif KKS
            return ActionControl.initialized ? ActionControl.instance.actionScene?.AdvScene : null;
#endif
        }
        
        /// <summary>
        /// Gets the ActionScene instance if it's initialized, null otherwise
        /// </summary>
        public static ActionScene GetActionScene()
        {
#if KK
            return Manager.Game.IsInstance() ? Manager.Game.Instance.actScene : null;
#elif KKS
            return ActionControl.initialized ? ActionControl.instance.actionScene : null;
#endif
        }

        /// <summary>
        /// Gets the TalkScene instance if it's initialized, null otherwise
        /// </summary>
        public static TalkScene GetTalkScene()
        {
            return TalkScene.initialized ? TalkScene.instance : null;
        }

        /// <summary>
        /// Find heroine that is currently in focus (in a talk or adv scene, leading girl in H scene).
        /// null if not found.
        /// </summary>
        public static SaveData.Heroine GetCurrentHeroine()
        {
            var hFlag = UnityEngine.Object.FindObjectOfType<HFlag>();
            if (hFlag != null)
                return hFlag.GetLeadingHeroine();

            var advScene = GetADVScene();
            if (advScene != null)
            {
                if (advScene.nowScene is TalkScene s && s.targetHeroine != null)
                    return s.targetHeroine;
                if (advScene.Scenario != null && advScene.Scenario.currentHeroine != null)
                    return advScene.Scenario.currentHeroine;
            }

            var talkScene = GetTalkScene();
            if (talkScene != null)
            {
                var result = talkScene.targetHeroine;
                if (result != null) return result;
            }

            // In event
            if (Manager.Character.dictEntryChara?.Count > 0)
            {
                // Main event char is usually (not always) the first one.
                // Also will pull the currently visible character, like the teacher or the mom.
                return Manager.Character.dictEntryChara[0]?.GetHeroine();
            }

            return null;
        }
    }
}
