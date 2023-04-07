using ExtensibleSaveFormat;
using KKAPI.Maker;
using KKAPI.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UniRx;
using UniRx.Triggers;
using UnityEngine;

#if AI || HS2
using AIChara;
#endif

namespace KKAPI.Chara
{
    /// <summary>
    /// Provides an easy way to add custom logic to all characters in the game and in studio. 
    /// It takes care of all the error-prone plumbing and lets you easily save and load data to the character cards.
    /// </summary>
    public static partial class CharacterApi
    {
        internal static readonly HashSet<ChaControl> ChaControls = new HashSet<ChaControl>();

        private readonly static SortedList<int, ControllerRegistration> _registeredHandlers = new SortedList<int, ControllerRegistration>();

        /// <summary>
        /// All currently registered kinds of <see cref="CharaCustomFunctionController"/> controllers.
        /// </summary>
        public static IEnumerable<ControllerRegistration> RegisteredHandlers => _registeredHandlers.Values;

        /// <summary>
        /// Override to supply custom extended data copying logic.
        /// By default copies all data under <code>ExtendedDataId</code> by reference.
        /// </summary>
        /// <param name="dst">Copy current character's ext data to this character</param>
        /// <param name="src">Current character to copy the data from</param>
        public delegate void CopyExtendedDataFunc(ChaFile dst, ChaFile src);

        /// <summary>
        /// Get ChaControl that is using the specified ChaFileControl.
        /// </summary>
        public static ChaControl FileControlToChaControl(ChaFileControl fileControl)
        {
            return ChaControls.Single(x => x.chaFile == fileControl);
        }

        /// <summary>
        /// Get all extra behaviours for specified character. If null, returns extra behaviours for all characters.
        /// </summary>
        public static IEnumerable<CharaCustomFunctionController> GetBehaviours(ChaControl character = null)
        {
            if (character == null)
                return _registeredHandlers.SelectMany(x => x.Value.Instances);

            return character.GetComponents<CharaCustomFunctionController>();
        }

        /// <summary>
        /// Get the first controller that was registered with the specified extendedDataId.
        /// </summary>
        public static ControllerRegistration GetRegisteredBehaviour(string extendedDataId)
        {
            return _registeredHandlers.Values.First(registration => string.Equals(registration.ExtendedDataId, extendedDataId, StringComparison.Ordinal));
        }

        /// <summary>
        /// Get the first controller of the specified type that was registered. The type has to be an exact match.
        /// </summary>
        public static ControllerRegistration GetRegisteredBehaviour(Type controllerType)
        {
            return _registeredHandlers.Values.First(registration => registration.ControllerType == controllerType);
        }

        /// <summary>
        /// Get the first controller of the specified type that was registered with the specified extendedDataId. The type has to be an exact match.
        /// </summary>
        public static ControllerRegistration GetRegisteredBehaviour(Type controllerType, string extendedDataId)
        {
            return _registeredHandlers.Values.First(registration => registration.ControllerType == controllerType && string.Equals(registration.ExtendedDataId, extendedDataId, StringComparison.Ordinal));
        }

        /// <summary>
        /// Register new functionality that will be automatically added to all characters (where applicable).
        /// Offers easy API for saving and loading extended data, and for running logic to apply it to the characters.
        /// All necessary hooking and event subscribing is done for you. All you have to do is create a type
        /// that inherits from <code>CharaExtraBehaviour</code> (don't make instances, the API will make them for you).
        /// </summary>
        /// <typeparam name="T">Type with your custom logic to add to a character</typeparam>
        /// <param name="extendedDataId">Extended data ID used by this behaviour. Set to null if not used. Needed to copy the data in some situations.</param>
        public static void RegisterExtraBehaviour<T>(string extendedDataId) where T : CharaCustomFunctionController, new()
        {
            RegisterExtraBehaviour<T>(extendedDataId, 1000);
        }

        /// <summary>
        /// Register new functionality that will be automatically added to all characters (where applicable).
        /// Offers easy API for saving and loading extended data, and for running logic to apply it to the characters.
        /// All necessary hooking and event subscribing is done for you. All you have to do is create a type
        /// that inherits from <code>CharaExtraBehaviour</code> (don't make instances, the API will make them for you).
        /// </summary>
        /// <typeparam name="T">Type with your custom logic to add to a character</typeparam>
        /// <param name="extendedDataId">Extended data ID used by this behaviour. Set to null if not used. Needed to copy the data in some situations.</param>
        /// <param name="priority">Default value is 1000, Decrease to increase priority or vice versa such on <see cref="CharaCustomFunctionController.OnReload(GameMode)"/> or related functions.</param>
        public static void RegisterExtraBehaviour<T>(string extendedDataId, int priority) where T : CharaCustomFunctionController, new()
        {
            void BasicCopier(ChaFile dst, ChaFile src)
            {
                var extendedData = ExtendedSave.GetExtendedDataById(src, extendedDataId);
                ExtendedSave.SetExtendedDataById(dst, extendedDataId, extendedData);
            }

            var copier = extendedDataId == null ? (CopyExtendedDataFunc)null : BasicCopier;

            RegisterExtraBehaviour<T>(extendedDataId, copier, priority);
        }


        /// <summary>
        /// Register new functionality that will be automatically added to all characters (where applicable).
        /// Offers easy API for saving and loading extended data, and for running logic to apply it to the characters.
        /// All necessary hooking and event subscribing is done for you. All you have to do is create a type
        /// that inherits from <code>CharaExtraBehaviour</code> (don't make instances, the API will make them for you).
        /// </summary>
        /// <typeparam name="T">Type with your custom logic to add to a character</typeparam>
        /// <param name="extendedDataId">Extended data ID used by this behaviour. Set to null if not used.</param>
        /// <param name="customDataCopier">Override default extended data copy logic</param>
        public static void RegisterExtraBehaviour<T>(string extendedDataId, CopyExtendedDataFunc customDataCopier) where T : CharaCustomFunctionController, new()
        {
            RegisterExtraBehaviour<T>(extendedDataId, customDataCopier, 1000);
        }


        /// <summary>
        /// Register new functionality that will be automatically added to all characters (where applicable).
        /// Offers easy API for saving and loading extended data, and for running logic to apply it to the characters.
        /// All necessary hooking and event subscribing is done for you. All you have to do is create a type
        /// that inherits from <code>CharaExtraBehaviour</code> (don't make instances, the API will make them for you).
        /// </summary>
        /// <typeparam name="T">Type with your custom logic to add to a character</typeparam>
        /// <param name="extendedDataId">Extended data ID used by this behaviour. Set to null if not used.</param>
        /// <param name="customDataCopier">Override default extended data copy logic</param>
        /// <param name="priority">Default value is 1000, Decrease to increase priority or vice versa such on <see cref="CharaCustomFunctionController.OnReload(GameMode)"/> or related functions.</param>
        public static void RegisterExtraBehaviour<T>(string extendedDataId, CopyExtendedDataFunc customDataCopier, int priority) where T : CharaCustomFunctionController, new()
        {
            while (_registeredHandlers.ContainsKey(priority))
            {
                priority++;
            }
            _registeredHandlers.Add(priority, new ControllerRegistration(typeof(T), extendedDataId, customDataCopier));
        }

        internal static void Init()
        {
            Hooks.InitHooks();

            // Cards -------------------------
            ExtendedSave.CardBeingSaved += OnCardBeingSaved;
#pragma warning disable 618
            MakerAPI.ChaFileLoaded += (sender, args) =>
#pragma warning restore 618
            {
                var chaControl = MakerAPI.GetCharacterControl();
                if (chaControl != null)
                {
#if AI || HS2
                    var chaFile = MakerAPI.Hooks.InternalLastLoadedChaFile;
                    KoikatuAPI.Instance.StartCoroutine(new object[] {
                        // Need to wait until clothes and accs are fully loaded
                        null, new WaitForEndOfFrame(),
                        CoroutineUtils.CreateCoroutine(() =>
                        {
                            // Needed to fix losing ext data because the chafile gets replaced by chafiles from the list being reloaded
                            MakerAPI.Hooks.InternalLastLoadedChaFile = chaFile;
                            ReloadChara(chaControl);
                        })
                    }.GetEnumerator());
#else
                    ReloadChara(chaControl);
#endif
                }
            };

            // Coordinates -------------------
            ExtendedSave.CoordinateBeingSaved += file =>
            {
                if (file == null) return;

                // Safe to assume we're in maker
                var character = MakerAPI.GetCharacterControl();
                if (character == null)
                {
                    KoikatuAPI.Logger.LogError("OnCoordinateBeingSaved fired outside chara maker for " + file.coordinateName);
                    KoikatuAPI.Logger.LogInfo(new StackTrace());
                    return;
                }

                OnCoordinateBeingSaved(character, file);
            };
            ExtendedSave.CoordinateBeingLoaded += file =>
            {
                if (Hooks.ClothesFileControlLoading || file == null) return;

                // Coord cards are loaded by loading into the character's nowCoordinate
                var cf = ChaControls.FirstOrDefault(x => x.nowCoordinate == file);
                if (cf != null)
                {
#if AI || HS2
                    KoikatuAPI.Instance.StartCoroutine(CoroutineUtils.CreateCoroutine(new WaitForEndOfFrame(), () => OnCoordinateBeingLoaded(cf, file)));
#else
                    OnCoordinateBeingLoaded(cf, file);
#endif
                }
            };

            if (KoikatuAPI.EnableDebugLogging)
                RegisterExtraBehaviour<TestCharaCustomFunctionController>(nameof(TestCharaCustomFunctionController));
        }

        private static void CreateOrAddBehaviours(ChaControl target)
        {
            foreach (var handler in _registeredHandlers)
            {
                var existing = target.gameObject.GetComponents(handler.Value.ControllerType)
                    .Cast<CharaCustomFunctionController>()
                    .FirstOrDefault(x => x.ExtendedDataId == handler.Value.ExtendedDataId);

                if (existing == null)
                {
                    try
                    {
                        handler.Value.CreateInstance(target);
                    }
                    catch (Exception e)
                    {
                        KoikatuAPI.Logger.LogError(e);
                    }
                }
            }

#if KK || KKS
            var controllers = target.GetComponents<CharaCustomFunctionController>();
            target.UpdateAsObservable().Subscribe(
                _ =>
                {
                    var changed = false;
                    var currentCoordinate = (ChaFileDefine.CoordinateType)target.fileStatus.coordinateType;
                    for (var i = 0; i < controllers.Length; i++)
                    {
                        var controllerSubject = controllers[i].CurrentCoordinate;

                        if (i == 0)
                        {
                            changed = currentCoordinate != controllerSubject.Value;
                            if (changed && KoikatuAPI.EnableDebugLogging)
                                KoikatuAPI.Logger.LogMessage(
                                    $"Changed coord of character {target.chaFile.GetFancyCharacterName()} to {currentCoordinate}");
                        }

                        if (changed) controllerSubject.OnNext(currentCoordinate);
                    }
                });
#endif

            // Trigger the equivalent of "Start" reload from controllers
            if (!MakerAPI.InsideMaker)
                target.StartCoroutine(CoroutineUtils.CreateCoroutine(new WaitForEndOfFrame(), () => { }, () => OnCharacterReloaded(target)));
        }

        private static void OnCardBeingSaved(ChaFile chaFile)
        {
            var gamemode = KoikatuAPI.GetCurrentGameMode();

#if HS2
            // Prevent overwriting character coordinates with whatever is loaded at the time of saving
            if (gamemode == GameMode.MainGame) //todo use for ai as well?
            {
                KoikatuAPI.Logger.LogDebug("Skipping sending the save event");
                return;
            }
#endif

            var chaControl = gamemode == GameMode.Maker ? MakerAPI.GetCharacterControl() : chaFile.GetChaControl();
            if (chaControl == null)
            {
#if KKS // todo probably should handle this in other games as well
                if (MainGame.GameAPI.GameBeingSaved)
                {
                    KoikatuAPI.Logger.LogWarning("Could not find chaControl responsible for this chaFile! The save event will NOT be triggered for " + GetLogName(chaFile));
                    return;
                }
#endif
                KoikatuAPI.Logger.LogWarning("Could not find chaControl responsible for this chaFile! Triggering the save event on all chaControls!");
            }

            OnCardBeingSaved(chaControl, gamemode);
        }

        private static void OnCardBeingSaved(ChaControl chaControl, GameMode gamemode)
        {
            KoikatuAPI.Logger.LogDebug("Character save: " + GetLogName(chaControl?.chaFile));

            foreach (var behaviour in GetBehaviours(chaControl))
                behaviour.OnCardBeingSavedInternal(gamemode);
        }

        private static readonly HashSet<ChaControl> _currentlyReloading = new HashSet<ChaControl>();

        private static void ReloadChara(ChaControl chaControl = null)
        {
            if (IsCurrentlyReloading(chaControl))
                return;

            if (chaControl == null)
                _currentlyReloading.UnionWith(ChaControls);
            else
                _currentlyReloading.Add(chaControl);

            KoikatuAPI.Logger.LogDebug("Character load/reload: " + GetLogName(chaControl?.chaFile));

            // Always send events to controllers before subscribers of CharacterReloaded
            var gamemode = KoikatuAPI.GetCurrentGameMode();
            foreach (var behaviour in GetBehaviours(chaControl))
                behaviour.OnReloadInternal(gamemode);

            OnCharacterReloaded(chaControl);

            if (MakerAPI.InsideAndLoaded)
                MakerAPI.OnReloadInterface(new CharaReloadEventArgs(chaControl));

            if (chaControl == null)
                _currentlyReloading.Clear();
            else
                _currentlyReloading.Remove(chaControl);
        }

        private static void OnCharacterReloaded(ChaControl chaControl)
        {
            var args = new CharaReloadEventArgs(chaControl);
            try
            {
                CharacterReloaded?.Invoke(null, args);
            }
            catch (Exception e)
            {
                KoikatuAPI.Logger.LogError(e);
            }
        }

        private static bool IsCurrentlyReloading(ChaControl chaControl)
        {
            return (chaControl == null && _currentlyReloading.Count > 0) || _currentlyReloading.Contains(chaControl);
        }

        private static IEnumerator DelayedReloadChara(ChaControl chaControl)
        {
            if (MakerAPI.InsideMaker || IsCurrentlyReloading(chaControl)) yield break;

            yield return null;
            ReloadChara(chaControl);
        }

        private static void OnCoordinateBeingSaved(ChaControl character, ChaFileCoordinate coordinateFile)
        {
#if KK || KKS
            KoikatuAPI.Logger.LogDebug($"Saving coord \"{coordinateFile.coordinateName}\" to chara \"{GetLogName(character?.chaFile)}\" / {(ChaFileDefine.CoordinateType)character.fileStatus.coordinateType}");
#else
            KoikatuAPI.Logger.LogDebug($"Saving coord \"{coordinateFile.coordinateName}\" to chara \"{GetLogName(character?.chaFile)}\"");
#endif

            foreach (var controller in GetBehaviours(character))
                controller.OnCoordinateBeingSavedInternal(coordinateFile);

            try
            {
                CoordinateSaving?.Invoke(null, new CoordinateEventArgs(character, coordinateFile));
            }
            catch (Exception e)
            {
                KoikatuAPI.Logger.LogError(e);
            }
        }

        private static void OnCoordinateBeingLoaded(ChaControl character, ChaFileCoordinate coordinateFile)
        {
#if KK || KKS
            KoikatuAPI.Logger.LogDebug($"Loading coord \"{coordinateFile.coordinateName}\" to chara \"{GetLogName(character?.chaFile)}\" / {(ChaFileDefine.CoordinateType)character.fileStatus.coordinateType}");
#else
            KoikatuAPI.Logger.LogDebug($"Loading coord \"{coordinateFile.coordinateName}\" to chara \"{GetLogName(character?.chaFile)}\"");
#endif

            foreach (var controller in GetBehaviours(character))
                controller.OnCoordinateBeingLoadedInternal(coordinateFile);

            var args = new CoordinateEventArgs(character, coordinateFile);
            try
            {
                CoordinateLoaded?.Invoke(null, args);
            }
            catch (Exception e)
            {
                KoikatuAPI.Logger.LogError(e);
            }

            if (MakerAPI.InsideAndLoaded)
                MakerAPI.OnReloadInterface(args);
        }

        private static string GetLogName(ChaFile chaFile)
        {
            return chaFile == null ? "NULL / ALL CHARACTERS" : $"{chaFile.GetFancyCharacterName()} from {(string.IsNullOrEmpty(chaFile.charaFileName) ? "Unknown" : chaFile.charaFileName)}";
        }

        /// <summary>
        /// Fired after all CharaCustomFunctionController have updated.
        /// </summary>
        public static event EventHandler<CharaReloadEventArgs> CharacterReloaded;

        /// <summary>
        /// Fired after a coordinate card was loaded and all controllers were updated.
        /// Not filed if the coordinate file was not loaded into a character (so not during list updates). 
        /// </summary>
        public static event EventHandler<CoordinateEventArgs> CoordinateLoaded;

        /// <summary>
        /// Fired just before a coordinate card is saved, but after all controllers wrote their data.
        /// </summary>
        public static event EventHandler<CoordinateEventArgs> CoordinateSaving;
    }
}
